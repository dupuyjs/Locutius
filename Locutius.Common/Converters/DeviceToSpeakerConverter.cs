using Locutius.Common.Models;
using System;

namespace Locutius.Common.Converters
{
    public static class DeviceToSpeakerConverter
    {
        public static SpeakerType Convert(DeviceType deviceType)
        {
            if (deviceType == DeviceType.Microphone)
                return SpeakerType.Advisor;
            if (deviceType == DeviceType.Loopback)
                return SpeakerType.Customer;

            throw new NotSupportedException("DeviceType was not Microphone or Loopback.");
        }
    }
}
