using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dyna.Player.TagHelpers
{
    [HtmlTargetElement("asset", TagStructure = TagStructure.WithoutEndTag)]
    public class AssetTagHelper : TagHelper
    {
        private static readonly HashSet<AssetInfo> _presentAssets = new HashSet<AssetInfo>();
        private static ILogger _logger;

        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string AssetLocation { get; set; }

        private readonly ILogger<AssetTagHelper> _instanceLogger;

        public AssetTagHelper(ILogger<AssetTagHelper> logger)
        {
            _instanceLogger = logger;
            if (_logger == null)
            {
                _logger = _instanceLogger;
            }
        }

        /// <summary>
        /// Static initialization of logger for static methods
        /// </summary>
        public static void InitializeLogger(ILogger logger)
        {
            _logger = logger;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // This tag helper is no longer used to render individual assets
            // It's only used for tracking which assets are present
            // The actual rendering is now handled by the BundleTagHelper
            output.SuppressOutput();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds an asset to the list of present assets
        /// </summary>
        public static void AddPresentAsset(string assetName, string assetLocation)
        {
            // Add both CSS and JS assets
            _presentAssets.Add(new AssetInfo { AssetName = assetName, AssetType = "css", AssetLocation = assetLocation });
            _presentAssets.Add(new AssetInfo { AssetName = assetName, AssetType = "js", AssetLocation = assetLocation });
        }

        /// <summary>
        /// Adds a library asset to the list of present assets with a specified priority
        /// </summary>
        /// <param name="assetName">The full path to the library asset (e.g., "Libraries/Creative/CreativeTicker")</param>
        /// <param name="assetType">The type of asset, use "library" for library assets</param>
        /// <param name="priority">Priority for ordering assets (lower numbers load first)</param>
        public static void AddPresentAsset(string assetName, string assetType, int priority)
        {
            _logger?.LogDebug("AddPresentAsset called with: {AssetName}, {AssetType}, {Priority}", assetName, assetType, priority);
            
            // For library assets, we need to handle the path differently
            if (assetType == "library")
            {
                // For CreativeTicker specifically, use the exact path
                if (assetName.Contains("CreativeTicker"))
                {
                    _presentAssets.Add(new AssetInfo { 
                        AssetName = "CreativeTicker", 
                        AssetType = "js", 
                        AssetLocation = "Libraries/Creative", 
                        Priority = priority 
                    });
                    _logger?.LogDebug("Added CreativeTicker asset: Libraries/Creative/CreativeTicker.js (Priority: {Priority})", priority);
                }
                // For WidgetAnimations, use the exact path
                else if (assetName.Contains("WidgetAnimations"))
                {
                    string fileName = assetName.Split('/').Last();
                    _presentAssets.Add(new AssetInfo { 
                        AssetName = fileName, 
                        AssetType = "js", 
                        AssetLocation = "Libraries/WidgetAnimations", 
                        Priority = priority 
                    });
                    _logger?.LogDebug("Added WidgetAnimation asset: Libraries/WidgetAnimations/{FileName}.js (Priority: {Priority})", fileName, priority);
                }
                // For other library assets, use the standard approach
                else
                {
                    string[] parts = assetName.Split('/');
                    string fileName = parts[parts.Length - 1];
                    string directory = string.Join("/", parts.Take(parts.Length - 1));
                    
                    _presentAssets.Add(new AssetInfo { 
                        AssetName = fileName, 
                        AssetType = "js", 
                        AssetLocation = directory, 
                        Priority = priority 
                    });
                    _logger?.LogDebug("Added library asset: {Directory}/{FileName}.js (Priority: {Priority})", directory, fileName, priority);
                }
            }
            else
            {
                // For non-library assets, use the standard approach
                _presentAssets.Add(new AssetInfo { 
                    AssetName = assetName, 
                    AssetType = assetType, 
                    AssetLocation = assetName, 
                    Priority = priority 
                });
                _logger?.LogDebug("Added standard asset: {AssetName}.{AssetType} (Priority: {Priority})", assetName, assetType, priority);
            }
        }

        /// <summary>
        /// Gets the list of present assets
        /// </summary>
        public static IEnumerable<AssetInfo> GetPresentAssets()
        {
            // Debug: Log all assets in the collection
            _logger?.LogDebug("GetPresentAssets called, {Count} assets in collection", _presentAssets.Count);
            
            if (_logger?.IsEnabled(LogLevel.Trace) == true)
            {
                foreach (var asset in _presentAssets)
                {
                    _logger.LogTrace("Present asset: {Location}/{Name}.{Type} (Priority: {Priority})", 
                        asset.AssetLocation, asset.AssetName, asset.AssetType, asset.Priority);
                }
            }
            
            // Filter out any "site" layout assets that might be causing errors
            var filtered = _presentAssets
                .Where(a => !(a.AssetName == "site" && (a.AssetType == "layout" || a.AssetType == "js" || a.AssetType == "css")))
                .OrderBy(a => a.Priority)
                .ToList();
                
            _logger?.LogDebug("Returning {Count} filtered assets", filtered.Count);
            return filtered;
        }

        /// <summary>
        /// Clears the list of present assets
        /// </summary>
        public static void ClearPresentAssets()
        {
            _presentAssets.Clear();
        }
    }

    public class AssetInfo
    {
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string AssetLocation { get; set; }
        public int Priority { get; set; } = 100; // Default priority

        public override bool Equals(object obj)
        {
            if (obj is AssetInfo other)
            {
                return AssetName == other.AssetName && 
                       AssetType == other.AssetType && 
                       AssetLocation == other.AssetLocation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AssetName, AssetType, AssetLocation);
        }
    }
}