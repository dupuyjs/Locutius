using Locutius.Common.Models;
using Locutius.Gateway.Audio;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Locutius.Gateway.Middleware
{
    public class SocketMiddleware
    {
        private readonly RequestDelegate next;

        public SocketMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IAudioSocket audio)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (audio == null)
                throw new ArgumentNullException(nameof(audio));

            if (context.WebSockets.IsWebSocketRequest)
            {
                var conversationId = context.Request.Headers["ConversationId"].ToString();
                var speaker = context.Request.Headers["SpeakerType"].ToString();
                var speakerType = (SpeakerType)Enum.Parse(typeof(SpeakerType), speaker);

                using var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(true);
                await audio.ReceiveAsync(socket, conversationId, speakerType).ConfigureAwait(false);
            }
            else
            {
                await next(context).ConfigureAwait(false);
            }
        }
    }
}
