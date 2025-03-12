using Dyna.Player.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dyna.Player.Pages.Shared.Components.ImageWidget
{
    public class ImageWidgetViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public ImageWidgetViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(ImageWidgetClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}