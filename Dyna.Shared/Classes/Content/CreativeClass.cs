using Dyna.Shared.Classes.Components.Layouts;
using Dyna.Shared.Classes.Components.Widgets;
using System.Text.Json.Serialization;

namespace Dyna.Shared.Classes.Content
{
    public class CreativeClass
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created")]
        public DateTime CreateDateTime { get; set; }

        [JsonPropertyName("updated")]
        public DateTime UpdatedDateTime { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }

        [JsonPropertyName("elements")]
        public List<ElementsClass> Elements { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("parent")]
        public List<string> Parent { get; set; }
    }
    public class ElementsClass
    {
        [JsonPropertyName("slideLayout")]
        public SlideLayoutClass? SlideLayout { get; set; }

        [JsonPropertyName("frameLayout")]
        public FrameLayoutClass? FrameLayout { get; set; }
    }
    public class ElementContainerClass
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
    public class FormatClass
    {

    }
}
