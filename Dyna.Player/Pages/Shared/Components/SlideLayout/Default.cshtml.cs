using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; // Important: Add this using statement
using Dyna.Player.Models;

namespace Dyna.Player.Pages.Shared.Components.SlideLayout // Replace YourProject
{
    public class SlideLayoutViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(SlideData slide) // Correct return type
        {
            return View(slide);
        }
    }
}