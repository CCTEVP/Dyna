using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Dyna.Player.Services
{
    public class FileService : IFileService // Assuming you have an IFileService interface
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOptions<FileServiceOptions> _options;

        public FileService(IWebHostEnvironment webHostEnvironment, IOptions<FileServiceOptions> options)
        {
            _webHostEnvironment = webHostEnvironment;
            _options = options;
        }

        public async Task<object> GetObjectDefinitionFromJsonAsync(Type componentType, string componentName)
        {
            string filePath = Path.Combine(_options.Value.ComponentsFolder, componentName, "Default.json");

            Debug.WriteLine($"[FileService] Attempting to load Default.json from: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"[FileService] Component definition file not found: {filePath}");
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
                Debug.WriteLine($"[FileService] Loaded Default.json for {componentType.Name}: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FileService] Error deserializing Default.json for {componentName}: {ex.Message}");
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