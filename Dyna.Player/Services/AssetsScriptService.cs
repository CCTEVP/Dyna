using System.IO;
using System.Threading.Tasks;
using NUglify;
using NUglify.JavaScript;
using NUglify.Css;

namespace Dyna.Player.Services
{
    public interface IAssetScriptService
    {
        Task<string> GetAssetScriptAsync(string assetName, string assetType, string assetLocation, bool debugMode);
    }

    public class AssetScriptService : IAssetScriptService
    {
        public async Task<string> GetAssetScriptAsync(string assetName, string assetType, string assetLocation, bool debugMode)
        {
            var basePath = Directory.GetCurrentDirectory();
            string componentsPath;
            string jsFileName = $"Source_{assetName}.js";
            string cssFileName = $"Source_{assetName}.css";

            if (assetLocation == "widget")
            {
                componentsPath = Path.Combine(basePath, "Pages", "Shared", "Components", assetName);
            }
            else if (assetLocation == "layout")
            {
                componentsPath = Path.Combine(basePath, "Pages", "Shared", "Components", assetName);
            }
            else
            {
                return "Invalid asset location: " + assetLocation;
            }

            var jsFilePath = Path.Combine(componentsPath, jsFileName);
            var cssFilePath = Path.Combine(componentsPath, cssFileName);

            if (!File.Exists(jsFilePath) || !File.Exists(cssFilePath))
            {
                return "Non existing asset file: " + assetName + ".js/.css";
            }

            var jsContent = await File.ReadAllTextAsync(jsFilePath);
            var cssContent = await File.ReadAllTextAsync(cssFilePath);

            // Minify CSS content before inserting into JavaScript
            var minifiedCss = MinifyCss(cssContent);

            var combinedJsContent = jsContent.Replace("/* CSS_CONTENT_PLACEHOLDER */", minifiedCss.Replace("`", "\\`"));

            if (!debugMode)
            {
                combinedJsContent = MinifyJavaScript(combinedJsContent);
            }

            return combinedJsContent;
        }

        private string MinifyJavaScript(string javascript)
        {
            var result = Uglify.Js(javascript);

            if (result.HasErrors)
            {
                // Handle minification errors (log or throw an exception)
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Minification error: {error.Message} at line {error.StartLine}, column {error.StartColumn}");
                }
                // Return the original javascript to avoid broken code.
                return javascript;
            }

            return result.Code;
        }

        private string MinifyCss(string css)
        {
            var result = Uglify.Css(css);

            if (result.HasErrors)
            {
                // Handle minification errors (log or throw an exception)
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Minification error: {error.Message} at line {error.StartLine}, column {error.StartColumn}");
                }
                // Return the original css to avoid broken code.
                return css;
            }

            return result.Code;
        }
    }
}