using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Dyna.Api.Services;
using System.Security.Cryptography;
using System.Text;

namespace Dyna.Api.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Downloads a file from URL and saves it to the appropriate folder based on file type
        /// </summary>
        /// <param name="url">The URL to download the file from</param>
        /// <returns>The filename of the saved file</returns>
        Task<string> SaveFileFromUrlAsync(string url);

        /// <summary>
        /// Retrieves a file from the appropriate folder based on file type
        /// </summary>
        /// <param name="filekind">The type of file (image, video, or audio)</param>
        /// <param name="filename">The filename to retrieve</param>
        /// <returns>The file content and content type</returns>
        Task<(byte[] content, string contentType)> GetFileAsync(string filetype, string filename);
    }
    public class FileService : IFileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IMediaService _mediaService;
        private readonly string _sharedFilesPath;

        public FileService(
            HttpClient httpClient,
            ILogger<FileService> logger,
            IWebHostEnvironment env,
            IMediaService mediaService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = env;
            _mediaService = mediaService;
            
            // Get the absolute path to the solution directory
            var solutionPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, ".."));
            _sharedFilesPath = Path.Combine(solutionPath, "Dyna.Shared", "Files");
            
            _logger.LogInformation("Shared files path: {Path}", _sharedFilesPath);
        }

        public async Task<string> SaveFileFromUrlAsync(string url)
        {
            try
            {
                // Get the file extension from the URL
                var extension = Path.GetExtension(url);
                if (string.IsNullOrEmpty(extension))
                {
                    // Try to determine extension from content type
                    var response = await _httpClient.GetAsync(url);
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    extension = GetExtensionFromContentType(contentType);
                }

                // Determine file type from extension
                var fileKind = GetfileKindFromExtension(extension);
                
                // Create the file type directory if it doesn't exist
                var fileKindPath = Path.Combine(_sharedFilesPath, fileKind);
                Directory.CreateDirectory(fileKindPath);
                _logger.LogInformation("Created directory: {Path}", fileKindPath);

                // Generate a unique filename
                string filename;
                string filePath;
                do
                {
                    var hash = GenerateUniqueHash();
                    filename = $"{hash}{extension}";
                    filePath = Path.Combine(fileKindPath, filename);
                } while (File.Exists(filePath));

                _logger.LogInformation("Saving file to: {Path}", filePath);
                
                var content = await _mediaService.GetMediaContentAsync(url, GetMediaTypeFromExtension(extension));
                await File.WriteAllBytesAsync(filePath, content);
                _logger.LogInformation("File saved successfully");

                return filename;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving file from URL: {Url}", url);
                throw;
            }
        }

        private string GenerateUniqueHash()
        {
            using (var md5 = MD5.Create())
            {
                var input = $"{Guid.NewGuid()}{DateTime.UtcNow.Ticks}";
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash)
                    .Replace("/", "")
                    .Replace("+", "")
                    .Substring(0, 20);
            }
        }

        private string GetfileKindFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => "image",
                ".mp4" or ".webm" or ".ogg" => "video",
                ".mp3" or ".wav" => "audio",
                _ => "other"
            };
        }

        public async Task<(byte[] content, string contentType)> GetFileAsync(string filetype, string filename)
        {
            try
            {
                var filePath = Path.Combine(_sharedFilesPath, filetype, filename);
                
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filename}");
                }

                var content = await File.ReadAllBytesAsync(filePath);
                var extension = Path.GetExtension(filename);
                var contentType = _mediaService.GetContentType(GetMediaTypeFromExtension(extension));

                return (content, contentType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving file: {Filename}", filename);
                throw;
            }
        }

        private string GetExtensionFromContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return ".bin";

            return contentType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                "video/mp4" => ".mp4",
                "video/webm" => ".webm",
                "video/ogg" => ".ogg",
                "audio/mpeg" => ".mp3",
                "audio/ogg" => ".ogg",
                "audio/wav" => ".wav",
                _ => ".bin"
            };
        }

        private string GetMediaTypeFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                _ => "application/octet-stream"
            };
        }
    }
} 