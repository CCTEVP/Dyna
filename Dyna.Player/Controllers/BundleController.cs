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
using System.Text;
using NUglify;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

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
        private readonly CreativeCacheService _creativeCacheService;
        private readonly IWebHostEnvironment _env;

        public BundleController(
            IAssetService assetService,
            IMemoryCache cache,
            ApiService apiService,
            FileService fileService,
            ILogger<BundleController> logger,
            CreativeCacheService creativeCacheService,
            IWebHostEnvironment env)
        {
            _assetService = assetService;
            _cache = cache;
            _apiService = apiService;
            _fileService = fileService;
            _logger = logger;
            _creativeCacheService = creativeCacheService;
            _env = env;
        }

        [HttpGet]
        [Route("{creativeId}.components.bundle.{assetType:regex(^(js|css)$)}")]
        public async Task<IActionResult> GetComponentsBundle(string creativeId, string assetType)
        {
            return await GetComponentsBundle(creativeId, assetType, true);
        }

        [HttpGet]
        [Route("{creativeId}.components.bundle.min.{assetType:regex(^(js|css)$)}")]
        public async Task<IActionResult> GetComponentsBundleMin(string creativeId, string assetType)
        {
            return await GetComponentsBundle(creativeId, assetType, false);
        }

        [HttpGet]
        [Route("{creativeId}.libraries.bundle.{assetType:regex(^(js|css)$)}")]
        public async Task<IActionResult> GetLibrariesBundle(string creativeId, string assetType)
        {
            return await GetLibrariesBundle(creativeId, assetType, true);
        }

        [HttpGet]
        [Route("{creativeId}.libraries.bundle.min.{assetType:regex(^(js|css)$)}")]
        public async Task<IActionResult> GetLibrariesBundleMin(string creativeId, string assetType)
        {
            return await GetLibrariesBundle(creativeId, assetType, false);
        }

        [HttpGet]
        [Route("{creativeId}.caching.bundle.{assetType:regex(^js$)}")]
        public async Task<IActionResult> GetCachingJsBundle(string creativeId, string assetType)
        {
            return await GetCachingBundle(creativeId, assetType, true);
        }

        [HttpGet]
        [Route("{creativeId}.caching.bundle.min.{assetType:regex(^js$)}")]
        public async Task<IActionResult> GetCachingJsBundleMin(string creativeId, string assetType)
        {
            return await GetCachingBundle(creativeId, assetType, false);
        }

        [HttpGet]
        [Route("{creativeId}.manager.bundle.{assetType:regex(^js$)}")]
        public async Task<IActionResult> GetManagerJsBundle(string creativeId, string assetType)
        {
            return await GetManagerBundle(creativeId, assetType, true);
        }

        [HttpGet]
        [Route("{creativeId}.manager.bundle.min.{assetType:regex(^js$)}")]
        public async Task<IActionResult> GetManagerJsBundleMin(string creativeId, string assetType)
        {
            return await GetManagerBundle(creativeId, assetType, false);
        }

        private async Task<IActionResult> GetComponentsBundle(string creativeId, string type, bool debugMode)
        {
            try
            {
                var bundleContent = new StringBuilder();

                // Add a header comment in debug mode
                if (debugMode)
                {
                    bundleContent.AppendLine($"/* Components bundle generated in debug mode on {DateTime.Now} */");
                    bundleContent.AppendLine($"/* Contains components of type {type} */");
                    bundleContent.AppendLine();
                }

                // Get the creative entry from cache service
                var cacheEntry = await _creativeCacheService.GetOrCreateCreativeEntryAsync(creativeId);
                if (cacheEntry == null)
                {
                    _logger?.LogWarning("Could not load creative cache entry for ID: {CreativeId}. Using default assets.", creativeId);
                    AssetTagHelper.ClearPresentAssets();
                    AddDefaultAssets();
                }

                // Get all component assets (not libraries) of the requested type
                var assets = cacheEntry?.ComponentAssets?
                    .Where(a => a.AssetType == type)
                    .OrderBy(a => a.Priority)
                    .ToList()
                    ?? AssetTagHelper.GetPresentAssets()
                        .Where(a => a.AssetType == type && !a.AssetLocation.StartsWith("Libraries/"))
                        .OrderBy(a => a.Priority)
                        .ToList();

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

                // Add initialization calls for JS components bundle
                if (type == "js" && !assets.Any(a => a.AssetLocation.StartsWith("Libraries/")))
                {
                    bundleContent.AppendLine("\n// Initialize components");

                    // Check for each component type and add its initialization if present
                    if (assets.Any(a => a.AssetName.Contains("CardWidget")))
                        bundleContent.AppendLine("renderCardWidgets();");

                    if (assets.Any(a => a.AssetName.Contains("TextWidget")))
                        bundleContent.AppendLine("renderTextWidgets();");

                    if (assets.Any(a => a.AssetName.Contains("ImageWidget")))
                        bundleContent.AppendLine("renderImageWidgets();");

                    if (assets.Any(a => a.AssetName.Contains("VideoWidget")))
                        bundleContent.AppendLine("renderVideoWidgets();");

                    if (assets.Any(a => a.AssetName.Contains("CountdownWidget")))
                        bundleContent.AppendLine("renderCountdownWidgets();");

                    if (assets.Any(a => a.AssetName.Contains("BoxLayout")))
                        bundleContent.AppendLine("renderBoxLayouts();");

                    if (assets.Any(a => a.AssetName.Contains("SlideLayout")))
                        bundleContent.AppendLine("renderSlideLayouts();");

                    // Trigger service worker registration after all components are initialized
                    bundleContent.AppendLine("\n// Trigger service worker registration");
                    bundleContent.AppendLine($"window.dispatchEvent(new CustomEvent('creative-ready', {{ detail: {{ creativeId: '{creativeId}' }} }}));");

                    result = bundleContent.ToString();
                }

                // Minify only if not in debug mode
                if (!debugMode)
                {
                    try
                    {
                        _logger?.LogInformation("Starting minification for {Type} components bundle with {Count} assets and {Size} bytes",
                            type, assets.Count, result.Length);

                        string beforeMinification = result;

                        result = type.ToLower() switch
                        {
                            "js" => MinifyJavaScript(result),
                            "css" => MinifyCss(result),
                            _ => result
                        };

                        // Check if minification actually changed anything
                        if (result == beforeMinification)
                        {
                            _logger?.LogWarning("Minification did not change content for {Type} components bundle. This might indicate an issue.", type);
                        }

                        // Log the size reduction
                        int originalSize = bundleContent.Length;
                        int minifiedSize = result.Length;
                        double reductionPercentage = originalSize > 0 ? (1 - ((double)minifiedSize / originalSize)) * 100 : 0;

                        _logger?.LogInformation("Minification reduced components bundle size by {Percentage:F2}% ({OriginalSize} → {MinifiedSize} bytes)",
                            reductionPercentage, originalSize, minifiedSize);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error minifying {Type} components bundle", type);
                    }
                }
                else
                {
                    _logger?.LogInformation("Skipping minification for {Type} components bundle (debug mode is {DebugMode})", type, debugMode);
                }

                return Content(result, type == "css" ? "text/css" : "application/javascript");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating {Type} components bundle", type);
                return StatusCode(500, "Error generating bundle");
            }
        }

        private string MinifyJavaScript(string javascript)
        {
            if (string.IsNullOrWhiteSpace(javascript))
            {
                _logger?.LogWarning("Attempted to minify empty or whitespace-only JavaScript content");
                return javascript;
            }

            try
            {
                _logger?.LogInformation("Minifying JavaScript content of length: {Length}", javascript.Length);

                var settings = new NUglify.JavaScript.CodeSettings
                {
                    MinifyCode = true,
                    PreserveImportantComments = false,
                    Format = NUglify.JavaScript.JavaScriptFormat.Normal
                };
                var result = Uglify.Js(javascript, settings);

                if (result.HasErrors)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger?.LogWarning("JS minification error: {Message} at line {Line}, column {Column}",
                            error.Message, error.StartLine, error.StartColumn);
                    }
                    _logger?.LogWarning("Minification had errors - returning original JavaScript code");
                    return javascript;
                }

                if (string.IsNullOrWhiteSpace(result.Code))
                {
                    _logger?.LogWarning("Minification produced empty result - returning original JavaScript code");
                    return javascript;
                }

                _logger?.LogInformation("JavaScript minification successful: {OriginalLength} → {MinifiedLength} bytes",
                    javascript.Length, result.Code.Length);
                return result.Code;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception during JavaScript minification");
                return javascript;
            }
        }

        private string MinifyCss(string css)
        {
            if (string.IsNullOrWhiteSpace(css))
            {
                _logger?.LogWarning("Attempted to minify empty or whitespace-only CSS content");
                return css;
            }

            try
            {
                _logger?.LogInformation("Minifying CSS content of length: {Length}", css.Length);

                var result = Uglify.Css(css);

                if (result.HasErrors)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger?.LogWarning("CSS minification error: {Message} at line {Line}, column {Column}",
                            error.Message, error.StartLine, error.StartColumn);
                    }
                    _logger?.LogWarning("CSS minification had errors - returning original CSS code");
                    return css;
                }

                if (string.IsNullOrWhiteSpace(result.Code))
                {
                    _logger?.LogWarning("CSS minification produced empty result - returning original CSS code");
                    return css;
                }

                _logger?.LogInformation("CSS minification successful: {OriginalLength} → {MinifiedLength} bytes",
                    css.Length, result.Code.Length);
                return result.Code;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception during CSS minification");
                return css;
            }
        }

        private void AddDefaultAssets()
        {
            // Add base assets first
            AddBaseAssets();

            // Add common widget assets
            AssetTagHelper.AddPresentAsset("SlideLayout", "layout");
            AssetTagHelper.AddPresentAsset("BoxLayout", "layout");
            AssetTagHelper.AddPresentAsset("CardWidget", "widget");
            AssetTagHelper.AddPresentAsset("CountdownWidget", "widget");
            AssetTagHelper.AddPresentAsset("TextWidget", "widget");
        }

        private void AddBaseAssets()
        {
            // Add base assets that are always needed
            AssetTagHelper.AddPresentAsset("Libraries/Creative/CreativeTicker", "library", 0);
            // Add the WidgetInitializer with priority 1 (right after CreativeTicker)
            AssetTagHelper.AddPresentAsset("Libraries/Creative/WidgetInitializer", "library", 1);
        }

        private async Task<IActionResult> GetLibrariesBundle(string creativeId, string type, bool debugMode)
        {
            try
            {
                var bundleContent = new StringBuilder();

                if (debugMode)
                {
                    bundleContent.AppendLine($"/* Libraries bundle generated in debug mode on {DateTime.Now} */");
                    bundleContent.AppendLine();
                }

                var cacheEntry = await _creativeCacheService.GetOrCreateCreativeEntryAsync(creativeId);
                if (cacheEntry == null)
                {
                    _logger?.LogWarning("Could not load creative cache entry for ID: {CreativeId}. Using default library assets.", creativeId);
                    AssetTagHelper.ClearPresentAssets();
                    AddBaseAssets();
                }

                var assets = cacheEntry?.LibraryAssets?
                    .Where(a => a.AssetType == type)
                    .OrderBy(a => a.Priority)
                    .ToList()
                    ?? AssetTagHelper.GetPresentAssets()
                        .Where(a => a.AssetLocation.StartsWith("Libraries/") && a.AssetType == type)
                        .OrderBy(a => a.Priority)
                        .ToList();

                foreach (var asset in assets)
                {
                    try
                    {
                        string content = await _assetService.GetAssetAsync(asset.AssetName, asset.AssetLocation, asset.AssetType, true);

                        if (content.StartsWith("Invalid") || content.StartsWith("Non existing"))
                        {
                            _logger?.LogWarning("Skipping library asset {AssetName}: {Content}", asset.AssetName, content);
                            if (debugMode)
                            {
                                bundleContent.AppendLine($"/* Error loading {asset.AssetLocation}/{asset.AssetName}: {content} */");
                            }
                            continue;
                        }

                        if (debugMode)
                        {
                            bundleContent.AppendLine($"/* ===== BEGIN {asset.AssetLocation}/{asset.AssetName} ===== */");
                        }

                        bundleContent.AppendLine(content);

                        if (debugMode)
                        {
                            bundleContent.AppendLine($"/* ===== END {asset.AssetLocation}/{asset.AssetName} ===== */");
                        }

                        bundleContent.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error bundling library asset {AssetName}", asset.AssetName);
                        if (debugMode)
                        {
                            bundleContent.AppendLine($"/* Error processing {asset.AssetLocation}/{asset.AssetName}: {ex.Message} */");
                        }
                    }
                }

                string result = bundleContent.ToString();

                if (!debugMode)
                {
                    try
                    {
                        // Determine content type from the first asset, defaulting to js if no assets
                        var contentType = assets.FirstOrDefault()?.AssetType?.ToLower() ?? "js";
                        result = contentType switch
                        {
                            "js" => MinifyJavaScript(result),
                            "css" => MinifyCss(result),
                            _ => result
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error minifying libraries bundle");
                    }
                }

                // Determine content type from assets collection
                var responseContentType = assets.FirstOrDefault()?.AssetType == "css" ? "text/css" : "application/javascript";
                return Content(result, responseContentType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating libraries bundle");
                return StatusCode(500, "Error generating libraries bundle");
            }
        }

        private async Task<IActionResult> GetCachingBundle(string creativeId, string type, bool debugMode)
        {
            try
            {
                // Service worker bundles are always JavaScript
                if (type.ToLower() != "js")
                {
                    _logger?.LogWarning("Attempted to get caching bundle with type {Type}. Only JavaScript is supported.", type);
                    return StatusCode(400, "Caching bundle only supports JavaScript type");
                }

                // Read the service worker template file
                string templatePath = Path.Combine(_env.ContentRootPath, "Pages", "Shared", "Libraries", "ServiceWorkers", "DefinitionTemplate.js");

                if (!System.IO.File.Exists(templatePath))
                {
                    _logger?.LogError("Service worker definition template file not found at {Path}", templatePath);
                    return StatusCode(500, "Service worker definition template file not found at " + templatePath);
                }

                string serviceWorkerTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

                // Get creative cache entry
                var cacheEntry = await _creativeCacheService.GetOrCreateCreativeEntryAsync(creativeId);
                if (cacheEntry == null)
                {
                    return NotFound($"Creative with ID {creativeId} not found");
                }

                // Build the list of files to cache
                var filesToCache = new List<string>();

                // Add bundle URLs
                string minSuffix = debugMode ? "" : ".min";
                //filesToCache.Add("~/lib/bootstrap/dist/css/bootstrap.min.css");
                //filesToCache.Add("~/css/site.css");
                //filesToCache.Add("~/lib/jquery/dist/jquery.min.js");
                //filesToCache.Add("~/lib/bootstrap/dist/js/bootstrap.bundle.min.js");
                //filesToCache.Add("~/js/site.js");
                filesToCache.Add($"/{creativeId}.components.bundle{minSuffix}.js");
                filesToCache.Add($"/{creativeId}.libraries.bundle{minSuffix}.js");
                filesToCache.Add($"/{creativeId}.libraries.bundle{minSuffix}.css");
                filesToCache.Add($"/{creativeId}.components.bundle{minSuffix}.css");

                // Add media assets from creative cache entry
                if (cacheEntry.CachingAssets != null && cacheEntry.CachingAssets.Any())
                {
                    filesToCache.AddRange(cacheEntry.CachingAssets);
                }

                // Get excluded URLs (if any)
                var excludedUrls = cacheEntry.ExcludedFromCache ?? new List<string>();

                // Replace placeholders in template
                string result = serviceWorkerTemplate
                    .Replace("{{creativeId}}", creativeId)
                    .Replace("{{filesToCache}}", JsonConvert.SerializeObject(filesToCache))
                    .Replace("{{excludedUrls}}", JsonConvert.SerializeObject(excludedUrls))
                    .Replace("{{minSuffix}}", minSuffix);

                // Add debug information if in debug mode
                if (debugMode)
                {
                    var debugComment = new StringBuilder();
                    debugComment.AppendLine("/**");
                    debugComment.AppendLine($" * Service Worker for Creative ID: {creativeId}");
                    debugComment.AppendLine($" * Generated: {DateTime.Now}");
                    debugComment.AppendLine($" * Files to cache: {filesToCache.Count()}");
                    debugComment.AppendLine($" * Excluded patterns: {excludedUrls.Count()}");
                    debugComment.AppendLine(" */");

                    result = debugComment.ToString() + Environment.NewLine + result;
                }

                return Content(result, "application/javascript");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating caching bundle for creative ID {CreativeId}", creativeId);
                return StatusCode(500, "Error generating caching bundle");
            }
        }

        private async Task<IActionResult> GetManagerBundle(string creativeId, string type, bool debugMode)
        {
            try
            {
                // Service worker manager bundles are always JavaScript
                if (type.ToLower() != "js")
                {
                    _logger?.LogWarning("Attempted to get manager bundle with type {Type}. Only JavaScript is supported.", type);
                    return StatusCode(400, "Manager bundle only supports JavaScript type");
                }

                var bundleContent = new StringBuilder();

                if (debugMode)
                {
                    bundleContent.AppendLine($"/* Service Worker Manager bundle generated in debug mode on {DateTime.Now} */");
                    bundleContent.AppendLine();
                }

                // Read the service worker manager code
                string managerPath = Path.Combine(_env.ContentRootPath, "Pages", "Shared", "Libraries", "ServiceWorkers", "ManagerTemplate.js");
                if (!System.IO.File.Exists(managerPath))
                {
                    _logger?.LogError("Service worker manager template file not found at {Path}", managerPath);
                    return StatusCode(500, "Service worker manager template file not found: " + managerPath);
                }

                string managerCode = await System.IO.File.ReadAllTextAsync(managerPath);

                // Replace placeholders with actual values
                managerCode = managerCode
                    .Replace("{{viewType}}", "dynamic")
                    .Replace("{{creativeId}}", creativeId)
                    .Replace("{{minSuffix}}", debugMode ? "" : ".min");

                bundleContent.AppendLine(managerCode);

                // Add auto-registration code
                bundleContent.AppendLine("\n// Auto-register service worker when creative ID is available");
                bundleContent.AppendLine("window.addEventListener('creative-ready', async function(event) {");
                bundleContent.AppendLine("    const creativeId = event.detail.creativeId;");
                bundleContent.AppendLine("    if (creativeId && 'serviceWorker' in navigator) {");
                bundleContent.AppendLine("        try {");
                bundleContent.AppendLine("            await window.swManager.register(creativeId);");
                bundleContent.AppendLine("        } catch (err) {");
                bundleContent.AppendLine("            console.error('ServiceWorker registration failed:', err);");
                bundleContent.AppendLine("        }");
                bundleContent.AppendLine("    }");
                bundleContent.AppendLine("});");

                string result = bundleContent.ToString();

                if (!debugMode)
                {
                    try
                    {
                        var settings = new NUglify.JavaScript.CodeSettings
                        {
                            MinifyCode = true,
                            PreserveImportantComments = false,
                            Format = NUglify.JavaScript.JavaScriptFormat.Normal
                        };
                        var minResult = Uglify.Js(result, settings);
                        if (!minResult.HasErrors)
                        {
                            result = minResult.Code;
                        }
                        else
                        {
                            foreach (var error in minResult.Errors)
                            {
                                _logger?.LogWarning("JS minification error: {Message} at line {Line}, column {Column}",
                                    error.Message, error.StartLine, error.StartColumn);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error minifying manager bundle");
                    }
                }

                return Content(result, "application/javascript");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating manager bundle");
                return StatusCode(500, "Error generating manager bundle");
            }
        }
    }
}