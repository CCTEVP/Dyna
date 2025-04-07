using Microsoft.AspNetCore.Mvc;
using Dyna.Shared.Classes.Components.Layouts;
using Dyna.Shared.Classes.Components.Widgets;


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