using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dyna.Player.Services;
using Microsoft.AspNetCore.Http;

namespace Dyna.Player.TagHelpers
{
    [HtmlTargetElement("bundle", TagStructure = TagStructure.WithoutEndTag)]
    public class BundleTagHelper : TagHelper
    {
        private readonly IBundleService _bundleService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BundleTagHelper(IBundleService bundleService, IHttpContextAccessor httpContextAccessor)
        {
            _bundleService = bundleService;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Type { get; set; } // "css" or "js"
        
        [HtmlAttributeName("debug")]
        public bool? Debug { get; set; } // Optional debug attribute for development

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Get all assets of the specified type
            var assets = AssetTagHelper.GetPresentAssets();
            
            // Check if debug mode is requested via query parameter ONLY
            bool debugMode = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("debug") && 
                            (_httpContextAccessor.HttpContext.Request.Query["debug"] == "true" || 
                             _httpContextAccessor.HttpContext.Request.Query["debug"] == "");
            
            // Log the debug mode determination
            System.Diagnostics.Debug.WriteLine($"Bundle debug mode: {debugMode}, from query parameter");
            
            // Get the bundle URL
            string bundleUrl = null;
            
            if (Type.ToLower() == "css")
            {
                bundleUrl = await _bundleService.GetCssBundleUrlAsync(assets, debugMode);
                output.TagName = "link";
                output.Attributes.SetAttribute("rel", "stylesheet");
                output.Attributes.SetAttribute("href", bundleUrl);
                output.TagMode = TagMode.SelfClosing;
            }
            else if (Type.ToLower() == "js")
            {
                bundleUrl = await _bundleService.GetJsBundleUrlAsync(assets, debugMode);
                output.TagName = "script";
                output.Attributes.SetAttribute("src", bundleUrl);
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetHtmlContent(""); // Empty content between tags
            }
            else
            {
                output.SuppressOutput();
            }
            
            // If no bundle URL was generated, suppress output
            if (string.IsNullOrEmpty(bundleUrl))
            {
                output.SuppressOutput();
            }
            else if (debugMode)
            {
                // Add a comment before the tag in debug mode to help identify the bundle
                output.PreElement.SetHtmlContent($"<!-- Bundle of type {Type} with {assets.Count(a => a.AssetType == Type)} assets (debug mode: query parameter) -->\n");
            }
        }
    }
} 