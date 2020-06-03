using Newtonsoft.Json;

namespace Locutius.Common.Models.Speech
{
    public class NBest
    {
        /// <summary>
        /// The confidence score of the entry from 0.0 (no confidence) to 1.0 (full confidence).
        /// </summary>
        [JsonProperty("Confidence")]
        public double Confidence { get; set; }

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

        /// <summary>
        /// Sentiment. Not used at the moment.
        /// </summary>
        [JsonProperty("Sentiment")]
        public Sentiment Sentiment { get; set; }

        /// <summary>
        /// Words. Not used at the moment.
        /// </summary>
        [JsonProperty("Words")]
        public Word[] Words { get; set; }
    }
}
