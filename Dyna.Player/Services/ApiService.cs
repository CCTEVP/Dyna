using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dyna.Player.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetAsync<T>(string apiUrl)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ApiService] API Response from {apiUrl}: {json}"); // Log the response

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    try
                    {
                        return JsonSerializer.Deserialize<T>(json, options);
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"[ApiService] JSON Deserialization Error from {apiUrl}: {ex.Message}");
                        Debug.WriteLine($"[ApiService] JSON that failed to parse: {json}"); // Log the failing json
                        return default;
                    }
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API request failed with status code: {(int)response.StatusCode} from {apiUrl}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ApiService] Error Content: {errorContent}"); // Log the error content
                    return default;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP Request Error from {apiUrl}: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] General Error from {apiUrl}: {ex.Message}");
                return default;
            }
        }
    }
}