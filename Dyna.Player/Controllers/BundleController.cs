using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dyna.Player.Services;
using Dyna.Player.TagHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Dyna.Player.Models;
using System.Text.RegularExpressions;

namespace Dyna.Player.Controllers
{
    [ApiController]
    public class BundleController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BundleController> _logger;
        private readonly ApiService _apiService;
        private readonly FileService _fileService;

        public BundleController(
            IAssetService assetService,
            IMemoryCache cache,
            ApiService apiService,
            FileService fileService,
            ILogger<BundleController> logger = null)
        {
            _assetService = assetService;
            _cache = cache;
            _apiService = apiService;
            _fileService = fileService;
            _logger = logger;
        }

        // CSS bundles with constraint to differentiate between debug and minified
        [HttpGet]
        [Route("css/bundle/{creativeId}.css", Order = 1)]
        public async Task<IActionResult> GetCssDebugBundle(string creativeId)
        {
            return await GetBundle(creativeId, "css", true);
        }

        [HttpGet]
        [Route("css/bundle/{creativeId}.min.css", Order = 2)]
        public async Task<IActionResult> GetCssMinBundle(string creativeId)
        {
            return await GetBundle(creativeId, "css", false);
        }

        // JS bundles with constraint to differentiate between debug and minified
        [HttpGet]
        [Route("js/bundle/{creativeId}.js", Order = 1)]
        public async Task<IActionResult> GetJsDebugBundle(string creativeId)
        {
            return await GetBundle(creativeId, "js", true);
        }

        [HttpGet]
        [Route("js/bundle/{creativeId}.min.js", Order = 2)]
        public async Task<IActionResult> GetJsMinBundle(string creativeId)
        {
            return await GetBundle(creativeId, "js", false);
        }

        private async Task<IActionResult> GetBundle(string creativeId, string type, bool debugMode)
        {
            try
            {
                // Create a cache key for this bundle
                string cacheKey = $"bundle_{type}_{creativeId}_{(debugMode ? "debug" : "min")}";

                // Log the request details
                _logger?.LogInformation("Bundle request: type={Type}, creativeId={CreativeId}, debugMode={DebugMode}, path={Path}", 
                    type, creativeId, debugMode, Request.Path);

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out string cachedContent))
                {
                    _logger?.LogInformation("Using cached bundle content for creative ID: {CreativeId}, type: {Type}, debug: {DebugMode}", 
                        creativeId, type, debugMode);
                    
                    return Content(cachedContent, type == "css" ? "text/css" : "application/javascript");
                }

                // Get the assets for this creative ID
                var assets = await GetAssetsForCreativeId(creativeId, type);
                
                // If no assets found, return a default empty bundle instead of 404
                if (assets == null || !assets.Any())
                {
                    _logger?.LogWarning("No assets found for creative ID: {CreativeId}, type: {Type}. Returning empty bundle.", 
                        creativeId, type);
                    
                    string emptyBundle = debugMode 
                        ? $"/* Empty {type} bundle for creative ID: {creativeId} (debug mode) */"
                        : "";
                    
                    return Content(emptyBundle, type == "css" ? "text/css" : "application/javascript");
                }

                // Generate the bundle content
                string bundleContent = await GenerateBundleContentAsync(assets, type, debugMode);
                
                // Cache the content (30 minutes for production, 5 minutes for debug)
                _cache.Set(cacheKey, bundleContent, TimeSpan.FromMinutes(debugMode ? 5 : 30));
                
                return Content(bundleContent, type == "css" ? "text/css" : "application/javascript");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating {Type} bundle for creative ID: {CreativeId}", type, creativeId);
                return StatusCode(500, $"Error generating bundle: {ex.Message}");
            }
        }

        private async Task<List<AssetInfo>> GetAssetsForCreativeId(string creativeId, string type)
        {
            // Clear existing assets to avoid duplicates
            AssetTagHelper.ClearPresentAssets();
            
            try
            {
                // Try to load the creative model to analyze its structure
                string apiUrl = $"https://localhost:7193/data/structure_{creativeId}.json";
                var creative = await _apiService.GetAsync<CreativeClass>(apiUrl);
                
                if (creative != null)
                {
                    _logger?.LogInformation("Successfully loaded creative model for ID: {CreativeId}", creativeId);
                    
                    // Add base assets that are always needed
                    AssetTagHelper.AddPresentAsset("Libraries/Creative/CreativeTicker", "library", 0);
                    // Add the WidgetInitializer with priority 1 (right after CreativeTicker)
                    AssetTagHelper.AddPresentAsset("Libraries/Creative/WidgetInitializer", "library", 1);
                    
                    // Analyze the creative structure to determine which assets are needed
                    await AnalyzeCreativeStructure(creative);
                }
                else
                {
                    _logger?.LogWarning("Could not load creative model for ID: {CreativeId}. Using default assets.", creativeId);
                    
                    // Add default assets if creative model couldn't be loaded
                    AssetTagHelper.AddPresentAsset("Libraries/Creative/CreativeTicker", "library", 0);
                    // Add the WidgetInitializer with priority 1 (right after CreativeTicker)
                    AssetTagHelper.AddPresentAsset("Libraries/Creative/WidgetInitializer", "library", 1);
                    
                    // Add common widget assets
                    AssetTagHelper.AddPresentAsset("SlideLayout", "layout");
                    AssetTagHelper.AddPresentAsset("BoxLayout", "layout");
                    AssetTagHelper.AddPresentAsset("CardWidget", "widget");
                    AssetTagHelper.AddPresentAsset("CountdownWidget", "widget");
                    AssetTagHelper.AddPresentAsset("TextWidget", "widget");
                    
                    // Explicitly add animation libraries for testing
                    _logger?.LogInformation("Adding animation libraries for testing");
                    AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/CardWidgetAnimation", "library", 100);
                    AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/CardWidgetAnimationRoll", "library", 101);
                    AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/CardWidgetAnimationFlip", "library", 102);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading creative model for ID: {CreativeId}. Using default assets.", creativeId);
                
                // Add default assets if there was an error
                AssetTagHelper.AddPresentAsset("Libraries/Creative/CreativeTicker", "library", 0);
                // Add the WidgetInitializer with priority 1 (right after CreativeTicker)
                AssetTagHelper.AddPresentAsset("Libraries/Creative/WidgetInitializer", "library", 1);
            }
            
            // Get all registered assets and filter by type
            var allAssets = AssetTagHelper.GetPresentAssets();
            
            // Debug: Log all assets before filtering
            _logger?.LogInformation("All registered assets before filtering:");
            foreach (var asset in allAssets)
            {
                _logger?.LogInformation("Asset: {Location}/{Name}.{Type} (Priority: {Priority})", 
                    asset.AssetLocation, asset.AssetName, asset.AssetType, asset.Priority);
            }
            
            var filteredAssets = allAssets.Where(a => a.AssetType == type).ToList();
            
            _logger?.LogInformation("Found {Count} assets of type {Type} for creative ID: {CreativeId}", 
                filteredAssets.Count, type, creativeId);
            
            // Log all assets to help with debugging
            foreach (var asset in filteredAssets)
            {
                _logger?.LogInformation("Filtered Asset: {Location}/{Name}.{Type} (Priority: {Priority})", 
                    asset.AssetLocation, asset.AssetName, asset.AssetType, asset.Priority);
            }
            
            return filteredAssets;
        }

        private async Task AnalyzeCreativeStructure(CreativeClass creative)
        {
            if (creative?.Pieces == null) return;
            
            foreach (var piece in creative.Pieces)
            {
                // Check for SlideLayout
                if (piece?.SlideLayout != null)
                {
                    _logger?.LogInformation("Found SlideLayout in creative");
                    AssetTagHelper.AddPresentAsset("SlideLayout", "layout");
                    
                    // Analyze SlideLayout contents
                    await AnalyzeSlideLayout(piece.SlideLayout);
                }
            }
        }

        private async Task AnalyzeSlideLayout(object slideLayout)
        {
            // Use reflection to analyze the structure
            var type = slideLayout.GetType();
            
            // Check if this is a widget or layout by name
            string typeName = type.Name;
            if (typeName.EndsWith("Class"))
            {
                typeName = typeName.Substring(0, typeName.Length - 5);
            }
            
            if (typeName.EndsWith("Widget"))
            {
                _logger?.LogInformation("Found widget: {WidgetName}", typeName);
                AssetTagHelper.AddPresentAsset(typeName, "widget");
            }
            else if (typeName.EndsWith("Layout"))
            {
                _logger?.LogInformation("Found layout: {LayoutName}", typeName);
                AssetTagHelper.AddPresentAsset(typeName, "layout");
            }
            
            // Look for Contents property which might contain more widgets/layouts
            var contentsProperty = type.GetProperty("Contents");
            if (contentsProperty != null)
            {
                var contents = contentsProperty.GetValue(slideLayout) as System.Collections.IEnumerable;
                if (contents != null)
                {
                    foreach (var item in contents)
                    {
                        if (item == null) continue;
                        
                        // Check all properties of this item for widgets or layouts
                        var itemType = item.GetType();
                        foreach (var prop in itemType.GetProperties())
                        {
                            var propValue = prop.GetValue(item);
                            if (propValue != null)
                            {
                                var propTypeName = propValue.GetType().Name;
                                if (propTypeName.EndsWith("Class"))
                                {
                                    propTypeName = propTypeName.Substring(0, propTypeName.Length - 5);
                                }
                                
                                if (propTypeName.EndsWith("Widget"))
                                {
                                    _logger?.LogInformation("Found widget in contents: {WidgetName}", propTypeName);
                                    AssetTagHelper.AddPresentAsset(propTypeName, "widget");
                                    
                                    // Recursively analyze this widget
                                    await AnalyzeSlideLayout(propValue);
                                }
                                else if (propTypeName.EndsWith("Layout"))
                                {
                                    _logger?.LogInformation("Found layout in contents: {LayoutName}", propTypeName);
                                    AssetTagHelper.AddPresentAsset(propTypeName, "layout");
                                    
                                    // Recursively analyze this layout
                                    await AnalyzeSlideLayout(propValue);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<string> GenerateBundleContentAsync(List<AssetInfo> assets, string type, bool debugMode)
        {
            var bundleContent = new System.Text.StringBuilder();
            
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
                    // Get the unminified content
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
                    double reductionPercentage = originalSize > 0 ? (1 - ((double)minifiedSize / originalSize)) * 100 : 0;
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
            var result = NUglify.Uglify.Js(javascript);

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
            var result = NUglify.Uglify.Css(css);

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
    }
} 