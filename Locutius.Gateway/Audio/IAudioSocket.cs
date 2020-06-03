using Locutius.Common.Models;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Locutius.Gateway.Audio
{
    public interface IAudioSocket
    {
        public Task ReceiveAsync(WebSocket socket, string conversationId, SpeakerType speakerType);
    }
}
