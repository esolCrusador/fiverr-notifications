using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public class Tag
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
