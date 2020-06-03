using System;
using Locutius.Common.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Locutius.Common.Models.Speech
{
    public class SegmentResult
    {
        [JsonProperty("RecognitionStatus")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RecognitionStatus? RecognitionStatus { get; set; }

        /// <summary>
        /// Channel number. Not used at the moment.
        /// </summary>
        [JsonProperty("ChannelNumber")]
        public string ChannelNumber { get; set; }

        /// <summary>
        /// Speaker Identifier. Not used at the moment.
        /// </summary>
        [JsonProperty("SpeakerId")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long SpeakerId { get; set; }

        /// <summary>
        /// The time (in 100-nanosecond units) at which the recognized speech begins in the audio stream.
        /// </summary>
        [JsonProperty("Offset")]
        public long Offset { get; set; }

        /// <summary>
        /// The duration (in 100-nanosecond units) of the recognized speech in the audio stream.
        /// </summary>
        [JsonProperty("Duration")]
        public long Duration { get; set; }

        /// <summary>
        /// The time (in second units) at which the recognized speech begins in the audio stream.
        /// </summary>
        [JsonProperty("OffsetInSeconds")]
        public double OffsetInSeconds { get; set; }

        /// <summary>
        /// The duration (in second units) of the recognized speech in the audio stream.
        /// </summary>
        [JsonProperty("DurationInSeconds")]
        public double DurationInSeconds { get; set; }

        [JsonProperty("NBest")]
        public NBest[] NBest { get; set; }

        /// <summary>
        /// TimeStamp when Speech Completed
        /// </summary>
        [JsonProperty("SpeechCompletedTimeStamp")]
        public DateTime SpeechCompletedTimeStamp { get; set; }
    }
}
