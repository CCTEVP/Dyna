using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dyna.Player.Converters; // Make sure the namespace is correct

namespace Dyna.Player.Models
{
    public class CreativeModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CreativeModel> _logger; // Add ILogger

        public CreativeModel(HttpClient httpClient, ILogger<CreativeModel> logger) // Inject ILogger
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public CreativeData Creative { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                //string apiUrl = $"http://localhost:5230/File/GetById/{id}";
                string apiUrl = $"https://localhost:7193/data/structure_123456789.json";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new WidgetsConverter()); // Add the converter back in!

                    Creative = JsonSerializer.Deserialize<CreativeData>(json, options);

                    if (Creative == null)
                    {
                        _logger.LogError("Creative is null after deserialization. Check JSON and model classes.");
                        return NotFound();
                    }

                    return Page();
                }
                else
                {
                    _logger.LogError($"API request failed with status code: {(int)response.StatusCode}");
                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching data: {ex.Message}");
                return StatusCode(500, "Error fetching data.");
            }
            catch (JsonException ex) // Catch JSON deserialization errors
            {
                _logger.LogError($"JSON Deserialization Error: {ex.Message}");
                return StatusCode(500, "Error deserializing data.");
            }
        }
    }

    public class CreativeData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ElementData> Elements { get; set; }
    }

    public class ElementData
    {
        public SlideData Slide { get; set; }
    }

    public class SlideData
    {
        public string Identifier { get; set; }
        public List<string> Attributes { get; set; }
        public string Status { get; set; }
        public List<object> Widgets { get; set; } // List of objects for flexibility
    }

    public class ImageWidget
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public List<string> Attributes { get; set; }
        public string Status { get; set; }
    }
    public class VideoWidget
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public List<string> Attributes { get; set; }
        public string Status { get; set; }
    }

    public class CountdownWidget
    {
        public List<string> Attributes { get; set; }
        public TargetDateTimeData TargetDateTime { get; set; }
        public List<TextWidget> Parts { get; set; }
        public OutcomeData Outcome { get; set; }
    }
    public class TextWidget { 
        public string Type { get; set; }
        public string Source { get; set; }
        public List<string> Attributes { get; set; }
    }
    public class TargetDateTimeData
    {
        public string Variable { get; set; }
        public string Name { get; set; }
    }

    public class OutcomeData
    {
        public string Action { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
    }
}