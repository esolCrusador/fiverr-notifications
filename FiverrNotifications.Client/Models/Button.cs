using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class Button
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public Meta Meta { get; set; }
    }
}
