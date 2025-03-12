using Dyna.Player.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dyna.Player.Pages.Shared.Components.BoxLayout
{
    public class BoxLayoutClass
    {
        public string ComponentName { get; set; } = "BoxLayout";

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        [JsonPropertyName("attributes")]
        public List<string> Attributes { get; set; }

        [JsonPropertyName("contents")]
        public List<ElementContainerClass> Contents { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }
    }
}
