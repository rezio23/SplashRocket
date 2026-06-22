using System.Collections.Generic;

namespace SplashRocket.Models
{
    public class Workspace
    {
        public string Name { get; set; } = string.Empty;
        public List<AppItem> Apps { get; set; } = new();
    }
}
