using Microsoft.AspNetCore.Mvc;

namespace Dyna.Player.Pages.Shared.Components.CountdownWidget
{
    public class CountdownWidgetViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public CountdownWidgetViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(CountdownWidgetClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}