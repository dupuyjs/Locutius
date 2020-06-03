using Locutius.Common.Helpers;
using Locutius.Common.Models;
using Locutius.Common.Models.Speech;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Locutius.Gateway.Speech
{
    public class SpeechRecognition : ISpeechRecognition, IDisposable
    {
        private readonly AudioConfig audioConfig;
        private readonly SpeechConfig speechConfig;
        private readonly SpeechRecognizer speechRecognizer;
        private readonly List<SegmentResult> listSegment;
        private bool disposed = false;

        private IConfiguration Config { get; set; }
        private ILogger Logger { get; set; }
        public PushAudioInputStream PushStream { get; private set; }

        private string ConversationId { get; set; }
        private SpeakerType SpeakerType { get; set; }
        private DateTime StartTime { get; set; }

        public SpeechRecognition(IConfiguration config, ILogger<SpeechRecognition> logger)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.Logger = logger;
            this.Config = config;

            var speechKey = config["Azure.Cognitive.Speech.Key"];
            var speechRegion = config["Azure.Cognitive.Speech.Region"];
            var speechLanguage = config["Azure.Cognitive.Speech.Language"];
            var speechEndpointId = config["Azure.Cognitive.Speech.EndpointId"];

            listSegment = new List<SegmentResult>();

            speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

            if (speechConfig != null)
            {
                speechConfig.SpeechRecognitionLanguage = speechLanguage;
                speechConfig.OutputFormat = OutputFormat.Detailed;

                if (!string.IsNullOrEmpty(speechEndpointId))
                    speechConfig.EndpointId = speechEndpointId;

                PushStream = AudioInputStream.CreatePushStream();
                audioConfig = AudioConfig.FromStreamInput(PushStream);
                speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            }

            speechRecognizer.Recognizing += (s, e) =>
            {
                Logger.LogDebug($"Recognizing: Text={e.Result.Text}");
            };

            speechRecognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Building transcription.
                    var listBest = new List<NBest>();
                    foreach (var best in e.Result.Best())
                    {
                        listBest.Add(new NBest()
                        {
                            Confidence = best.Confidence,
                            Lexical = best.LexicalForm,
                            Itn = best.NormalizedForm,
                            MaskedItn = best.MaskedNormalizedForm,
                            Display = best.Text,
                            Sentiment = null,
                            Words = null
                        });
                    }

                    var segment = new SegmentResult()
                    {
                        ChannelNumber = null,
                        SpeakerId = (long)SpeakerType,
                        Offset = e.Result.OffsetInTicks,
                        Duration = e.Result.Duration.Ticks,
                        OffsetInSeconds = new TimeSpan(e.Result.OffsetInTicks).TotalSeconds,
                        DurationInSeconds = e.Result.Duration.TotalSeconds,
                        SpeechCompletedTimeStamp = DateTime.Now,
                        NBest = listBest.ToArray()
                    };

                    listSegment.Add(segment);
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    Logger.LogDebug($"NoMatch: Speech could not be recognized.");
                }
            };

            speechRecognizer.Canceled += (s, e) =>
            {
                Logger.LogInformation($"Canceled: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Logger.LogDebug($"Canceled: ErrorCode={e.ErrorCode}");
                    Logger.LogDebug($"Canceled: ErrorDetails={e.ErrorDetails}");
                    Logger.LogDebug($"Canceled: Did you update the subscription info?");
                }
            };

            speechRecognizer.SessionStarted += (s, e) =>
            {
                Logger.LogInformation($"Session started for {ConversationId}:{SpeakerType.ToString()}");
            };

            speechRecognizer.SessionStopped += (s, e) =>
            {
                Logger.LogInformation($"Session stopped for {ConversationId}:{SpeakerType.ToString()}");
                Logger.LogInformation("Stop recognition.");
            };
        }

        public async Task StartContinuousRecognitionAsync(string conversationId, SpeakerType speakerType, DateTime startTime)
        {
            ConversationId = conversationId;
            SpeakerType = speakerType;
            StartTime = startTime;

            await speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        }

        public async Task StopContinuousRecognitionAsync()
        {
            await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            Logger.LogInformation($"Session stopped for {ConversationId}:{SpeakerType.ToString()}");
            await OnCompletedSpeechAsync().ConfigureAwait(false);
        }

        public async Task OnCompletedSpeechAsync()
        {
            var culture = CultureInfo.InvariantCulture;
            var timestamp = StartTime.ToString("s", culture);
            var audioFileName = $"{ConversationId}/{ConversationId}-{culture.TextInfo.ToLower(SpeakerType.ToString())}-{timestamp}.wav";

            var audioFileResult = new AudioFileResult()
            {
                AudioFileName = audioFileName,
                AudioFileUrl = audioFileName,
                SegmentResults = listSegment.ToArray()
            };

            var json = new RootObject()
            {
                AudioFileResults = new AudioFileResult[] {
                    audioFileResult
                }
            }.ToJson();

            var localDirectory = Environment.GetEnvironmentVariable("LocalAppData");
            var outFilePath = Path.Combine(localDirectory, $"{Guid.NewGuid()}.json");

            try
            {
                File.WriteAllText(outFilePath, json);

                if (File.Exists(outFilePath))
                {
                    var blobName = $"{ConversationId}/{ConversationId}-{culture.TextInfo.ToLower(SpeakerType.ToString())}-{timestamp}.json";
                    await AzureStorageHelper.UploadTranscriptFileAsync(outFilePath, Config["Azure.Storage.ConnectionString"], Config["Azure.Storage.Container.Transcript"], blobName).ConfigureAwait(false);
                    File.Delete(outFilePath);

                    Logger.LogInformation($"Successfully uploaded transcript file for {ConversationId}:{SpeakerType.ToString()}.");
                }
            }
            catch (IOException ex)
            {
                Logger.LogError(ex, $"Issue when uploading (or deleting) transcript file for {ConversationId}:{SpeakerType.ToString()}.");
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool cleanup)
        {
            if (disposed)
                return;

            if (cleanup)
            {
                speechRecognizer.Dispose();
                audioConfig.Dispose();
                PushStream.Dispose();
            }

            disposed = true;
        }
    }
}
