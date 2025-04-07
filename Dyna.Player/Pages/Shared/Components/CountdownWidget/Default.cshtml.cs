using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Dyna.Player.Services;
using Dyna.Shared.Classes.Components.Layouts;
using Dyna.Shared.Classes.Components.Widgets;


namespace Dyna.Player.Pages.Shared.Components.CountdownWidget
{
    public class CountdownWidgetViewComponent : BaseViewComponent
    {
        private readonly QueryParameterService _queryParameterService;

        public CountdownWidgetViewComponent(
            ILogger<CountdownWidgetViewComponent> logger = null,
            QueryParameterService queryParameterService = null) 
            : base(logger)
        {
            _queryParameterService = queryParameterService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CountdownWidgetClass widget)
        {
            Logger?.LogDebug("Rendering CountdownWidget: {Id}", widget?.Identifier);
            
            if (widget != null)
            {
                Logger?.LogTrace("CountdownWidget: {Widget}", JsonConvert.SerializeObject(widget));
                
                string targetDateTime = widget.TargetDateTime?.Default;

                if (widget.TargetDateTime?.Source == "queryParameter" && _queryParameterService != null)
                {
                    string queryTargetDateTime = _queryParameterService.GetQueryParameterValue(widget.TargetDateTime.Name);
                    if (queryTargetDateTime != null)
                    {
                        targetDateTime = queryTargetDateTime;
                        Logger?.LogDebug("Using query parameter value for {ParameterName}: {Value}", 
                            widget.TargetDateTime.Name, targetDateTime);
                    }
                }
                
                if (widget.Contents != null)
                {
                    Logger?.LogDebug("CountdownWidget has {Count} content items", widget.Contents.Count);
                }
            }
            
            return await base.InvokeAsync(widget);
        }
    }
}