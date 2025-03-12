using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyna.Player.Pages.Interactive
{
    public class DefaultModel : PageModel
    {
        public void OnGet()
        {
            // Retrieve the 'debug' query parameter
            if (HttpContext.Request.Query.TryGetValue("debug", out var debugValue))
            {
                // Check if the 'debug' parameter is present and its value
                if (debugValue == "true")
                {
                    // Debug mode is enabled
                    ViewData["DebugMode"] = "Debug mode enabled"; // Store in ViewData
                    // Your debug-specific logic here
                }
                else
                {
                    // Debug mode is disabled or has a value other than "true"
                    ViewData["DebugMode"] = "Debug mode disabled"; // Store in ViewData
                }
            }
            else
            {
                // 'debug' parameter is missing
                ViewData["DebugMode"] = "Debug parameter not provided"; // Store in ViewData
            }

            // You can access other query parameters similarly
        }
    }
}