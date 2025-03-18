using Dyna.Player.Pages.Shared.Components.BoxLayout;
using Dyna.Player.Pages.Shared.Components.CardWidget;
using Dyna.Player.Pages.Shared.Components.CountdownWidget;
using Dyna.Player.Pages.Shared.Components.SlideLayout;
using Dyna.Player.Pages.Shared.Components.TextWidget;
using Dyna.Player.Pages.Shared.Components.VideoWidget;
using System.Text.Json.Serialization;

namespace Dyna.Player.Models
{
    public class CreativeClass
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("createDateTime")]
        public DateTime CreateDateTime { get; set; }

        [JsonPropertyName("pieces")]
        public List<PiecesClass> Pieces { get; set; }

        [JsonPropertyName("styles")]
        public Dictionary<string, string> Styles { get; set; }
    }
    public class PiecesClass
    {
        [JsonPropertyName("slideLayout")]
        public SlideLayoutClass? SlideLayout { get; set; }
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
}
