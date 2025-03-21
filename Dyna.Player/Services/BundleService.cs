using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dyna.Player.TagHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NUglify;
using Microsoft.AspNetCore.Http;

namespace Dyna.Player.Services
{
    public interface IBundleService
    {
        Task<string> GetCssBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode, string bundleType = "components");
        Task<string> GetJsBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode, string bundleType = "components");
    }

    public class BundleService : IBundleService
    {
        private readonly IAssetService _assetService;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BundleService> _logger;
        private const string CSS_BUNDLE_DIRECTORY = "wwwroot/css";
        private const string JS_BUNDLE_DIRECTORY = "wwwroot/js";
        private static readonly SemaphoreSlim _bundleLock = new SemaphoreSlim(1, 1);

        public BundleService(
            IAssetService assetService, 
            IWebHostEnvironment environment, 
            IMemoryCache cache,
            ILogger<BundleService> logger = null)
        {
            _assetService = assetService;
            _environment = environment;
            _cache = cache;
            _logger = logger;

            // Ensure bundle directories exist
            EnsureDirectoryExists(CSS_BUNDLE_DIRECTORY);
            EnsureDirectoryExists(JS_BUNDLE_DIRECTORY);
        }

        private void EnsureDirectoryExists(string directory)
        {
            var dir = Path.Combine(_environment.ContentRootPath, directory);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public async Task<string> GetCssBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode, string bundleType = "components")
        {
            return await GetBundleUrlAsync(assets, "css", debugMode, bundleType);
        }

        public async Task<string> GetJsBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode, string bundleType = "components")
        {
            return await GetBundleUrlAsync(assets, "js", debugMode, bundleType);
        }

        private async Task<string> GetBundleUrlAsync(IEnumerable<AssetInfo> assets, string type, bool debugMode, string bundleType)
        {
            // Filter assets by type
            var filteredAssets = assets.Where(a => a.AssetType == type).ToList();
            if (!filteredAssets.Any())
            {
                return null;
            }
            
            // Validate bundle type
            bundleType = bundleType.ToLower();
            if (bundleType != "components" && bundleType != "libraries" && bundleType != "caching")
            {
                bundleType = "components"; // Default to components if invalid
            }
            
            // For caching bundles, only JS is supported
            if (bundleType == "caching" && type.ToLower() != "js")
            {
                _logger?.LogWarning("Caching bundles only support JS. Requested {Type} will be ignored.", type);
                return null;
            }

            // Generate a hash of the asset list to use as a cache key
            string assetsHash = GenerateAssetsHash(filteredAssets);
            
            // Extract the creative ID from the route path
            var httpContext = new HttpContextAccessor().HttpContext;
            string path = httpContext?.Request.Path ?? "";
            string creativeId = "default";
            
            // Parse the creative ID from paths like /dynamic/123456789 or /interactive/123456789
            if (path.StartsWith("/dynamic/") || path.StartsWith("/interactive/"))
            {
                string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length >= 2)
                {
                    creativeId = segments[1]; // The second segment is the ID
                }
            }
            
            // Create a URL that includes the creative ID and bundle type
            string bundleUrl = $"/{type}/{bundleType}_bundle/{creativeId}{(debugMode ? "" : ".min")}.{type}";
            
            _logger?.LogInformation("Generated bundle URL: {BundleUrl} for creative ID: {CreativeId}, bundle type: {BundleType}, debug mode: {DebugMode}, path: {Path}", 
                bundleUrl, creativeId, bundleType, debugMode, path);
            
            return bundleUrl;
        }

        private string GenerateRequestIdentifier(string requestPath, string requestId)
        {
            // Create a string that represents the request
            string requestString = $"{requestPath}|{requestId}";
            
            // Generate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(requestString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant().Substring(0, 8);
            }
        }

        private async Task<string> GenerateBundleContentAsync(List<AssetInfo> assets, string type, bool debugMode)
        {
            StringBuilder bundleContent = new StringBuilder();
            
            // Add a header comment in debug mode
            if (debugMode)
            {
                bundleContent.AppendLine($"/* Bundle generated in debug mode on {DateTime.Now} */");
                bundleContent.AppendLine($"/* Contains {assets.Count} {type} assets */");
                bundleContent.AppendLine();
            }
            
            foreach (var asset in assets)
            {
                try
                {
                    // Get the unminified content - we'll minify the entire bundle at the end if needed
                    string content = await _assetService.GetAssetAsync(asset.AssetName, asset.AssetLocation, type, true);
                    
                    // Skip if content indicates an error
                    if (content.StartsWith("Invalid") || content.StartsWith("Non existing"))
                    {
                        _logger?.LogWarning("Skipping asset {AssetName} of type {AssetType} in location {AssetLocation}: {Content}", 
                            asset.AssetName, asset.AssetType, asset.AssetLocation, content);
                        
                        if (debugMode)
                        {
                            bundleContent.AppendLine($"/* Error loading {asset.AssetLocation}/{asset.AssetName}.{type}: {content} */");
                        }
                        continue;
                    }
                    
                    // Add a comment to identify the source file in debug mode
                    if (debugMode)
                    {
                        bundleContent.AppendLine($"/* ===== BEGIN {asset.AssetLocation}/{asset.AssetName}.{type} ===== */");
                    }
                    
                    bundleContent.AppendLine(content);
                    
                    if (debugMode)
                    {
                        bundleContent.AppendLine($"/* ===== END {asset.AssetLocation}/{asset.AssetName}.{type} ===== */");
                    }
                    
                    bundleContent.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error bundling asset {AssetName}.{AssetType}", asset.AssetName, type);
                    
                    if (debugMode)
                    {
                        bundleContent.AppendLine($"/* Error processing {asset.AssetLocation}/{asset.AssetName}.{type}: {ex.Message} */");
                    }
                }
            }

            string result = bundleContent.ToString();
            
            // Minify only if not in debug mode
            if (!debugMode)
            {
                try
                {
                    _logger?.LogInformation("Minifying {Type} bundle with {Count} assets", type, assets.Count);
                    
                    result = type.ToLower() switch
                    {
                        "js" => MinifyJavaScript(result),
                        "css" => MinifyCss(result),
                        _ => result
                    };
                    
                    // Log the size reduction
                    int originalSize = bundleContent.Length;
                    int minifiedSize = result.Length;
                    double reductionPercentage = (1 - ((double)minifiedSize / originalSize)) * 100;
                    _logger?.LogInformation("Minification reduced size by {Percentage:F2}% ({OriginalSize} → {MinifiedSize} bytes)", 
                        reductionPercentage, originalSize, minifiedSize);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error minifying {Type} bundle", type);
                }
            }
            else
            {
                _logger?.LogInformation("Skipping minification for {Type} bundle (debug mode)", type);
            }
            
            return result;
        }

        private string MinifyJavaScript(string javascript)
        {
            var result = Uglify.Js(javascript);

            if (result.HasErrors)
            {
                foreach (var error in result.Errors)
                {
                    _logger?.LogWarning("JS minification error: {Message} at line {Line}, column {Column}", 
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
                    _logger?.LogWarning("CSS minification error: {Message} at line {Line}, column {Column}", 
                        error.Message, error.StartLine, error.StartColumn);
                }
                return css;
            }

            return result.Code;
        }

        private string GenerateAssetsHash(IEnumerable<AssetInfo> assets)
        {
            // Create a string that represents all assets
            string assetString = string.Join("|", assets.Select(a => $"{a.AssetLocation}/{a.AssetName}.{a.AssetType}"));
            
            // Generate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(assetString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
} 