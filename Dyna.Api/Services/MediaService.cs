using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Dyna.Api.Services
{
    public interface IMediaService
    {
        Task<byte[]> GetMediaContentAsync(string source, string mediaType);
        string GetContentType(string mediaType);
        bool IsValidSource(string source);
        bool IsBase64Source(string source);
    }
    public class MediaService : IMediaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MediaService> _logger;

        public MediaService(HttpClient httpClient, ILogger<MediaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<byte[]> GetMediaContentAsync(string source, string mediaType)
        {
            try
            {
                var response = await _httpClient.GetAsync(source);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving media content from source: {Source}", source);
                throw;
            }
        }

        public string GetContentType(string mediaType)
        {
            return mediaType?.ToLower() switch
            {
                "image" or "image/jpeg" => "image/jpeg",
                "image/png" => "image/png",
                "image/gif" => "image/gif",
                "image/webp" => "image/webp",
                "image/svg+xml" => "image/svg+xml",
                "video" or "video/mp4" => "video/mp4",
                "video/webm" => "video/webm",
                "video/ogg" => "video/ogg",
                "audio" or "audio/mpeg" => "audio/mpeg",
                "audio/ogg" => "audio/ogg",
                "audio/wav" => "audio/wav",
                _ => "application/octet-stream"
            };
        }

        public bool IsValidSource(string source)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            if (IsBase64Source(source))
                return true;

            return Uri.TryCreate(source, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public bool IsBase64Source(string source)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            if (source.StartsWith("data:"))
                return true;

            try
            {
                return Regex.IsMatch(source, @"^[A-Za-z0-9+/]*={0,2}$");
            }
            catch
            {
                return false;
            }
        }
    }
} 