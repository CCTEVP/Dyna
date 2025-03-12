using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dyna.Player.Pages.Shared.Components.CardWidget
{
    public class CardWidgetClass
    {
        public string ComponentName { get; set; } = "CardWidget";

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

        [JsonPropertyName("animation")]
        public string Animation { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }
    }
}
