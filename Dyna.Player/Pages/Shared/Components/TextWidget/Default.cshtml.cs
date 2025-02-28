using Microsoft.AspNetCore.Mvc;
using Dyna.Player.Models;

namespace Dyna.Player.Pages.Shared.Components.TextWidget
{
    public class TextWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dyna.Player.Models.TextWidget model)
        {
            return View(model);
        }
    }
}