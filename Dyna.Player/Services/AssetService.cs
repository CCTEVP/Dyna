using System.IO;
using System.Threading.Tasks;
using NUglify;
using NUglify.Css;
using Microsoft.Extensions.Logging;

namespace Dyna.Player.Services
{
    public interface IAssetService
    {
        Task<string> GetAssetAsync(string assetName, string assetLocation, string extension, bool debugMode);
        string GetAssetUrl(string assetName, string assetLocation, string assetType, bool debugMode);
    }

    public class AssetService : IAssetService
    {
        private readonly ILogger<AssetService> _logger;

        public AssetService(ILogger<AssetService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetAssetAsync(string assetName, string assetLocation, string extension, bool debugMode)
        {
            var basePath = Directory.GetCurrentDirectory();
            string assetPath;
            string fullPath;

            _logger.LogDebug("GetAssetAsync called with: {AssetName}, {AssetLocation}, {Extension}, {DebugMode}", 
                assetName, assetLocation, extension, debugMode);

            // Handle library paths with subdirectories
            if (assetLocation.StartsWith("Libraries/"))
            {
                // For paths like "Libraries/Creative" or "Libraries/WidgetAnimations"
                assetPath = Path.Combine(basePath, "Pages", "Shared", assetLocation);
                fullPath = Path.Combine(assetPath, $"{assetName}.{extension}");
                _logger.LogDebug("Library path: {FullPath}", fullPath);
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
                        _logger.LogWarning("Invalid asset location: {AssetLocation}", assetLocation);
                        return $"Invalid asset location: {assetLocation}";
                }
                
                fullPath = Path.Combine(assetPath, $"Default.{extension}");
                _logger.LogDebug("Standard path: {FullPath}", fullPath);
            }

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FullPath}", fullPath);
                return $"Non existing asset file: {assetName} ({fullPath})";
            }

            _logger.LogDebug("File found: {FullPath}", fullPath);
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
                    _logger.LogWarning("JS minification error: {Message} at line {Line}, column {Column}", 
                        error.Message, error.StartLine, error.StartColumn);
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
                    _logger.LogWarning("CSS minification error: {Message} at line {Line}, column {Column}",
                        error.Message, error.StartLine, error.StartColumn);
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