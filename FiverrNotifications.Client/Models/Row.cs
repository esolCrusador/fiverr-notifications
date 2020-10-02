using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class Row
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("cells")]
        public Cell[] Cells { get; set; }
    }
}
