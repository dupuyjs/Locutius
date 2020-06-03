using Microsoft.Extensions.Configuration;

namespace Locutius.Capture.Configuration
{
    public static class CaptureConfiguration
    {
        public static IConfiguration Configuration { get; set; }

        public static void Init()
        {
            Configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();
        }
    }
}
