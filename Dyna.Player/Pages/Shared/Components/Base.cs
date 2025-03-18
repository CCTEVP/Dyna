using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Dyna.Player.Pages.Shared.Components
{
    public abstract class BaseViewComponent : ViewComponent
    {
        protected BaseViewComponent()
        {
            // No dependencies needed here anymore
        }

        protected async Task<IViewComponentResult> InvokeAsync<T>(T model) where T : class
        {
            await Task.CompletedTask; // Just to make the method async
            return View(model); // Directly render the model
        }
    }
}