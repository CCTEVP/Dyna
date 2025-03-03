using Microsoft.AspNetCore.Mvc;
using Dyna.Player.Models;

namespace Dyna.Player.Pages.Shared.Components.BoxLayout
{
    public class BoxLayoutViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dyna.Player.Models.BoxLayout model)
        {
            return View(model);
        }
    }
}