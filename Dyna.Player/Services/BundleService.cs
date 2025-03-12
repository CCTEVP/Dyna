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

namespace Dyna.Player.Services
{
    public interface IBundleService
    {
        Task<string> GetCssBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode);
        Task<string> GetJsBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode);
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

        public async Task<string> GetCssBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode)
        {
            return await GetBundleUrlAsync(assets, "css", debugMode);
        }

        public async Task<string> GetJsBundleUrlAsync(IEnumerable<AssetInfo> assets, bool debugMode)
        {
            return await GetBundleUrlAsync(assets, "js", debugMode);
        }

        private async Task<string> GetBundleUrlAsync(IEnumerable<AssetInfo> assets, string type, bool debugMode)
        {
            // Filter assets by type
            var filteredAssets = assets.Where(a => a.AssetType == type).ToList();
            if (!filteredAssets.Any())
            {
                return null;
            }

            // Generate a hash of the asset list to use as a cache key and filename
            string assetsHash = GenerateAssetsHash(filteredAssets);
            string cacheKey = $"bundle_{type}_{assetsHash}_{(debugMode ? "debug" : "min")}";
            
            _logger?.LogInformation("Generating bundle for {Type} with debug mode: {DebugMode}", type, debugMode);

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out string cachedUrl))
            {
                _logger?.LogInformation("Using cached bundle URL: {CachedUrl}", cachedUrl);
                return cachedUrl;
            }

            // Generate bundle filename - use .min for production, no suffix for debug
            string fileName = $"bundle-{assetsHash}{(debugMode ? "" : ".min")}.{type}";
            string bundleDirectory = type.ToLower() == "css" ? CSS_BUNDLE_DIRECTORY : JS_BUNDLE_DIRECTORY;
            string bundlePath = Path.Combine(bundleDirectory, fileName);
            string fullPath = Path.Combine(_environment.ContentRootPath, bundlePath);
            
            _logger?.LogInformation("Bundle file path: {FullPath}", fullPath);

            // Check if bundle file already exists
            if (File.Exists(fullPath))
            {
                string url = $"/{type}/bundle-{assetsHash}{(debugMode ? "" : ".min")}.{type}";
                _cache.Set(cacheKey, url, TimeSpan.FromHours(24));
                _logger?.LogInformation("Using existing bundle file: {Url}", url);
                return url;
            }

            // Use a semaphore to prevent multiple threads from generating the same bundle
            await _bundleLock.WaitAsync();
            try
            {
                // Check again in case another thread created the file while we were waiting
                if (File.Exists(fullPath))
                {
                    string url = $"/{type}/bundle-{assetsHash}{(debugMode ? "" : ".min")}.{type}";
                    _cache.Set(cacheKey, url, TimeSpan.FromHours(24));
                    _logger?.LogInformation("Using existing bundle file (after lock): {Url}", url);
                    return url;
                }

                _logger?.LogInformation("Generating new bundle content with debug mode: {DebugMode}", debugMode);
                
                // Bundle doesn't exist, create it
                string bundleContent = await GenerateBundleContentAsync(filteredAssets, type, debugMode);
                
                // Write to file
                await File.WriteAllTextAsync(fullPath, bundleContent);
                _logger?.LogInformation("Wrote bundle file to: {FullPath}", fullPath);

                // Return the URL
                string bundleUrl = $"/{type}/bundle-{assetsHash}{(debugMode ? "" : ".min")}.{type}";
                _cache.Set(cacheKey, bundleUrl, TimeSpan.FromHours(24));
                
                return bundleUrl;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating bundle for {Type} with hash {Hash}", type, assetsHash);
                // Return a fallback URL that will trigger individual asset loading
                return null;
            }
            finally
            {
                _bundleLock.Release();
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
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant().Substring(0, 12);
            }
        }
    }
} 