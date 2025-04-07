using Dyna.Shared.Classes.Components.Layouts;
using Dyna.Shared.Classes.Components.Widgets;
using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Content
{
    public class CreativeClassOld
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("createDateTime")]
        public DateTime CreateDateTime { get; set; }

        [JsonPropertyName("elements")]
        public List<ElementsClassOld> Elements { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }
    }
    public class ElementsClassOld
    {
        [JsonPropertyName("slideLayout")]
        public SlideLayoutClass? SlideLayout { get; set; }

        [JsonPropertyName("FrameLayout")]
        public FrameLayoutClass? FrameLayout { get; set; }
    }
    public class ElementContainerClassOld
    {
        [JsonPropertyName("textWidget")]
        public TextWidgetClass? TextWidget { get; set; }

        [JsonPropertyName("cardWidget")]
        public CardWidgetClass? CardWidget { get; set; }

        [JsonPropertyName("imageWidget")]
        public ImageWidgetClass? ImageWidget { get; set; }

        [JsonPropertyName("countdownWidget")]
        public CountdownWidgetClass? CountdownWidget { get; set; }

        [JsonPropertyName("videoWidget")]
        public VideoWidgetClass? VideoWidget { get; set; }

        [JsonPropertyName("boxLayout")]
        public BoxLayoutClass? BoxLayout { get; set; }
    }
}
