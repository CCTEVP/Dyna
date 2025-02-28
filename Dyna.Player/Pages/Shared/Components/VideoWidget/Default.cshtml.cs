using Microsoft.AspNetCore.Mvc;
using Dyna.Player.Models;

namespace Dyna.Player.Pages.Shared.Components.VideoWidget
{
    public class VideoWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dyna.Player.Models.VideoWidget model)
        {
            return View(model);
        }
    }
}