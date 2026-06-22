using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SplashRocket.Models;

namespace SplashRocket.Services
{
    public class WorkspaceService
    {
        private readonly string _filePath;

        public WorkspaceService(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(Workspace workspace)
        {
            var json = JsonSerializer.Serialize(
                workspace,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(_filePath, json);
        }

        public Workspace Load()
        {
            if (!File.Exists(_filePath))
                return new Workspace();

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new Workspace();

            try
            {
                return JsonSerializer.Deserialize<Workspace>(json) ?? new Workspace();
            }
            catch
            {
                try
                {
                    var legacy = JsonSerializer.Deserialize<List<Workspace>>(json);
                    return legacy?.Count > 0 ? legacy[0] : new Workspace();
                }
                catch
                {
                    return new Workspace();
                }
            }
        }
    }
}
