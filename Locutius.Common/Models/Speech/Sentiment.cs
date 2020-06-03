using Newtonsoft.Json;

namespace Locutius.Common.Models.Speech
{
    public class Sentiment
    {
        [JsonProperty("Negative")]
        public double Negative { get; set; }

        [JsonProperty("Neutral")]
        public double Neutral { get; set; }

        [JsonProperty("Positive")]
        public double Positive { get; set; }
    }
}
