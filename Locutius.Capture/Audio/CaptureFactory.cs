using Locutius.Capture.Providers;
using Locutius.Common.Converters;
using Locutius.Common.Models;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Locutius.Capture.Audio
{
    public class CaptureFactory : IDisposable
    {
        private MMDevice device;
        private WasapiCapture capture;
        private WaveFileWriter inFileWriter;
        private WaveFileWriter outFileWriter;
        private WaveFormat outFormat;
        private StreamSampleProvider audioIeee;
        private StereoToMonoSampleProvider audioMono;
        private WdlResamplingSampleProvider audioResampling;
        private SampleToWaveProvider16 audioPcm;
        private ClientWebSocket socket;
        private TaskCompletionSource<int> taskCompletionSource;
        private bool disposed = false;

        public Uri SocketUri { get; set; }
        public DeviceType DeviceType { get; set; }
        public string DeviceId { get; set; }
        public string ConversationId { get; set; }


        public CaptureFactory()
        {
            // PCM format, 16000 samples per second, 16 bits per sample, 1 channel (mono)
            outFormat = new WaveFormat(16000, 16, 1);
            taskCompletionSource = new TaskCompletionSource<int>();
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
                socket?.Dispose();
                audioIeee?.Dispose();
                inFileWriter?.Dispose();
                outFileWriter?.Dispose();
                capture?.Dispose();
                device?.Dispose();
            }

            disposed = true;
        }

        /// <summary>
        /// This method starts the recording session.
        /// </summary>
        public void StartRecording()
        {
            capture?.StartRecording();
        }

        /// <summary>
        /// This method starts the recording session.
        /// </summary>
        public Task<int> StopRecording()
        {
            capture?.StopRecording();
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// This method initializes audio capture and socket connection.
        /// </summary>
        /// <param name="debugMode">enable or disable the debug mode.</param>
        public async Task InitializeAsync(bool debugMode = true)
        {
            if ((!string.IsNullOrEmpty(DeviceId)) && ((device = DeviceManagement.GetDevice(DeviceId)) != null))
            {
                if (DeviceType == DeviceType.Loopback)
                    capture = new WasapiLoopbackCapture(device);
                else if (DeviceType == DeviceType.Microphone)
                    capture = new WasapiCapture(device);
            }
            else
            {
                if (DeviceType == DeviceType.Loopback)
                    capture = new WasapiLoopbackCapture();
                else if (DeviceType == DeviceType.Microphone)
                    capture = new WasapiCapture();
            }

            if (debugMode)
            {
                var culture = CultureInfo.InvariantCulture;

                var inFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{ConversationId}-{culture.TextInfo.ToLower(DeviceType.ToString())}-raw.wav");
                inFileWriter = new WaveFileWriter(inFilePath, capture.WaveFormat);

                var outFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{ConversationId}-{culture.TextInfo.ToLower(DeviceType.ToString())}-out.wav");
                outFileWriter = new WaveFileWriter(outFilePath, outFormat);

                DisplayWaveFormat(capture.WaveFormat);
                Console.WriteLine();
            }

            audioIeee = new StreamSampleProvider(capture.WaveFormat);
            audioMono = new StereoToMonoSampleProvider(audioIeee);
            audioResampling = new WdlResamplingSampleProvider(audioMono, 16000);
            audioPcm = new SampleToWaveProvider16(audioResampling);

            try
            {
                socket = new ClientWebSocket();
                socket.Options.SetRequestHeader("ConversationId", ConversationId);
                socket.Options.SetRequestHeader("SpeakerType", DeviceToSpeakerConverter.Convert(DeviceType).ToString());
                await socket.ConnectAsync(SocketUri, CancellationToken.None).ConfigureAwait(false);
                if (socket.State == WebSocketState.Open)
                {
                    Console.WriteLine($"Successfully connected to {SocketUri}.");
                }
            }
            catch (AggregateException e)
            {
                Console.WriteLine($"Failed to connect to {SocketUri}.");
                Console.WriteLine(e.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                throw;
            }

            capture.DataAvailable += async (s, a) =>
            {
                Console.WriteLine($"Captured {a.BytesRecorded} bytes on {DeviceType}.");

                if (socket.State == WebSocketState.Open)
                {
                    var data = ResampleAudioInput(a.Buffer, a.BytesRecorded, debugMode);

                    try
                    {
                        await socket.SendAsync(data, WebSocketMessageType.Binary, false, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine($"Exception on SendAsync: {ex.Message}");
                    }
                }
            };

            capture.RecordingStopped += async (s, a) =>
            {
                Console.WriteLine($"Recording stopped on {DeviceType}.");

                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Recording Stopped", CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine($"Exception on CloseAsync: {ex.Message}");
                    }

                    Console.WriteLine($"Connection closed.");
                    taskCompletionSource.SetResult(0);
                }
            };
        }

        /// <summary>
        /// This method displays wave format details.
        /// </summary>
        /// <param name="format">WaveFormat instance.</param>
        public static void DisplayWaveFormat(WaveFormat format)
        {
            if (format != null)
            {
                Console.WriteLine($"Sample Rate: {format.SampleRate}");
                Console.WriteLine($"Channels: {format.Channels}");
                Console.WriteLine($"Bit Depth: {format.BitsPerSample}");
                Console.WriteLine($"Block Align: {format.BlockAlign}");
                Console.WriteLine($"Average Bytes per Second: {format.AverageBytesPerSecond}");
                Console.WriteLine($"Encoding: {format.Encoding}");
            }
        }

        private byte[] ResampleAudioInput(byte[] audioBuffer, int audioBytesCount, bool debugMode)
        {
            if (debugMode)
                inFileWriter.Write(audioBuffer, 0, audioBytesCount);

            audioIeee.Write(audioBuffer, 0, audioBytesCount);

            var numBytes = ((float)audioBytesCount / 4 / capture.WaveFormat.SampleRate) * outFormat.SampleRate;
            byte[] outBuffer = new byte[Convert.ToInt32(Math.Ceiling(numBytes))];
            var outBytesCount = audioPcm.Read(outBuffer, 0, outBuffer.Length);

            if (debugMode)
                outFileWriter.Write(outBuffer, 0, outBytesCount);

            return outBuffer;
        }
    }
}
