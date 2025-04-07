using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dyna.Player.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dyna.Player.TagHelpers
{
    [HtmlTargetElement("bundle", TagStructure = TagStructure.WithoutEndTag)]
    public class BundleTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BundleTagHelper> _logger;

        public BundleTagHelper(
            IHttpContextAccessor httpContextAccessor,
            ILogger<BundleTagHelper> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string Type { get; set; } // "css" or "js"
        
        [HtmlAttributeName("debug")]
        public bool? Debug { get; set; } // Optional debug attribute for development

        [HtmlAttributeName("bundle-type")]
        public string BundleType { get; set; } = "components"; // Can be "components", "libraries", "caching", or "creative"

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                // Get the creative ID from the route data
                var routeData = _httpContextAccessor.HttpContext.GetRouteData();
                var creativeId = routeData?.Values["id"]?.ToString();
                
                // Get viewType from the path since it's not in route data
                var path = _httpContextAccessor.HttpContext.Request.Path.Value;
                var viewType = path?.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                if (string.IsNullOrEmpty(creativeId) || string.IsNullOrEmpty(viewType))
                {
                    _logger.LogError("Missing required values. CreativeId: {CreativeId}, ViewType: {ViewType}, Path: {Path}", 
                        creativeId, viewType, path);
                    output.SuppressOutput();
                    return;
                }

                // Get debug mode from query string
                var debugMode = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("debug");

                // Get bundle type from attribute
                var bundleTypeValue = BundleType.ToLower();

                // Generate the bundle URL
                string bundleUrl = $"/{creativeId}.{bundleTypeValue}.bundle{(debugMode ? "" : ".min")}.{Type.ToLower()}";

                _logger.LogInformation(
                    "Generating bundle URL. Path: {Path}, ViewType: {ViewType}, CreativeId: {CreativeId}, BundleType: {BundleType}, Debug: {DebugMode}", 
                    bundleUrl, viewType, creativeId, bundleTypeValue, debugMode);

                // Create the appropriate tag based on the type
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
                    _logger.LogWarning("Invalid bundle type: {Type}", Type);
                    output.SuppressOutput();
                    return;
                }
                
                if (debugMode)
                {
                    // Add a comment before the tag in debug mode to help identify the bundle
                    output.PreElement.SetHtmlContent($"<!-- Dynamic {bundleTypeValue} bundle of type {Type} for creative ID: {creativeId} (debug mode: enabled) -->\n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bundle tag");
                output.SuppressOutput();
            }
        }
    }
} 