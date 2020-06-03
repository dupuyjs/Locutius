using Newtonsoft.Json;

namespace Locutius.Common.Models.Speech
{
    public class Word
    {
        [JsonProperty("Word")]
        public string WordWord { get; set; }

        [JsonProperty("Offset")]
        public long Offset { get; set; }

        [JsonProperty("Duration")]
        public long Duration { get; set; }

        [JsonProperty("OffsetInSeconds")]
        public double OffsetInSeconds { get; set; }

        [JsonProperty("DurationInSeconds")]
        public double DurationInSeconds { get; set; }
    }
}
