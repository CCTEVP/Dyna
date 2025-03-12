using System.IO;
using System.Threading.Tasks;
using NUglify;
using NUglify.Css;

namespace Dyna.Player.Services
{
    public interface IAssetService
    {
        Task<string> GetAssetAsync(string assetName, string assetLocation, string extension, bool debugMode);
    }

    public class AssetService : IAssetService
    {
        public async Task<string> GetAssetAsync(string assetName, string assetLocation, string extension, bool debugMode)
        {
            var basePath = Directory.GetCurrentDirectory();
            string assetPath;

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
                    return $"Invalid asset location: {assetLocation}";
            }

            var fullPath = Path.Combine(assetPath, $"Default.{extension}");

            if (!File.Exists(fullPath))
            {
                return $"Non existing asset file: {assetName} ({fullPath})";
            }

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
    }
} 