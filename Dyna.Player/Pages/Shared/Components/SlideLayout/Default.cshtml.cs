using Microsoft.AspNetCore.Mvc;

namespace Dyna.Player.Pages.Shared.Components.SlideLayout
{
    public class SlideLayoutViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public SlideLayoutViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(SlideLayoutClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}