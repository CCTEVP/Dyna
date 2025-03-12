using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dyna.Player.TagHelpers
{
    [HtmlTargetElement("asset", TagStructure = TagStructure.WithoutEndTag)]
    public class AssetTagHelper : TagHelper
    {
        private static readonly HashSet<AssetInfo> _presentAssets = new HashSet<AssetInfo>();

        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string AssetLocation { get; set; }

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
        /// Gets the list of present assets
        /// </summary>
        public static IEnumerable<AssetInfo> GetPresentAssets()
        {
            return _presentAssets;
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