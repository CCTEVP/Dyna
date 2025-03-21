using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dyna.Player.Services
{
    public interface IDebugService
    {
        Task<string> DebugLine(object content);
    }
    
    public class DebugService : IDebugService
    {
        private readonly ILogger<DebugService> _logger;
        
        public DebugService(ILogger<DebugService> logger = null)
        {
            _logger = logger;
        }
        
        public async Task<string> DebugLine(object content)
        {
            await Task.CompletedTask;
            _logger?.LogDebug("{Content}", content);
            return "";
        }
    }
}