using Microsoft.AspNetCore.Mvc;

namespace Dyna.Player.Pages.Shared.Components.CardWidget
{
    public class CardWidgetViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public CardWidgetViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(CardWidgetClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}