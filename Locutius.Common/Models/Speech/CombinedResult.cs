using Newtonsoft.Json;

namespace Locutius.Common.Models.Speech
{
    public class CombinedResult
    {
        /// <summary>
        /// Channel number.
        /// </summary>
        [JsonProperty("ChannelNumber")]
        public string ChannelNumber { get; set; }

        /// <summary>
        /// The lexical form of the recognized text: the actual words recognized.
        /// </summary>
        [JsonProperty("Lexical")]
        public string Lexical { get; set; }

        /// <summary>
        /// The inverse-text-normalized ("canonical") form of the recognized text, with phone numbers, numbers, abbreviations ("doctor smith" to "dr smith"), and other transformations applied.
        /// </summary>
        [JsonProperty("ITN")]
        public string Itn { get; set; }

        /// <summary>
        /// The ITN form with profanity masking applied, if requested.
        /// </summary>
        [JsonProperty("MaskedITN")]
        public string MaskedItn { get; set; }

        /// <summary>
        /// The display form of the recognized text, with punctuation and capitalization added. This parameter is the same as DisplayText provided when format is set to simple.
        /// </summary>
        [JsonProperty("Display")]
        public string Display { get; set; }
    }
}
