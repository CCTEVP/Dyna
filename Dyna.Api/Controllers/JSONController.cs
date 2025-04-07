using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dyna.Api.Services;
using MongoDB.Bson;

namespace Dyna.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JSONController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _baseDataPath;
        private readonly ILogger<JSONController> _logger;
        private readonly string _jsonFilePath;
        private readonly IFileService _fileService;

        public JSONController(
            IWebHostEnvironment env,
            IFileService fileService,
            ILogger<JSONController> logger)
        {
            _env = env;
            _baseDataPath = Path.Combine(_env.ContentRootPath, "Data");
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet("GetDefault")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult GetDefault()
        {
            string fileName = "67e541477acf34a2c3747727.json";
            string _jsonFilePath = Path.Combine(_baseDataPath, fileName);
            try
            {
                if (!System.IO.File.Exists(_jsonFilePath))
                {
                    return NotFound("JSON file not found.");
                }
                FileInfo fileInfo = new FileInfo(_jsonFilePath);
                _logger.LogInformation($"[FileController.cs] Last Modified: {fileInfo.LastWriteTime}");

                using (FileStream fs = new FileStream(_jsonFilePath, FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                {
                    var jsonString = reader.ReadToEnd();
                    return Content(jsonString, "application/json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[FileController.cs] {ex}");  // Replace with your logging mechanism
                return StatusCode(500, "An error occurred while reading the JSON file."); // Return 500
            }
        }

        [HttpGet("GetById/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult GetById([FromRoute] string? id = null)
        {
            var jsonString = "{\"creative\":\"list\"}";

            if (!string.IsNullOrEmpty(id))
            {
                if (ObjectId.TryParse(id, out _))
                {
                    string fileName = $"{id}.json";
                    string _jsonFilePath = Path.Combine(_baseDataPath, fileName);
                    try
                    {
                        if (!System.IO.File.Exists(_jsonFilePath))
                        {
                            return NotFound("JSON file not found.");
                        }
                        FileInfo fileInfo = new FileInfo(_jsonFilePath);
                        _logger.LogInformation($"[FileController.cs] Last Modified: {fileInfo.LastWriteTime}");
                        using (FileStream fs = new FileStream(_jsonFilePath, FileMode.Open))
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            jsonString = reader.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[FileController.cs] {ex}");  // Replace with your logging mechanism
                        return StatusCode(500, "An error occurred while reading the JSON file."); // Return 500
                    }
                }
                else
                {
                    return NotFound("JSON file not found.");
                }
            }
            return Content(jsonString, "application/json");
        }
    }
}
