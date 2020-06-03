using System;

namespace Locutius.Gateway.Helpers
{
    public static class Settings
    {
        // Internal default settings for web sockets usage, no reason at the moment to expose them in app settings.
        public static TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(120);
        public static int ReceiveBufferSize => 4 * 1024;
    }
}
