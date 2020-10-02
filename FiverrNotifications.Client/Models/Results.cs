using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class Results
    {
        [JsonProperty("rows")]
        public Row[] Rows { get; set; }

        [JsonProperty("thead")]
        public Head[] Head { get; set; }
    }
}
