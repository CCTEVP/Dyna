using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dyna.Player.Converters;
using Dyna.Player.TagHelpers;
using Dyna.Player.Utilities;

namespace Dyna.Player.Services
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Initializes all static loggers in the application
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        public static void InitializeStaticLoggers(this IServiceProvider serviceProvider)
        {
            // Get the logger factory
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            
            // Initialize converter loggers
            var widgetsConverterLogger = loggerFactory.CreateLogger("Dyna.Player.Converters.WidgetsConverter");
            WidgetsConverter.InitializeLogger(widgetsConverterLogger);
            
            // Initialize tag helper loggers
            var assetTagHelperLogger = loggerFactory.CreateLogger("Dyna.Player.TagHelpers.AssetTagHelper");
            AssetTagHelper.InitializeLogger(assetTagHelperLogger);
            
            // Initialize utility loggers
            var conversionUtilitiesLogger = loggerFactory.CreateLogger("Dyna.Player.Utilities.ConversionUtilities");
            ConversionUtilities.InitializeLogger(conversionUtilitiesLogger);
        }
    }
} 