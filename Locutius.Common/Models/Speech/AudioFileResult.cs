using Newtonsoft.Json;

namespace Locutius.Common.Models.Speech
{
    public class AudioFileResult
    {
        /// <summary>
        /// Audio file name.
        /// </summary>
        [JsonProperty("AudioFileName")]
        public string AudioFileName { get; set; }

        /// <summary>
        /// Audio file url.
        /// </summary>
        [JsonProperty("AudioFileUrl")]
        public string AudioFileUrl { get; set; }

        /// <summary>
        /// Audio length in seconds.
        /// </summary>
        [JsonProperty("AudioLengthInSeconds")]
        public double AudioLengthInSeconds { get; set; }

        /// <summary>
        /// Combined results.
        /// </summary>
        [JsonProperty("CombinedResults")]
        public CombinedResult[] CombinedResults { get; set; }

        /// <summary>
        /// Segment results.
        /// </summary>
        [JsonProperty("SegmentResults")]
        public SegmentResult[] SegmentResults { get; set; }
    }
}
