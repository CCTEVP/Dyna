using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dyna.Player.Pages.Shared.Components.VideoWidget
{
    public class VideoWidgetClass
    {
        public string ComponentName { get; set; } = "VideoWidget";

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        [JsonPropertyName("attributes")]
        public List<string> Attributes { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }
    }
}
