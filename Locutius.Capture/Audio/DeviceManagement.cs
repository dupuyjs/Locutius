using NAudio.CoreAudioApi;
using System;

namespace Locutius.Capture.Audio
{
    /// <summary>
    /// The <c>DeviceManagement</c> class is used to get devices (audio endpoints) information.
    /// </summary>
    public static class DeviceManagement
    {
        /// <summary>
        /// This method displays all devices (audio endpoints).
        /// </summary>
        public static void DisplayAllDevices()
        {
            DisplayDevices(DataFlow.Capture);
            DisplayDevices(DataFlow.Render);
        }

        /// <summary>
        /// This method displays devices (audio endpoints) that meet the specified <c>DataFlow</c>.
        /// </summary>
        /// <param name="dataFlow"><c>DataFlow</c> enumeration.</param>
        public static void DisplayDevices(DataFlow dataFlow)
        {
            using var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(dataFlow, DeviceState.Active);
            var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Console);

            Console.WriteLine($"{dataFlow} available devices:");

            if (devices.Count > 0)
                foreach (var d in devices)
                {
                    Console.WriteLine($"Name: {d.FriendlyName} ID: {d.ID}");
                }
            else
                Console.WriteLine($"No {dataFlow} device found");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"(default) Name: {device.FriendlyName} ID: {device.ID}\n");
            Console.ResetColor();
        }

        /// <summary>
        /// This method retrieves an audio endpoint device that is identified by an endpoint device ID string.
        /// </summary>
        /// <param name="deviceId">string containing the endpoint device ID.</param>
        /// <returns><c>MMDevice</c> instance.</returns>
        public static MMDevice GetDevice(string deviceId)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

                foreach (var d in devices)
                {
                    if (d.ID == deviceId)
                        return d;
                }
            }

            return null;
        }
    }
}
