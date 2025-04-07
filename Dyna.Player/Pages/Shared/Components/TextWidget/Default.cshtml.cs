using Microsoft.AspNetCore.Mvc;
using Dyna.Shared.Classes.Components.Layouts;
using Dyna.Shared.Classes.Components.Widgets;

namespace Dyna.Player.Pages.Shared.Components.TextWidget
{
    public class TextWidgetViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public TextWidgetViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(TextWidgetClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}