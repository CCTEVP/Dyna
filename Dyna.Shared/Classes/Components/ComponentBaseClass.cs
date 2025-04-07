using Dyna.Shared.Classes.Content;
using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Components
{
    public class ComponentBaseClass
    {
        [JsonPropertyName("_id")]
        public string id { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("depth")]
        public string Depth { get; set; }

        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class ComponentLayoutClass : ComponentBaseClass
    {
        public string ComponentName { get; set; }

        [JsonPropertyName("attributes")]
        public List<string> Attributes { get; set; }

        [JsonPropertyName("contents")]
        public List<ElementContainerClass> Contents { get; set; }

    }
    public class ComponentWidgetClass : ComponentBaseClass
    {
        public string ComponentName { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("animation")]
        public string Animation { get; set; }

    }

    public class Asset
    {
        [JsonPropertyName("form")]
        public string Form { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("type")]
        public string Mime { get; set; }
    }
}
