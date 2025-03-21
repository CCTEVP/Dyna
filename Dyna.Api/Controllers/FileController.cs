using Microsoft.AspNetCore.Mvc;
using Dyna.Api.Models;
using Microsoft.Extensions.Logging;

namespace Dyna.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _baseDataPath;
        private readonly ILogger<FileController> _logger;

        private readonly string _jsonFilePath;

        public FileController(IWebHostEnvironment env)
        {
            _env = env;
            _baseDataPath = Path.Combine(_env.ContentRootPath, "Data");

        }

        [HttpGet("GetDefault")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult GetDefault()
        {
            string fileName = "structure_123456789.json";
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
        public IActionResult GetById([FromRoute] int? id)
        {
            string fileName = "structure_123456789.json";
            if (id.HasValue)
            {
                fileName = $"structure_{id}.json";
            }
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

    }
}
