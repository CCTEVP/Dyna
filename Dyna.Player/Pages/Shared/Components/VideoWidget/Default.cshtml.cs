using Microsoft.AspNetCore.Mvc;

namespace Dyna.Player.Pages.Shared.Components.VideoWidget
{
    public class VideoWidgetViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public VideoWidgetViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(VideoWidgetClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}