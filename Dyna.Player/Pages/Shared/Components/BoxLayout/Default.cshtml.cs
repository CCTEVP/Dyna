using Microsoft.AspNetCore.Mvc;
using Dyna.Shared.Classes.Components.Layouts;

namespace Dyna.Player.Pages.Shared.Components.BoxLayout
{
    public class BoxLayoutViewComponent : BaseViewComponent
    {
        public BoxLayoutViewComponent(ILogger<BoxLayoutViewComponent> logger = null) 
            : base(logger)
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(BoxLayoutClass layout)
        {
            Logger?.LogDebug("Rendering BoxLayout: {Id}", layout?.Identifier);
            
            // Log container items if exists
            if (layout?.Contents != null)
            {
                foreach (var widgetContainer in layout.Contents)
                {
                    Logger?.LogDebug("Widget Container Type: {Type}", widgetContainer.GetType().Name);
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
                        Logger?.LogDebug("Rendering nested BoxLayout");
                    else
                        Logger?.LogWarning("No widget found in WidgetContainer");
                }
            }
            
            return await base.InvokeAsync(layout);
        }
    }
}