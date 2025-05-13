using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dyna.Player.Services;
using Dyna.Shared.Classes.Content;
using System.Reflection;
using System.Collections.Generic;
using Dyna.Shared.Classes.Components.Widgets;

namespace Dyna.Player.Models
{
    public class CreativeModel : PageModel
    {
        private readonly ApiService _apiService;
        private readonly ILogger<CreativeModel> _logger;

        public CreativeModel(
            ApiService apiService, 
            FileService fileService, 
            CreativeCacheService cacheService = null,
            ILogger<CreativeModel> logger = null)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public CreativeClass Creative { get; set; }

        public async Task<IActionResult> OnGetAsync(string id, bool mergeDefaults = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger?.LogWarning("[CreativeModel] ID is null or empty.");
                return NotFound();
            }

            try
            {
                string apiUrl = $"https://localhost:7230/Content/Get/Creatives/{id}"+"/?nested=true";
                Creative = await _apiService.GetAsync<CreativeClass>(apiUrl);

                if (Creative == null)
                {
                    _logger?.LogWarning("[CreativeModel] Creative is null after deserialization.");
                    return NotFound();
                }

                // Preprocess the creative data to ensure media assets are properly structured
                PreprocessCreativeData(Creative);

                return Page();
            }
            catch (Exception ex)
            {
                _logger?.LogError("[CreativeModel] Error: {ErrorMessage}", ex.Message);
                return StatusCode(500, "[CreativeModel] An error occurred while getting the Creative data.");
            }
        }

        private void PreprocessCreativeData(CreativeClass creative)
        {
            if (creative?.Elements == null) return;

            foreach (var piece in creative.Elements)
            {
                if (piece?.SlideLayout != null)
                {
                    PreprocessComponent(piece.SlideLayout);
                }
            }
        }

        private void PreprocessComponent(object component)
        {
            if (component == null) return;

            var type = component.GetType();

            // Handle ImageWidgetClass and VideoWidgetClass specifically
            if (component is ImageWidgetClass imageWidget)
            {
                ProcessMediaWidgetSource(imageWidget.Source);
            }
            else if (component is VideoWidgetClass videoWidget)
            {
                ProcessMediaWidgetSource(videoWidget.Source);
            }
            else
            {
                // Process Source property for other components
                var sourceProperty = type.GetProperty("Source", BindingFlags.Public | BindingFlags.Instance);
                if (sourceProperty != null)
                {
                    ProcessSourceProperty(component, sourceProperty);
                }
            }

            // Process Styles property
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
                            // Extract and normalize the URL in the background style
                            string url = ExtractUrlFromCssValue(style.Value);
                            if (!string.IsNullOrEmpty(url))
                            {
                                if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("/"))
                                {
                                    url = "/" + url;
                                    styles[style.Key] = style.Value.Replace(ExtractUrlFromCssValue(style.Value), url);
                                }
                            }
                        }
                    }
                }
            }

            // Recursively process Contents
            var contentsProperty = type.GetProperty("Contents");
            if (contentsProperty != null)
            {
                var contents = contentsProperty.GetValue(component) as System.Collections.IEnumerable;
                if (contents != null)
                {
                    foreach (var item in contents)
                    {
                        if (item == null) continue;

                        var itemType = item.GetType();
                        foreach (var prop in itemType.GetProperties())
                        {
                            var propValue = prop.GetValue(item);
                            if (propValue != null)
                            {
                                PreprocessComponent(propValue);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessMediaWidgetSource(object source)
        {
            if (source == null) return;

            // Handle Source object
            var urlProperty = source.GetType().GetProperty("Url", BindingFlags.Public | BindingFlags.Instance);
            if (urlProperty != null)
            {
                var urlValue = urlProperty.GetValue(source) as string;
                if (!string.IsNullOrEmpty(urlValue) && 
                    !urlValue.StartsWith("http://") && 
                    !urlValue.StartsWith("https://") && 
                    !urlValue.StartsWith("/"))
                {
                    urlProperty.SetValue(source, "/" + urlValue);
                }
            }
        }

        private void ProcessSourceProperty(object component, PropertyInfo sourceProperty)
        {
            var sourceValue = sourceProperty.GetValue(component);
            if (sourceValue == null) return;

            var sourceType = sourceValue.GetType();
            
            // Handle string source
            if (sourceType == typeof(string))
            {
                string sourceString = (string)sourceValue;
                if (!string.IsNullOrEmpty(sourceString) && 
                    !sourceString.StartsWith("http://") && 
                    !sourceString.StartsWith("https://") && 
                    !sourceString.StartsWith("/"))
                {
                    sourceProperty.SetValue(component, "/" + sourceString);
                }
            }
            // Handle Source object
            else
            {
                var urlProperty = sourceType.GetProperty("Url", BindingFlags.Public | BindingFlags.Instance);
                if (urlProperty != null)
                {
                    var urlValue = urlProperty.GetValue(sourceValue) as string;
                    if (!string.IsNullOrEmpty(urlValue) && 
                        !urlValue.StartsWith("http://") && 
                        !urlValue.StartsWith("https://") && 
                        !urlValue.StartsWith("/"))
                    {
                        urlProperty.SetValue(sourceValue, "/" + urlValue);
                    }
                }
            }
        }

        private string ExtractUrlFromCssValue(string cssValue)
        {
            var match = System.Text.RegularExpressions.Regex.Match(cssValue, @"url\(['""]?(.*?)['""]?\)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
    }
}