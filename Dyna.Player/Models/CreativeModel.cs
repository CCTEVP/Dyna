using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Dyna.Player.Services;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using Dyna.Player.Pages.Shared.Components.CardWidget;
using Dyna.Player.Pages.Shared.Components.CountdownWidget;
using Dyna.Player.Pages.Shared.Components.BoxLayout;
using Dyna.Player.Pages.Shared.Components.SlideLayout;
using Dyna.Player.Pages.Shared.Components.TextWidget;
using Dyna.Player.Pages.Shared.Components.VideoWidget;
using Dyna.Player.Pages.Shared.Components.ImageWidget;

namespace Dyna.Player.Models
{
    public class CreativeModel : PageModel
    {
        private readonly ApiService _apiService;
        private readonly FileService _fileService;

        public CreativeModel(ApiService apiService, FileService fileService)
        {
            _apiService = apiService;
            _fileService = fileService;
        }

        public CreativeClass Creative { get; set; }

        public async Task<IActionResult> OnGetAsync(string id, bool mergeDefaults = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.WriteLine("[CreativeModel] ID is null or empty.");
                return NotFound();
            }

            try
            {
                string apiUrl = "https://localhost:7193/data/structure_123456789.json";
                Creative = await _apiService.GetAsync<CreativeClass>(apiUrl);

                if (Creative == null)
                {
                    Debug.WriteLine("[CreativeModel] Creative is null after deserialization. Check JSON and model classes.");
                    return NotFound();
                }

                if (mergeDefaults)
                {
                    await MergeDefaultComponentDefinitions(Creative);
                }

                var serializedCreative = JsonConvert.SerializeObject(Creative, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                Debug.WriteLine($"[CreativeModel] Cleaned and Merged Creative Model (Newtonsoft.Json):\n{serializedCreative}");

                return Page();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreativeModel] Error: {ex.Message}");
                return StatusCode(500, "An error occurred.");
            }
        }

        private async Task<object> GetObjectDefinitionFromJsonAsync(object componentDefinition)
        {
            if (componentDefinition == null) return null;

            var typeName = componentDefinition.GetType().Name;
            // Remove the "Class" suffix if it exists
            var componentName = typeName.EndsWith("Class") ? typeName.Substring(0, typeName.Length - 5) : typeName;

            return await _fileService.GetObjectDefinitionFromJsonAsync(componentDefinition.GetType(), componentName);
        }
        private void MergeMissingProperties(object sourceObject, object defaultObject)
        {
            if (sourceObject == null || defaultObject == null) return;

            var sourceType = sourceObject.GetType();
            var defaultType = defaultObject.GetType();

            Debug.WriteLine($"[CreativeModel] Merging properties for {sourceType.Name}");

            foreach (var property in defaultType.GetProperties())
            {
                try
                {
                    var sourceValue = property.GetValue(sourceObject);
                    var defaultValue = property.GetValue(defaultObject);

                    if (property.Name == "Styles")
                    {
                        if (sourceValue is Dictionary<string, string> sourceDict && 
                            defaultValue is Dictionary<string, string> defaultDict)
                        {
                            // If source has no styles dictionary, create one
                            if (sourceDict == null)
                            {
                                sourceDict = new Dictionary<string, string>();
                                property.SetValue(sourceObject, sourceDict);
                            }

                            // Only add styles that don't exist in source
                            foreach (var style in defaultDict)
                            {
                                if (!sourceDict.ContainsKey(style.Key))
                                {
                                    sourceDict[style.Key] = style.Value;
                                }
                            }
                        }
                    }
                    else if (sourceValue == null && defaultValue != null)
                    {
                        property.SetValue(sourceObject, defaultValue);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CreativeModel] Error merging property {property.Name}: {ex.Message}");
                }
            }
        }

        private async Task MergeDefaultComponentDefinitions(CreativeClass creative)
        {
            Debug.WriteLine("[CreativeModel] Starting MergeDefaultComponentDefinitions");

            // Skip Pieces (container class) and process its contents directly
            if (creative.Pieces != null)
            {
                foreach (var piece in creative.Pieces)
                {
                    // Process SlideLayout directly if it exists
                    if (piece.SlideLayout != null)
                    {
                        Debug.WriteLine("[CreativeModel] Processing SlideLayout");
                        await MergeComponentRecursively(piece.SlideLayout);
                    }
                }
            }
        }

        private async Task MergeComponentRecursively(object component)
        {
            if (component == null) return;

            var componentType = component.GetType().Name;
            var componentName = componentType.EndsWith("Class")
                ? componentType.Substring(0, componentType.Length - 5)
                : componentType;

            Debug.WriteLine($"[CreativeModel] ====== Processing {componentName} ======");

            switch (component)
            {
                case CountdownWidgetClass countdown:
                    // First merge countdown defaults
                    await MergeComponentDefaults(countdown);

                    if (countdown.Contents != null)
                    {
                        foreach (var content in countdown.Contents)
                        {
                            if (content.BoxLayout != null)
                            {
                                // Merge BoxLayout defaults
                                await MergeComponentDefaults(content.BoxLayout);

                                if (content.BoxLayout.Contents != null)
                                {
                                    foreach (var boxContent in content.BoxLayout.Contents)
                                    {
                                        if (boxContent.TextWidget != null)
                                        {
                                            // For TextWidgets, only add missing properties from defaults
                                            var textDefaults = await GetObjectDefinitionFromJsonAsync(boxContent.TextWidget);
                                            if (textDefaults != null)
                                            {
                                                Debug.WriteLine($"[CreativeModel] Processing TextWidget with current styles: {JsonConvert.SerializeObject(boxContent.TextWidget.Styles)}");

                                                // Only add styles that don't exist in the source
                                                if (boxContent.TextWidget.Styles == null)
                                                {
                                                    boxContent.TextWidget.Styles = new Dictionary<string, string>();
                                                }

                                                var defaultStyles = (textDefaults as TextWidgetClass)?.Styles;
                                                if (defaultStyles != null)
                                                {
                                                    foreach (var style in defaultStyles)
                                                    {
                                                        if (!boxContent.TextWidget.Styles.ContainsKey(style.Key))
                                                        {
                                                            Debug.WriteLine($"[CreativeModel] Adding missing style {style.Key}: {style.Value} to TextWidget");
                                                            boxContent.TextWidget.Styles[style.Key] = style.Value;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (boxContent.CardWidget != null)
                                        {
                                            // For CardWidgets, only add missing properties from defaults
                                            var cardDefaults = await GetObjectDefinitionFromJsonAsync(boxContent.CardWidget);
                                            if (cardDefaults != null)
                                            {
                                                Debug.WriteLine($"[CreativeModel] Processing CardWidget with current styles: {JsonConvert.SerializeObject(boxContent.CardWidget.Styles)}");

                                                // Only add styles that don't exist in the source
                                                if (boxContent.CardWidget.Styles == null)
                                                {
                                                    boxContent.CardWidget.Styles = new Dictionary<string, string>();
                                                }

                                                var defaultStyles = (cardDefaults as CardWidgetClass)?.Styles;
                                                if (defaultStyles != null)
                                                {
                                                    foreach (var style in defaultStyles)
                                                    {
                                                        if (!boxContent.CardWidget.Styles.ContainsKey(style.Key))
                                                        {
                                                            Debug.WriteLine($"[CreativeModel] Adding missing style {style.Key}: {style.Value} to CardWidget");
                                                            boxContent.CardWidget.Styles[style.Key] = style.Value;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case SlideLayoutClass slideLayout:
                    await MergeComponentDefaults(slideLayout);
                    if (slideLayout.Contents != null)
                    {
                        foreach (var content in slideLayout.Contents)
                        {
                            await ProcessElementContainer(content);
                        }
                    }
                    break;

                default:
                    await MergeComponentDefaults(component);
                    break;
            }
        }

        private async Task MergeComponentDefaults(object component)
        {
            if (component == null) return;
            
            var defaults = await GetObjectDefinitionFromJsonAsync(component);
            if (defaults != null)
            {
                Debug.WriteLine($"[CreativeModel] Merging defaults for {component.GetType().Name}");
                MergePropertiesRecursively(component, defaults);
            }
        }

        private void MergePropertiesRecursively(object source, object defaults)
        {
            if (source == null || defaults == null) return;

            Debug.WriteLine($"[CreativeModel] Merging properties for {source.GetType().Name}");

            try
            {
                foreach (var property in defaults.GetType().GetProperties())
                {
                    try
                    {
                        var sourceValue = property.GetValue(source);
                        var defaultValue = property.GetValue(defaults);

                        if (property.Name == "Styles")
                        {
                            MergeStyles(source, sourceValue as Dictionary<string, string>, defaultValue as Dictionary<string, string>);
                        }
                        else if (property.Name == "Contents" && sourceValue != null && defaultValue != null)
                        {
                            // Handle Contents array merging
                            var sourceContents = sourceValue as System.Collections.IList;
                            var defaultContents = defaultValue as System.Collections.IList;

                            if (sourceContents != null && defaultContents != null)
                            {
                                Debug.WriteLine($"[CreativeModel] Merging Contents arrays - Source count: {sourceContents.Count}, Default count: {defaultContents.Count}");

                                // Only process up to the length of the source array
                                for (int i = 0; i < sourceContents.Count && i < defaultContents.Count; i++)
                                {
                                    var sourceItem = sourceContents[i];
                                    var defaultItem = defaultContents[i];

                                    if (sourceItem != null && defaultItem != null)
                                    {
                                        Debug.WriteLine($"[CreativeModel] Merging content item {i} - Source type: {sourceItem.GetType().Name}, Default type: {defaultItem.GetType().Name}");

                                        // Only merge if the types match
                                        if (sourceItem.GetType() == defaultItem.GetType())
                                        {
                                            MergePropertiesRecursively(sourceItem, defaultItem);
                                        }
                                        else
                                        {
                                            Debug.WriteLine($"[CreativeModel] Warning: Content type mismatch at index {i}");
                                        }
                                    }
                                }
                            }
                        }
                        else if (sourceValue == null && defaultValue != null)
                        {
                            Debug.WriteLine($"[CreativeModel] Adding missing property {property.Name} from defaults");
                            property.SetValue(source, defaultValue);
                        }
                        else if (sourceValue != null && defaultValue != null &&
                                 !property.PropertyType.IsPrimitive &&
                                 property.PropertyType != typeof(string))
                        {
                            // Only merge complex objects of the same type
                            if (sourceValue.GetType() == defaultValue.GetType())
                            {
                                MergePropertiesRecursively(sourceValue, defaultValue);
                            }
                            else
                            {
                                Debug.WriteLine($"[CreativeModel] Warning: Type mismatch for property {property.Name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[CreativeModel] Error processing property {property.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreativeModel] Error in MergePropertiesRecursively: {ex.Message}");
            }
        }

        private void MergeStyles(object source, Dictionary<string, string> sourceStyles, Dictionary<string, string> defaultStyles)
        {
            try
            {
                if (defaultStyles == null) return;

                if (sourceStyles == null)
                {
                    sourceStyles = new Dictionary<string, string>();
                    var stylesProperty = source.GetType().GetProperty("Styles");
                    if (stylesProperty != null && stylesProperty.CanWrite)
                    {
                        stylesProperty.SetValue(source, sourceStyles);
                    }
                }

                foreach (var style in defaultStyles)
                {
                    if (!sourceStyles.ContainsKey(style.Key))
                    {
                        Debug.WriteLine($"[CreativeModel] Adding style {style.Key}: {style.Value} to {source.GetType().Name}");
                        sourceStyles[style.Key] = style.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreativeModel] Error in MergeStyles: {ex.Message}");
            }
        }

        private async Task ProcessElementContainer(ElementContainerClass container)
        {
            if (container == null) return;

            Debug.WriteLine("[CreativeModel] Processing ElementContainer");

            // Process each possible widget type
            if (container.TextWidget != null)
            {
                Debug.WriteLine("[CreativeModel] Processing TextWidget");
                await MergeComponentRecursively(container.TextWidget);
            }
            if (container.BoxLayout != null)
            {
                Debug.WriteLine("[CreativeModel] Processing BoxLayout");
                await MergeComponentRecursively(container.BoxLayout);
            }
            if (container.CountdownWidget != null)
            {
                Debug.WriteLine("[CreativeModel] Processing CountdownWidget");
                await MergeComponentRecursively(container.CountdownWidget);
            }
            if (container.ImageWidget != null)
            {
                Debug.WriteLine("[CreativeModel] Processing ImageWidget");
                await MergeComponentRecursively(container.ImageWidget);
            }
            if (container.VideoWidget != null)
            {
                Debug.WriteLine("[CreativeModel] Processing VideoWidget");
                await MergeComponentRecursively(container.VideoWidget);
            }
            if (container.CardWidget != null)
            {
                Debug.WriteLine("[CreativeModel] Processing CardWidget");
                await MergeComponentRecursively(container.CardWidget);
            }
        }
    }

}