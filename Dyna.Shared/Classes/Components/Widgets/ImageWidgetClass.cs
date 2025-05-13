using Dyna.Shared.Classes.Components;
using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Components.Widgets
{
    public class ImageWidgetClass : ComponentWidgetClass
    {
        public string ComponentName { get; set; } = "ImageWidget";

        [JsonPropertyName("source")]
        public Source Source { get; set; }
    }

}
