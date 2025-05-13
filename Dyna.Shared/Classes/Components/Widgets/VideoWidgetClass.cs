using Dyna.Shared.Classes.Components;
using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Components.Widgets
{
    public class VideoWidgetClass : ComponentWidgetClass
    {
        public string ComponentName { get; set; } = "VideoWidget";

        [JsonPropertyName("source")]
        public Source Source { get; set; }
    }
}
