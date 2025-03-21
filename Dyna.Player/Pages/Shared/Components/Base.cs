using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dyna.Player.Pages.Shared.Components
{
    public abstract class BaseViewComponent : ViewComponent
    {
        protected ILogger Logger { get; private set; }

        protected BaseViewComponent(ILogger logger = null)
        {
            Logger = logger;
        }

        protected async Task<IViewComponentResult> InvokeAsync<T>(T model) where T : class
        {
            Logger?.LogTrace("Rendering view component with model type: {ModelType}", typeof(T).Name);
            await Task.CompletedTask; // Just to make the method async
            return View(model); // Directly render the model
        }
    }
}