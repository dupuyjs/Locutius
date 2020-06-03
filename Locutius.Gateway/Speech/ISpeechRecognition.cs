using Locutius.Common.Models;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Threading.Tasks;

namespace Locutius.Gateway.Speech
{
    public interface ISpeechRecognition
    {
        public PushAudioInputStream PushStream { get; }
        public Task StartContinuousRecognitionAsync(string conversationId, SpeakerType speakerType, DateTime startTime);
        public Task StopContinuousRecognitionAsync();
    }
}
