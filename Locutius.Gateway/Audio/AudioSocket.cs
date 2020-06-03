using Locutius.Common.Helpers;
using Locutius.Common.Models;
using Locutius.Gateway.Helpers;
using Locutius.Gateway.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Locutius.Gateway.Audio
{
    public class AudioSocket : IAudioSocket
    {
        private readonly IConfiguration config;
        private readonly ISpeechRecognition speech;
        private readonly ILogger logger;

        public AudioSocket(IConfiguration config, ISpeechRecognition speech, ILogger<AudioSocket> logger)
        {
            this.speech = speech;
            this.config = config;
            this.logger = logger;
        }

        public async Task ReceiveAsync(WebSocket socket, string conversationId, SpeakerType speakerType)
        {
            // PCM format, 16000 samples per second, 16 bits per sample, 1 channel (mono)
            var outFormat = new WaveFormat(16000, 16, 1);
            var localDirectory = Environment.GetEnvironmentVariable("LocalAppData");
            var outFilePath = Path.Combine(localDirectory, $"{Guid.NewGuid()}.wav");
            var startTime = DateTime.Now;

            using (var outFileWriter = new WaveFileWriter(outFilePath, outFormat))
            {
                await speech.StartContinuousRecognitionAsync(conversationId, speakerType, startTime).ConfigureAwait(false);

                var socketBuffer = new byte[Settings.ReceiveBufferSize];

                if (socket != null)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(socketBuffer), CancellationToken.None).ConfigureAwait(false);

                    while (!result.CloseStatus.HasValue)
                    {
                        outFileWriter.Write(socketBuffer, 0, result.Count);

                        if (result.Count > 0)
                            speech.PushStream.Write(socketBuffer, result.Count);

                        result = await socket.ReceiveAsync(new ArraySegment<byte>(socketBuffer), CancellationToken.None).ConfigureAwait(false); ;
                    }

                    await speech.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);
                }

                outFileWriter.Close();
            }

            try
            {
                if (File.Exists(outFilePath))
                {
                    var culture = CultureInfo.InvariantCulture;
                    var timestamp = startTime.ToString("s", culture);

                    var blobName = $"{conversationId}/{conversationId}-{culture.TextInfo.ToLower(speakerType.ToString())}-{timestamp}.wav";
                    await AzureStorageHelper.UploadAudioFileAsync(outFilePath, config["Azure.Storage.ConnectionString"], config["Azure.Storage.Container.Audio"], blobName).ConfigureAwait(false);
                    File.Delete(outFilePath);

                    logger.LogInformation($"Successfully uploaded audio file for {conversationId}:{speakerType.ToString()}.");
                }
            }
            catch (IOException ex)
            {
                logger.LogError(ex, $"Issue when uploading (or deleting) audio file for {conversationId}:{speakerType.ToString()}.");
            };
        }
    }
}
