using System.Diagnostics;
using SplashRocket.Models;

namespace SplashRocket.Services
{
    public class AppLauncherService
    {
        public void Run(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }
    }
}
