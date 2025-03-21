using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Dyna.Player.Services
{
    public class FileService : IFileService // Assuming you have an IFileService interface
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOptions<FileServiceOptions> _options;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IWebHostEnvironment webHostEnvironment, 
            IOptions<FileServiceOptions> options,
            ILogger<FileService> logger = null)
        {
            _webHostEnvironment = webHostEnvironment;
            _options = options;
            _logger = logger;
        }

        public async Task<object> GetObjectDefinitionFromJsonAsync(Type componentType, string componentName)
        {
            string filePath = Path.Combine(_options.Value.ComponentsFolder, componentName, "Default.json");

            _logger?.LogDebug("[FileService] Attempting to load Default.json from: {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger?.LogWarning("[FileService] Component definition file not found: {FilePath}", filePath);
                return null;
            }

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var result = JsonConvert.DeserializeObject(json, componentType, new JsonSerializerSettings 
                { 
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });
                _logger?.LogDebug("[FileService] Loaded Default.json for {ComponentType}: {Result}", 
                    componentType.Name, JsonConvert.SerializeObject(result));
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError("[FileService] Error deserializing Default.json for {ComponentName}: {ErrorMessage}", 
                    componentName, ex.Message);
                return null;
            }
        }
    }

    public interface IFileService
    {
        Task<object> GetObjectDefinitionFromJsonAsync(Type componentType, string componentName);
    }

    public class FileServiceOptions
    {
        public string ComponentsFolder { get; set; }
    }
}