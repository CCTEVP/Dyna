using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Dyna.Player.Models;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Dyna.Player.Services
{
    public interface IWidgetStyleService
    {
        Task<string> GetCombinedStylesAsync(string widgetName, object widgetStyles);
    }

    public class WidgetStyleService : IWidgetStyleService
    {
        public async Task<string> GetCombinedStylesAsync(string widgetName, object widgetStyles)
        {
            // 1. Get default styles from JSON file
            var defaultStyles = await GetDefaultStylesFromJsonAsync(widgetName);

            // 2. Override with styles from widget data (if any)
            JObject overrideStyles = widgetStyles as JObject;
            if (overrideStyles != null)
            {
                var combinedStyles = OverrideStyles(defaultStyles, overrideStyles);
                return JObjectToCssStyleString(combinedStyles);
            }
            else
            {
                return JObjectToCssStyleString(defaultStyles);
            }
        }

        private async Task<JObject> GetDefaultStylesFromJsonAsync(string widgetName)
        {
            var basePath = Directory.GetCurrentDirectory();
            var jsonFilePath = Path.Combine(basePath, "Pages", "Shared", "Components", widgetName, $"Source_{widgetName}_DefaultStyles.json");

            if (File.Exists(jsonFilePath))
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(jsonFilePath);
                return JObject.Parse(jsonContent);
            }

            return new JObject(); // Return empty JObject if no JSON file
        }

        private JObject OverrideStyles(JObject defaultStyles, JObject overrideStyles)
        {
            if (overrideStyles != null)
            {
                foreach (var property in overrideStyles.Properties())
                {
                    defaultStyles[property.Name] = property.Value;
                }
            }

            return defaultStyles;
        }

        private string JObjectToCssStyleString(JObject styles)
        {
            string css = "";
            foreach (var property in styles.Properties())
            {
                css += $"{property.Name}: {property.Value.ToString()}; ";
            }
            return css;
        }
    }
}