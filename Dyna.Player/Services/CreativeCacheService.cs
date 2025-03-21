using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Dyna.Player.Models;
using Dyna.Player.TagHelpers;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Dyna.Player.Services
{
    public class CreativeCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ApiService _apiService;
        private readonly FileService _fileService;
        private readonly ILogger<CreativeCacheService> _logger;

        public CreativeCacheService(
            IMemoryCache cache,
            ApiService apiService,
            FileService fileService,
            ILogger<CreativeCacheService> logger)
        {
            _cache = cache;
            _apiService = apiService;
            _fileService = fileService;
            _logger = logger;
        }

        public class CreativeCacheEntry
        {
            public CreativeClass Creative { get; set; }
            public List<AssetInfo> ComponentAssets { get; set; }
            public List<AssetInfo> LibraryAssets { get; set; }
            public List<string> CachingAssets { get; set; }
            public List<string> ExcludedFromCache { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        public async Task<CreativeCacheEntry> GetOrCreateCreativeEntryAsync(string creativeId, bool mergeDefaults = true)
        {
            string cacheKey = $"creative_full_entry_{creativeId}";

            if (_cache.TryGetValue(cacheKey, out CreativeCacheEntry entry))
            {
                _logger.LogInformation("Retrieved creative entry from cache for ID: {CreativeId}", creativeId);
                return entry;
            }

            _logger.LogInformation("Creating new creative entry for ID: {CreativeId}", creativeId);
            
            // Create a new CreativeModel instance with the correct logger type
            var creativeModel = new CreativeModel(
                _apiService, 
                _fileService, 
                null, 
                _logger as ILogger<CreativeModel>);
            
            // Load the creative
            var result = await creativeModel.OnGetAsync(creativeId, mergeDefaults);
            
            if (result is Microsoft.AspNetCore.Mvc.NotFoundResult || creativeModel.Creative == null)
            {
                _logger.LogWarning("Could not load creative model for ID: {CreativeId}", creativeId);
                return null;
            }

            // Create new cache entry
            entry = new CreativeCacheEntry
            {
                Creative = creativeModel.Creative,
                ComponentAssets = new List<AssetInfo>(),
                LibraryAssets = new List<AssetInfo>(),
                CachingAssets = new List<string>(),
                ExcludedFromCache = new List<string>(),
                LastUpdated = DateTime.UtcNow
            };

            // Clear existing assets before identification
            AssetTagHelper.ClearPresentAssets();

            // Identify all required assets
            await IdentifyRequiredAssets(entry);

            // Cache the entry
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
            
            _cache.Set(cacheKey, entry, cacheOptions);

            return entry;
        }

        private async Task IdentifyRequiredAssets(CreativeCacheEntry entry)
        {
            if (entry?.Creative == null) return;

            // Add base assets
            AddBaseAssets();

            // Analyze creative structure for components and libraries
            await AnalyzeCreativeStructure(entry.Creative);

            // Get all registered assets
            var allAssets = AssetTagHelper.GetPresentAssets();

            // Separate assets by type
            foreach (var asset in allAssets)
            {
                if (asset.AssetLocation.StartsWith("Libraries/"))
                {
                    entry.LibraryAssets.Add(asset);
                }
                else
                {
                    entry.ComponentAssets.Add(asset);
                }
            }

            // Identify caching assets (media files, etc.)
            entry.CachingAssets.AddRange(ExtractMediaFilesFromCreative(entry.Creative));
        }

        private void AddBaseAssets()
        {
            // Add base assets that are always needed
            AssetTagHelper.AddPresentAsset("Libraries/Creative/CreativeTicker", "library", 0);
            AssetTagHelper.AddPresentAsset("Libraries/Creative/WidgetInitializer", "library", 1);

            // Add common animation libraries by default
            AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/CardWidgetAnimation", "library", 100);
            AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/CardWidgetAnimationRoll", "library", 101);
            AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/CardWidgetAnimationFlip", "library", 102);
            AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/TextWidgetAnimation", "library", 103);
            AssetTagHelper.AddPresentAsset("Libraries/WidgetAnimations/ImageWidgetAnimation", "library", 104);
        }

        private async Task AnalyzeCreativeStructure(CreativeClass creative)
        {
            if (creative?.Pieces == null) return;
            
            // Get available components from the /Pages/Shared/Components directory
            var availableComponents = GetAvailableComponents();
            
            // Add base assets first
            AddBaseAssets();
            
            foreach (var piece in creative.Pieces)
            {
                // Check for SlideLayout
                if (piece?.SlideLayout != null)
                {
                    _logger.LogInformation("Found SlideLayout in creative");
                    if (availableComponents.Contains("SlideLayout"))
                    {
                        AssetTagHelper.AddPresentAsset("SlideLayout", "layout");
                    }
                    
                    // Analyze SlideLayout contents
                    await AnalyzeComponentWithDirectoryCheck(piece.SlideLayout, availableComponents);
                }
            }
        }

        private List<string> GetAvailableComponents()
        {
            var componentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Pages", "Shared", "Components");
            var componentsList = new List<string>();
            
            if (Directory.Exists(componentsPath))
            {
                var directories = Directory.GetDirectories(componentsPath);
                foreach (var directory in directories)
                {
                    var dirName = new DirectoryInfo(directory).Name;
                    componentsList.Add(dirName);
                    _logger.LogInformation("Found component directory: {DirectoryName}", dirName);
                }
            }
            else
            {
                _logger.LogWarning("Components directory not found: {Path}", componentsPath);
            }
            
            return componentsList;
        }

        private async Task AnalyzeComponentWithDirectoryCheck(object component, List<string> availableComponents)
        {
            // Use reflection to analyze the structure
            var type = component.GetType();
            
            // Check if this is a widget or layout by name
            string typeName = type.Name;
            if (typeName.EndsWith("Class"))
            {
                typeName = typeName.Substring(0, typeName.Length - 5);
            }
            
            if (typeName.EndsWith("Widget") && availableComponents.Contains(typeName))
            {
                _logger.LogInformation("Found widget: {WidgetName}", typeName);
                AssetTagHelper.AddPresentAsset(typeName, "widget");
                
                // Check if we have corresponding animation libraries
                CheckForAnimationLibraries(typeName);
            }
            else if (typeName.EndsWith("Layout") && availableComponents.Contains(typeName))
            {
                _logger.LogInformation("Found layout: {LayoutName}", typeName);
                AssetTagHelper.AddPresentAsset(typeName, "layout");
            }
            
            // Look for Contents property which might contain more widgets/layouts
            var contentsProperty = type.GetProperty("Contents");
            if (contentsProperty != null)
            {
                var contents = contentsProperty.GetValue(component) as System.Collections.IEnumerable;
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
                                
                                if ((propTypeName.EndsWith("Widget") || propTypeName.EndsWith("Layout")) 
                                    && availableComponents.Contains(propTypeName))
                                {
                                    _logger.LogInformation("Found component in contents: {ComponentName}", propTypeName);
                                    
                                    if (propTypeName.EndsWith("Widget"))
                                    {
                                        AssetTagHelper.AddPresentAsset(propTypeName, "widget");
                                        
                                        // Check if we have corresponding animation libraries
                                        CheckForAnimationLibraries(propTypeName);
                                    }
                                    else if (propTypeName.EndsWith("Layout"))
                                    {
                                        AssetTagHelper.AddPresentAsset(propTypeName, "layout");
                                    }
                                    
                                    // Recursively analyze this component
                                    await AnalyzeComponentWithDirectoryCheck(propValue, availableComponents);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckForAnimationLibraries(string widgetName)
        {
            // Check for animation libraries for this widget
            var animationLibraryBaseName = $"Libraries/WidgetAnimations/{widgetName}Animation";
            var animationLibraryPath = Path.Combine(Directory.GetCurrentDirectory(), "Pages", "Shared", animationLibraryBaseName.Replace("/", Path.DirectorySeparatorChar.ToString()));
            
            if (File.Exists($"{animationLibraryPath}.js"))
            {
                _logger.LogInformation("Found animation library for {WidgetName}: {LibraryPath}", widgetName, animationLibraryBaseName);
                AssetTagHelper.AddPresentAsset($"{animationLibraryBaseName}", "library", 100);
            }
            
            // Check for specialized animation variants
            var animationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Pages", "Shared", "Libraries", "WidgetAnimations");
            if (Directory.Exists(animationsDirectory))
            {
                var animationFiles = Directory.GetFiles(animationsDirectory, $"{widgetName}Animation*.js");
                foreach (var file in animationFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName != $"{widgetName}Animation") // Skip the base animation we already checked
                    {
                        _logger.LogInformation("Found animation variant for {WidgetName}: {LibraryPath}", widgetName, fileName);
                        AssetTagHelper.AddPresentAsset($"Libraries/WidgetAnimations/{fileName}", "library", 101);
                    }
                }
            }
        }

        private List<string> ExtractMediaFilesFromCreative(CreativeClass creative)
        {
            var mediaFiles = new HashSet<string>();
            
            if (creative?.Pieces == null) return mediaFiles.ToList();
            
            foreach (var piece in creative.Pieces)
            {
                if (piece?.SlideLayout != null)
                {
                    // Extract media files from SlideLayout
                    ExtractMediaFilesFromComponent(piece.SlideLayout, mediaFiles);
                }
            }
            
            return mediaFiles.ToList();
        }

        private void ExtractMediaFilesFromComponent(object component, HashSet<string> mediaFiles)
        {
            if (component == null) return;
            
            var type = component.GetType();
            
            // Check for source properties that might contain media URLs
            var sourceProperty = type.GetProperty("Source");
            if (sourceProperty != null)
            {
                var source = sourceProperty.GetValue(component) as string;
                if (!string.IsNullOrEmpty(source) && IsMediaUrl(source))
                {
                    var normalizedUrl = NormalizeMediaUrl(source);
                    mediaFiles.Add(normalizedUrl);
                }
            }
            
            // Check for Styles that might contain background images
            var stylesProperty = type.GetProperty("Styles");
            if (stylesProperty != null)
            {
                var styles = stylesProperty.GetValue(component) as Dictionary<string, string>;
                if (styles != null)
                {
                    foreach (var style in styles)
                    {
                        if (style.Key.Contains("background", StringComparison.OrdinalIgnoreCase) && 
                            style.Value.Contains("url(", StringComparison.OrdinalIgnoreCase))
                        {
                            // Extract URLs from background style
                            string url = ExtractUrlFromCssValue(style.Value);
                            if (!string.IsNullOrEmpty(url))
                            {
                                var normalizedUrl = NormalizeMediaUrl(url);
                                mediaFiles.Add(normalizedUrl);
                            }
                        }
                    }
                }
            }
            
            // Look for Contents property for recursive search
            var contentsProperty = type.GetProperty("Contents");
            if (contentsProperty != null)
            {
                var contents = contentsProperty.GetValue(component) as System.Collections.IEnumerable;
                if (contents != null)
                {
                    foreach (var item in contents)
                    {
                        if (item == null) continue;
                        
                        // Check all properties of this item for nested components
                        var itemType = item.GetType();
                        foreach (var prop in itemType.GetProperties())
                        {
                            var propValue = prop.GetValue(item);
                            if (propValue != null)
                            {
                                ExtractMediaFilesFromComponent(propValue, mediaFiles);
                            }
                        }
                    }
                }
            }
        }

        private bool IsMediaUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            
            // Check for common media file extensions
            string[] mediaExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".mp4", ".webm", ".ogg", ".mp3", ".wav", ".ttf", ".otf", ".woff", ".woff2" };
            
            return mediaExtensions.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
        
        private string NormalizeMediaUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            
            // If it's already a full URL with http/https, use as is
            if (url.StartsWith("http://") || url.StartsWith("https://")) return url;
            
            // Handle URLs that start with a slash (they are relative to the domain root)
            if (url.StartsWith("/"))
            {
                return url;
            }
            
            // Otherwise assume it's relative to the creative
            return $"/media/{url}";
        }
        
        private string ExtractUrlFromCssValue(string cssValue)
        {
            // Extract URL from css url() function
            var match = Regex.Match(cssValue, @"url\(['""]?(.*?)['""]?\)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        private List<string> GetStandardCachingUrls(string creativeId, string viewType, bool isDebug)
        {
            // Ensure URLs start with a forward slash and use the new bundle naming convention
            var urls = new List<string>
            {
                $"/{viewType}/{creativeId}",
                $"/{viewType}/{creativeId}/js/components.bundle.min.js",
                $"/{viewType}/{creativeId}/js/libraries.bundle.min.js",
                $"/{viewType}/{creativeId}/css/components.bundle.min.css",
                $"/{viewType}/{creativeId}/css/libraries.bundle.min.css",
                "/offline.html",
                "/lib/jquery/dist/jquery.min.js",
                "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
                "/lib/bootstrap/dist/css/bootstrap.min.css"
            };

            if (isDebug)
            {
                urls.Add($"/{viewType}/{creativeId}?debug=true");
                urls.Add($"/{viewType}/{creativeId}/js/components.bundle.js");
                urls.Add($"/{viewType}/{creativeId}/js/libraries.bundle.js");
                urls.Add($"/{viewType}/{creativeId}/css/components.bundle.css");
                urls.Add($"/{viewType}/{creativeId}/css/libraries.bundle.css");
                urls.Add("/js/sw-debug.js");
            }

            _logger?.LogInformation("Generated standard caching URLs for creative {CreativeId}: {UrlCount} URLs", creativeId, urls.Count);
            foreach (var url in urls)
            {
                _logger?.LogDebug("Caching URL: {Url}", url);
            }

            return urls;
        }
    }
} 