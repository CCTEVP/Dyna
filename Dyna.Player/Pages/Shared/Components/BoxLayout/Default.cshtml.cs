using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Dyna.Player.Pages.Shared.Components.BoxLayout
{
    public class BoxLayoutViewComponent : BaseViewComponent // Inherit from the simplified BaseViewComponent
    {
        public BoxLayoutViewComponent() : base() { } // No dependencies needed

        public async Task<IViewComponentResult> InvokeAsync(BoxLayoutClass layout)
        {
            return await base.InvokeAsync(layout); // Directly pass the layout to the base InvokeAsync
        }
    }
}