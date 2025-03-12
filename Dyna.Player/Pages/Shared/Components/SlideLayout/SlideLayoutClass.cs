using Dyna.Player.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dyna.Player.Pages.Shared.Components.SlideLayout
{
    public class SlideLayoutClass
    {
        public string ComponentName { get; set; } = "SlideLayout";

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        [JsonPropertyName("attributes")]
        public List<string> Attributes { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("contents")]
        public List<ElementContainerClass> Contents { get; set; }
    }
}
