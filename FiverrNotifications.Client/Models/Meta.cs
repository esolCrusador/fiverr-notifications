using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class Meta
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("isProfessional")]
        public bool IsProfessional { get; set; }

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public long? Category { get; set; }

        [JsonProperty("subCategory", NullValueHandling = NullValueHandling.Ignore)]
        public long? SubCategory { get; set; }

        [JsonProperty("requestText", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestText { get; set; }

        [JsonProperty("userPict", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPict { get; set; }

        [JsonProperty("buyerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? BuyerId { get; set; }
    }
}
