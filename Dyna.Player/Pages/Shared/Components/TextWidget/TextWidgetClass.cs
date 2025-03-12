using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dyna.Player.Pages.Shared.Components.TextWidget
{
    public class TextWidgetClass
    {
        public string ComponentName { get; set; } = "TextWidget";

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }
    }
}
