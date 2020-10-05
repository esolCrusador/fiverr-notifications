using Newtonsoft.Json;

namespace FiverrNotifications.Client.Models
{
    public partial class Cell
    {
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("withText", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WithText { get; set; }

        [JsonProperty("userPict", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPict { get; set; }

        [JsonProperty("cssClass", NullValueHandling = NullValueHandling.Ignore)]
        public string CssClass { get; set; }

        [JsonProperty("hintBottom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HintBottom { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public Tag[] Tags { get; set; }

        [JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
        public object Attachment { get; set; }

        [JsonProperty("alignCenter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AlignCenter { get; set; }

        [JsonProperty("actionVisible", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ActionVisible { get; set; }

        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public Button[] Buttons { get; set; }
    }
}
