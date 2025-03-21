using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dyna.Player.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger = null)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T> GetAsync<T>(string apiUrl)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _logger?.LogDebug("[ApiService] API Response from {Url}: {Json}", apiUrl, json); // Log the response

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    try
                    {
                        return JsonSerializer.Deserialize<T>(json, options);
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogError("[ApiService] JSON Deserialization Error from {Url}: {ErrorMessage}", apiUrl, ex.Message);
                        _logger?.LogError("[ApiService] JSON that failed to parse: {Json}", json); // Log the failing json
                        return default;
                    }
                }
                else
                {
                    _logger?.LogWarning("[ApiService] API request failed with status code: {StatusCode} from {Url}", 
                        (int)response.StatusCode, apiUrl);
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogWarning("[ApiService] Error Content: {ErrorContent}", errorContent); // Log the error content
                    return default;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError("[ApiService] HTTP Request Error from {Url}: {ErrorMessage}", apiUrl, ex.Message);
                return default;
            }
            catch (Exception ex)
            {
                _logger?.LogError("[ApiService] General Error from {Url}: {ErrorMessage}", apiUrl, ex.Message);
                return default;
            }
        }
    }
}