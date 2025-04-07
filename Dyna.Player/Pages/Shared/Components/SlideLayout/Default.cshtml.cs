using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Dyna.Shared.Classes.Components.Layouts;

namespace Dyna.Player.Pages.Shared.Components.SlideLayout
{
    public class SlideLayoutViewComponent : BaseViewComponent
    {
        public SlideLayoutViewComponent(ILogger<SlideLayoutViewComponent> logger = null) 
            : base(logger)
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(SlideLayoutClass layout)
        {
            Logger?.LogDebug("Rendering SlideLayout: {Id}", layout?.Identifier);
            
            // Log container items if exists
            if (layout?.Contents != null)
            {
                foreach (var widgetContainer in layout.Contents)
                {
                    Logger?.LogDebug("Widget Container Type: {Type}", widgetContainer.GetType().Name);
                    Logger?.LogTrace("Contents: {Contents}", JsonConvert.SerializeObject(widgetContainer));
                    
                    if (widgetContainer.ImageWidget != null)
                        Logger?.LogDebug("Rendering ImageWidget");
                    else if (widgetContainer.CountdownWidget != null)
                        Logger?.LogDebug("Rendering CountdownWidget");
                    else if (widgetContainer.VideoWidget != null)
                        Logger?.LogDebug("Rendering VideoWidget");
                    else if (widgetContainer.TextWidget != null)
                        Logger?.LogDebug("Rendering TextWidget");
                    else if (widgetContainer.CardWidget != null)
                        Logger?.LogDebug("Rendering CardWidget");
                    else if (widgetContainer.BoxLayout != null)
                        Logger?.LogDebug("Rendering BoxLayout");
                    else
                        Logger?.LogWarning("No widget found in WidgetContainer");
                }
            }
            
            return await base.InvokeAsync(layout);
        }
    }
}