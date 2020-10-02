using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class FiverrResponse
    {
        [JsonProperty("results")]
        public Results Results { get; set; }

        [JsonProperty("subcat_id_filter")]
        public object SubcategoryIdFilter { get; set; }

        [JsonProperty("current_filter")]
        public string CurrentFilter { get; set; }

        [JsonProperty("current_filter_title")]
        public string CurrentFilterTitle { get; set; }

        [JsonProperty("search_term")]
        public object SearchTerm { get; set; }
    }
}
