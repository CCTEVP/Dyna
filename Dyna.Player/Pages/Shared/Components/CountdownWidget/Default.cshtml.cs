using Microsoft.AspNetCore.Mvc;
using Dyna.Player.Models;

namespace Dyna.Player.Pages.Shared.Components.CountdownWidget
{
    public class CountdownWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dyna.Player.Models.CountdownWidget model)
        {
            return View(model);
        }
    }
}