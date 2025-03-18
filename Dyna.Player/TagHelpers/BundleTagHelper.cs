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
                             _httpContextAccessor.HttpContext.Request.Query["debug"] != "");
            
            // Extract the creative ID from the route path
            string path = _httpContextAccessor.HttpContext.Request.Path;
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
            
            // Generate the bundle URL based on the creative ID
            string bundleUrl = $"/{Type}/bundle/{creativeId}{(debugMode ? "" : ".min")}.{Type}";
            
            // Log the debug mode and URL information
            System.Diagnostics.Debug.WriteLine($"Bundle URL: {bundleUrl}, debug mode: {debugMode}, creative ID: {creativeId}, path: {path}");
            
            if (Type.ToLower() == "css")
            {
                output.TagName = "link";
                output.Attributes.SetAttribute("rel", "stylesheet");
                output.Attributes.SetAttribute("href", bundleUrl);
                output.TagMode = TagMode.SelfClosing;
            }
            else if (Type.ToLower() == "js")
            {
                output.TagName = "script";
                output.Attributes.SetAttribute("src", bundleUrl);
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetHtmlContent(""); // Empty content between tags
            }
            else
            {
                output.SuppressOutput();
            }
            
            if (debugMode)
            {
                // Add a comment before the tag in debug mode to help identify the bundle
                output.PreElement.SetHtmlContent($"<!-- Dynamic bundle of type {Type} for creative ID: {creativeId} (debug mode: enabled) -->\n");
            }
        }
    }
} 