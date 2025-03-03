using System.IO;
using System.Threading.Tasks;
using NUglify;
using NUglify.JavaScript;

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

            if (!File.Exists(jsFilePath))
            {
                return "Non existing asset file: " + assetName + ".js";
            }

            var jsContent = await File.ReadAllTextAsync(jsFilePath);

            if (!debugMode)
            {
                jsContent = MinifyJavaScript(jsContent);
            }

            return jsContent;
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
    }
}