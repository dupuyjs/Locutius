using Newtonsoft.Json;

namespace Locutius.Common.Models.Speech
{
    public class RootObject
    {
        [JsonProperty("AudioFileResults")]
        public AudioFileResult[] AudioFileResults { get; set; }

        public static RootObject FromJson(string json) => JsonConvert.DeserializeObject<RootObject>(json);
    }

    public static class Serialize
    {
        public static string ToJson(this RootObject self) => JsonConvert.SerializeObject(self, Formatting.Indented);
    }
}
