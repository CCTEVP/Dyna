using Dyna.Shared.Classes.Components;
using Dyna.Shared.Classes.Content;
using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Components.Widgets
{
    public class CountdownWidgetClass : ComponentWidgetClass
    {
        public string ComponentName { get; set; } = "CountdownWidget";

        [JsonPropertyName("contents")]
        public List<ElementContainerClass> Contents { get; set; }

        [JsonPropertyName("targetDateTime")]
        public TargetDateTimeClass TargetDateTime { get; set; }

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
