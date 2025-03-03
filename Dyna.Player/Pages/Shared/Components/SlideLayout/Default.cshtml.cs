// SlideLayoutViewComponent.cs (example)

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Dyna.Player.Models;
using Dyna.Player.Services; // Add this

namespace Dyna.Player.Pages.Shared.Components.SlideLayout
{
    public class SlideLayoutViewComponent : ViewComponent
    {
        private readonly IWidgetStyleService _widgetStyleService; // Inject the service

        public SlideLayoutViewComponent(IWidgetStyleService widgetStyleService)
        {
            _widgetStyleService = widgetStyleService;
        }

        public async Task<IViewComponentResult> InvokeAsync(SlideData slide)
        {
            slide.Styles = await _widgetStyleService.GetCombinedStylesAsync("SlideLayout", slide.Styles); // Use the service
            return View(slide);
        }
    }
}