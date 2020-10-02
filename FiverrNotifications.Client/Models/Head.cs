using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class Head
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
