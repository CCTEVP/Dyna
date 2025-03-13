using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyna.Player.Controllers
{
    [Route("admin/[controller]")]
    public class LogsController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public LogsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            
            if (!Directory.Exists(logsDirectory))
            {
                return View("Error", new ErrorViewModel { Message = "Logs directory does not exist." });
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

            return View(logFiles);
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> ViewLog(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
            {
                return BadRequest("Invalid file name");
            }

            var logFilePath = Path.Combine(_environment.ContentRootPath, "Logs", fileName);
            
            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound();
            }

            // For large files, read only the last 1000 lines
            var content = await ReadLastLinesAsync(logFilePath, 1000);
            
            return View(new LogContentViewModel
            {
                FileName = fileName,
                Content = content
            });
        }

        private async Task<string> ReadLastLinesAsync(string filePath, int lineCount)
        {
            const int BUFFER_SIZE = 4096;
            var lines = new List<string>(lineCount);
            
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length == 0)
                {
                    return "File is empty";
                }

                // Start from the end of the file
                long position = fs.Length;
                int linesRead = 0;
                byte[] buffer = new byte[BUFFER_SIZE];
                
                // Read chunks from the end of the file
                while (position > 0 && linesRead < lineCount)
                {
                    long bytesToRead = Math.Min(BUFFER_SIZE, position);
                    position -= bytesToRead;
                    
                    fs.Position = position;
                    int bytesRead = await fs.ReadAsync(buffer, 0, (int)bytesToRead);
                    
                    // Process the buffer
                    string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] chunkLines = chunk.Split('\n');
                    
                    // Add lines to our collection, but don't exceed the requested line count
                    for (int i = chunkLines.Length - 1; i >= 0 && linesRead < lineCount; i--)
                    {
                        lines.Insert(0, chunkLines[i]);
                        linesRead++;
                    }
                }
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    public class LogFileViewModel
    {
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public long SizeKb { get; set; }
    }

    public class LogContentViewModel
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }

    public class ErrorViewModel
    {
        public string Message { get; set; }
    }
} 