using Microsoft.Extensions.Logging;

namespace Dyna.Player.Utilities
{
    public static class ConversionUtilities
    {
        private static ILogger _logger;

        public static void InitializeLogger(ILogger logger)
        {
            _logger = logger;
        }
        
        public static string GenerateCssStyleString(object stylesObject)
        {
            _logger?.LogTrace("stylesObject: {StylesObject}", stylesObject);
            
            if (stylesObject is Dictionary<string, string> styles)
            {
                _logger?.LogTrace("StylesObject is Dictionary<string, string>");
                var styleString = new System.Text.StringBuilder();
                
                foreach (var kvp in styles)
                {
                    styleString.Append($"{kvp.Key}: {kvp.Value}; ");
                }
                
                _logger?.LogTrace("Generated style string: {StyleString}", styleString);
                return styleString.ToString();
            }
            else
            {
                _logger?.LogWarning("Error: Styles object is not a Dictionary<string, string>");
                return string.Empty; // Or handle the error as needed
            }
        }
    }
}
