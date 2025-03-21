using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Dyna.Player.Controllers
{
    [Route("admin/logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly Regex _timestampRegex = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+[+-]\d{2}:\d{2}");
        private readonly Regex _hashRegex = new Regex(@"([A-Z0-9]+:[0-9]+)\s+(\[[A-Z]+\])\s+(.+)$");

        public LogsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            
            if (!Directory.Exists(logsDirectory))
            {
                return NotFound(new { error = "Logs directory does not exist." });
            }

            var logFiles = Directory.GetFiles(logsDirectory, "*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new LogFileViewModel
                {
                    Name = f.Name,
                    LastModified = f.LastWriteTime,
                    SizeKb = f.Length / 1024
                })
                .ToList();

            // Apply pagination
            var totalCount = logFiles.Count;
            var paginatedLogFiles = logFiles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Files = paginatedLogFiles
            };

            return Ok(result);
        }

        [HttpGet("current")]
        [Produces("application/json")]
        public async Task<IActionResult> GetCurrentDayLog([FromQuery] string filter = null, [FromQuery] int lines = 1000)
        {
            // Get today's date in YYYYMMDD format
            string today = DateTime.Now.ToString("yyyyMMdd");
            
            // Look for a log file with today's date
            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                return NotFound(new { error = "Logs directory does not exist." });
            }

            // Look for files matching pattern dyna-player-{today}.log
            var logFiles = Directory.GetFiles(logsDirectory, $"dyna-player-*{today}*.log");
            
            if (logFiles.Length == 0)
            {
                // If not found, try to find the most recent log file
                var mostRecentLog = Directory.GetFiles(logsDirectory, "*.log")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();

                if (mostRecentLog == null)
                {
                    return NotFound(new { error = "No log files found." });
                }

                return await ViewLog(mostRecentLog.Name, filter, lines);
            }

            // Use the first matching file
            var fileName = Path.GetFileName(logFiles[0]);
            return await ViewLog(fileName, filter, lines);
        }

        [HttpGet("{fileName}")]
        [Produces("application/json")]
        public async Task<IActionResult> ViewLog(string fileName, [FromQuery] string filter = null, [FromQuery] int lines = 1000)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
            {
                return BadRequest(new { error = "Invalid file name" });
            }

            var logFilePath = Path.Combine(_environment.ContentRootPath, "Logs", fileName);
            
            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound(new { error = $"Log file '{fileName}' not found" });
            }

            // For large files, read only the specified number of lines
            var content = await ReadLastLinesAsync(logFilePath, lines);
            
            // Parse log entries
            var logEntries = ParseLogEntries(content);
            
            // Apply text filter if provided
            if (!string.IsNullOrEmpty(filter))
            {
                logEntries = logEntries.Where(entry => 
                    entry.Message.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            var result = new LogContentViewModel
            {
                FileName = fileName,
                Entries = logEntries,
                TotalEntries = logEntries.Count,
                Filter = filter,
                MaxLines = lines
            };

            return Ok(result);
        }

        private List<LogEntry> ParseLogEntries(string content)
        {
            var entries = new List<LogEntry>();
            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var currentEntry = new LogEntry();
            var currentMessage = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var timestampMatch = _timestampRegex.Match(line);
                if (timestampMatch.Success)
                {
                    // If we have a previous entry, add it to the list
                    if (currentEntry.Timestamp != default)
                    {
                        currentEntry.Message = currentMessage.ToString().Trim();
                        entries.Add(currentEntry);
                        currentMessage.Clear();
                    }

                    // Start new entry
                    currentEntry = new LogEntry
                    {
                        Timestamp = DateTimeOffset.Parse(timestampMatch.Value).DateTime
                    };

                    // Parse the rest of the line for hash, type, and message
                    var remainingText = line.Substring(timestampMatch.Length).TrimStart();
                    var hashMatch = _hashRegex.Match(remainingText);
                    
                    if (hashMatch.Success)
                    {
                        currentEntry.Hash = hashMatch.Groups[1].Value;
                        currentEntry.Type = hashMatch.Groups[2].Value;
                        currentMessage.AppendLine(hashMatch.Groups[3].Value);
                    }
                    else
                    {
                        // Fallback if the pattern doesn't match
                        currentMessage.AppendLine(remainingText);
                    }
                }
                else
                {
                    // Append to current message
                    currentMessage.AppendLine(line);
                }
            }

            // Add the last entry
            if (currentEntry.Timestamp != default)
            {
                currentEntry.Message = currentMessage.ToString().Trim();
                entries.Add(currentEntry);
            }

            // Return only the last 100 entries
            return entries.TakeLast(100).ToList();
        }

        private async Task<string> ReadLastLinesAsync(string filePath, int lineCount)
        {
            const int BUFFER_SIZE = 1024 * 1024; // Increased buffer size to 1MB
            var content = new StringBuilder();
            var foundLines = 0;
            
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length == 0)
                {
                    return string.Empty;
                }

                // Start from the end of the file
                long position = fs.Length;
                byte[] buffer = new byte[BUFFER_SIZE];
                
                while (position > 0 && foundLines < lineCount * 2) // Read more lines than needed to ensure complete entries
                {
                    long bytesToRead = Math.Min(BUFFER_SIZE, position);
                    position -= bytesToRead;
                    
                    fs.Position = position;
                    int bytesRead = await fs.ReadAsync(buffer, 0, (int)bytesToRead);
                    
                    string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    content.Insert(0, chunk);
                    
                    // Count how many timestamp patterns we've found
                    foundLines += _timestampRegex.Matches(chunk).Count;
                }
            }

            return content.ToString();
        }
    }

    public class LogFileViewModel
    {
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public long SizeKb { get; set; }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Hash { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class LogContentViewModel
    {
        public string FileName { get; set; }
        public List<LogEntry> Entries { get; set; }
        public int TotalEntries { get; set; }
        public string Filter { get; set; }
        public int MaxLines { get; set; }
    }
} 