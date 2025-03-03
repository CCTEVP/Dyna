using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;

namespace Dyna.Player.TagHelpers
{
    public class AssetTagHelper : TagHelper
    {
        private static Dictionary<string, string> _presentAssets = new Dictionary<string, string>(); // Use a Dictionary

        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string AssetLocation { get; set; }

        public static void AddPresentAsset(string assetName, string assetLocation) // Modified method
        {
            _presentAssets[assetName] = assetLocation;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;

            

            if (_presentAssets.ContainsKey(AssetName))
            {
                AssetLocation = _presentAssets[AssetName]; //get the asset location from the Dictionary.

                if (AssetType == "css") // Custom styles
                {
                    if (AssetLocation == "widget")
                    {
                        output.Content.AppendHtml($"<link rel=\"stylesheet\" href=\"/css/widgets/Custom_{AssetName}.css\" />");
                    }
                    else if (AssetLocation == "layout")
                    {
                        output.Content.AppendHtml($"<link rel=\"stylesheet\" href=\"/css/layouts/Custom_{AssetName}.css\" />");
                    }
                }
                else if (AssetType == "js") // Shadow DOM for encapsulation
                {
                    if (AssetLocation == "widget")
                    {
                        output.Content.AppendHtml($"<script src=\"/js/widget/{AssetName}.js\" defer async></script>");
                    }
                    else if (AssetLocation == "layout")
                    {
                        output.Content.AppendHtml($"<script src=\"/js/layout/{AssetName}.js\" defer async></script>");
                    }
                }
            }
        }
    }
}