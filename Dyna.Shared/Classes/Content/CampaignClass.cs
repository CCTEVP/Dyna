using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Content
{
    public class CampaignClass
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("starts")]
        public string StartDate { get; set; }

        [JsonPropertyName("ends")]
        public string EndDate { get; set; }

        [JsonPropertyName("created")]
        public DateTime CreateDateTime { get; set; }

        [JsonPropertyName("updated")]
        public DateTime UpdatedDateTime { get; set; }

        [JsonPropertyName("creatives")]
        public List<CreativeClass> Creatives { get; set; }
    }
}
