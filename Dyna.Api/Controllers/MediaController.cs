using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dyna.Api.Services;

namespace Dyna.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _baseDataPath;
        private readonly ILogger<MediaController> _logger;
        private readonly string _jsonFilePath;
        private readonly IFileService _fileService;

        public MediaController(
            IWebHostEnvironment env,
            IFileService fileService,
            ILogger<MediaController> logger)
        {
            _env = env;
            _baseDataPath = Path.Combine(_env.ContentRootPath, "Data");
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("Store")]
        public async Task<IActionResult> SaveFile([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest("URL is required");
                }

                var filename = await _fileService.SaveFileFromUrlAsync(url);
                return Ok(new { filename });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving file from URL: {Url}", url);
                return StatusCode(500, "Error saving file");
            }
        }

        [HttpGet("{filekind}/{filename}")]
        public async Task<IActionResult> GetFile([FromRoute] string filekind, [FromRoute] string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filekind) || string.IsNullOrEmpty(filename))
                {
                    return BadRequest("Filekind and filename are required");
                }

                var (content, contentType) = await _fileService.GetFileAsync(filekind, filename);
                return File(content, contentType);
            }
            catch (FileNotFoundException ex)
            {
                _logger?.LogWarning(ex, "File not found: {Filename} of type {filekind}", filename, filekind);
                return NotFound($"File not found: {filename}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving file {Filename} of type {filekind}", filename, filekind);
                return StatusCode(500, "Error retrieving file");
            }
        }
    }
}
