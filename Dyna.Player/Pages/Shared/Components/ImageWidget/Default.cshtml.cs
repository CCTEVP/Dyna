using Microsoft.AspNetCore.Mvc;
using Dyna.Player.Models;

namespace Dyna.Player.Pages.Shared.Components.ImageWidget
{
    public class ImageWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dyna.Player.Models.ImageWidget model)
        {
            return View(model);
        }
    }
}