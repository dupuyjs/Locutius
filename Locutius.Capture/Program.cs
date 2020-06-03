using CommandLine;
using Locutius.Capture.Audio;
using Locutius.Capture.Configuration;
using Locutius.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Locutius.Capture
{
    static class Program
    {
        static CaptureFactory loopback;
        static CaptureFactory microphone;
        static bool captureStopped = false;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
        static async Task Main(string[] args)
        {
            bool debugMode = false;
            Uri socketUri = null;

            CaptureConfiguration.Init();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    socketUri = options.Uri == null ? new Uri(CaptureConfiguration.Configuration["Piscato.Gateway.Endpoint"]) : options.Uri;
                    debugMode = options.Debug;
                });

            Console.WriteLine("Start recording");
            DeviceManagement.DisplayAllDevices();

            var conversationId = Guid.NewGuid().ToString();
            await StartCaptureAsync(socketUri, conversationId, debugMode).ConfigureAwait(false);

            StopCapture();
            Console.WriteLine("End of recording");
            Cleanup();

            Console.CancelKeyPress += (s, e) =>
            {
                Exit();
            };

            Console.ReadLine();
            Exit();
        }

        /// <summary>
        /// This method starts capture process for microphone and loopback.
        /// </summary>
        /// <param name="socketUri">websocket uri of the audio gateway</param>
        /// <param name="conversationId">the conversation identifier</param>
        /// <param name="debugMode">enable or disable the debug mode</param>
        /// <returns></returns>
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        static async Task StartCaptureAsync(Uri socketUri, string conversationId, bool debugMode)
        {
            loopback = new CaptureFactory()
            {
                SocketUri = socketUri,
                DeviceType = DeviceType.Loopback,
                ConversationId = conversationId
            };
            await loopback.InitializeAsync(debugMode).ConfigureAwait(false);

            microphone = new CaptureFactory()
            {
                SocketUri = socketUri,
                DeviceType = DeviceType.Microphone,
                ConversationId = conversationId
            };
            await microphone.InitializeAsync(debugMode).ConfigureAwait(false);

            loopback.StartRecording();
            microphone.StartRecording();

            captureStopped = false;
        }

        /// <summary>
        /// This method stops microphone and loopback capture.
        /// </summary>
        private static void StopCapture()
        {
            if (captureStopped)
                return;

            var tscLoopback = loopback?.StopRecording();
            var tscMicrophone = microphone?.StopRecording();

            // Ensure we really stop the audio capture before to return.
            var allTask = new List<Task>();
            if (tscLoopback != null)
                allTask.Add(tscLoopback);
            if (tscMicrophone != null)
                allTask.Add(tscMicrophone);
            Task.WaitAll(allTask.ToArray());

            captureStopped = true;
        }

        /// <summary>
        /// This method cleanup microphone and loopback resources.
        /// </summary>
        private static void Cleanup()
        {
            loopback?.Dispose();
            microphone?.Dispose();
        }

        /// <summary>
        /// This method stops capture and cleanup all resources.
        /// </summary>
        private static void Exit()
        {
            StopCapture();
            Cleanup();
        }
    }
}
