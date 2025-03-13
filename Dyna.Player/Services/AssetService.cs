using System.IO;
using System.Threading.Tasks;
using NUglify;
using NUglify.Css;

namespace Dyna.Player.Services
{
    public interface IAssetService
    {
        Task<string> GetAssetAsync(string assetName, string assetLocation, string extension, bool debugMode);
        string GetAssetUrl(string assetName, string assetLocation, string assetType, bool debugMode);
    }

    public class AssetService : IAssetService
    {
        public async Task<string> GetAssetAsync(string assetName, string assetLocation, string extension, bool debugMode)
        {
            var basePath = Directory.GetCurrentDirectory();
            string assetPath;
            string fullPath;

            System.Diagnostics.Debug.WriteLine($"GetAssetAsync called with: {assetName}, {assetLocation}, {extension}, {debugMode}");

            // Handle library paths with subdirectories
            if (assetLocation.StartsWith("Libraries/"))
            {
                // For paths like "Libraries/Creative" or "Libraries/WidgetAnimations"
                assetPath = Path.Combine(basePath, "Pages", "Shared", assetLocation);
                fullPath = Path.Combine(assetPath, $"{assetName}.{extension}");
                System.Diagnostics.Debug.WriteLine($"Library path: {fullPath}");
            }
            else
            {
                switch (assetLocation.ToLower())
                {
                    case "library":
                        assetPath = Path.Combine(basePath, "Pages", "Shared", "Libraries", assetName);
                        break;
                    case "widget":
                    case "layout":
                        assetPath = Path.Combine(basePath, "Pages", "Shared", "Components", assetName);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"Invalid asset location: {assetLocation}");
                        return $"Invalid asset location: {assetLocation}";
                }
                
                fullPath = Path.Combine(assetPath, $"Default.{extension}");
                System.Diagnostics.Debug.WriteLine($"Standard path: {fullPath}");
            }

            if (!File.Exists(fullPath))
            {
                System.Diagnostics.Debug.WriteLine($"File not found: {fullPath}");
                return $"Non existing asset file: {assetName} ({fullPath})";
            }

            System.Diagnostics.Debug.WriteLine($"File found: {fullPath}");
            var content = await File.ReadAllTextAsync(fullPath);

            if (!debugMode)
            {
                content = extension.ToLower() switch
                {
                    "js" => MinifyJavaScript(content),
                    "css" => MinifyCss(content),
                    _ => content
                };
            }

            return content;
        }

        private string MinifyJavaScript(string javascript)
        {
            var result = Uglify.Js(javascript);

            if (result.HasErrors)
            {
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Minification error: {error.Message} at line {error.StartLine}, column {error.StartColumn}");
                }
                return javascript;
            }

            return result.Code;
        }

        private string MinifyCss(string css)
        {
            var result = Uglify.Css(css);

            if (result.HasErrors)
            {
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Minification error: {error.Message} at line {error.StartLine}, column {error.StartColumn}");
                }
                return css;
            }

            return result.Code;
        }

        public string GetAssetUrl(string assetName, string assetLocation, string assetType, bool debugMode)
        {
            string url = $"/asset/{assetType}/{assetLocation}/{assetName}.{assetType}";
            
            if (debugMode)
            {
                url += "?debug=true";
            }
            
            return url;
        }
    }
} 