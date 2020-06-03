using Locutius.Gateway.Audio;
using Locutius.Gateway.Helpers;
using Locutius.Gateway.Middleware;
using Locutius.Gateway.Speech;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Locutius.Gateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISpeechRecognition, SpeechRecognition>();
            services.AddScoped<IAudioSocket, AudioSocket>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = Settings.KeepAliveInterval,
                ReceiveBufferSize = Settings.ReceiveBufferSize
            };

            app.UseWebSockets(webSocketOptions);

            app.UseMiddleware<SocketMiddleware>();
        }
    }
}
