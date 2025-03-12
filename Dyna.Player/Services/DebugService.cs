using System.Diagnostics;

namespace Dyna.Player.Services
{
    public interface IDebugService
    {
        Task<string> DebugLine(object content);
    }
    public class DebugService
    {
        public async Task<string> DebugLine(object content)
        {
            Debug.WriteLine(content);
            return "";

        }
    }
}