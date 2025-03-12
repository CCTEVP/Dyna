using Dyna.Player.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dyna.Player.Pages.Shared.Components.CountdownWidget
{
    public class CountdownWidgetClass
    {
        public string ComponentName { get; set; } = "CountdownWidget";

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }

        [JsonPropertyName("attributes")]
        public List<string> Attributes { get; set; }

        [JsonPropertyName("targetDateTime")]
        public TargetDateTimeClass TargetDateTime { get; set; }

        [JsonPropertyName("contents")]
        public List<ElementContainerClass> Contents { get; set; }

        [JsonPropertyName("outcome")]
        public OutcomeClass Outcome { get; set; }
    }
    public class TargetDateTimeClass
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("default")]
        public string Default { get; set; }
    }
    public class OutcomeClass
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
