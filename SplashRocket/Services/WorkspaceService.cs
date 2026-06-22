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

        public void Save(List<Workspace> workspaces)
        {
            var json = JsonSerializer.Serialize(
                workspaces,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(_filePath, json);
        }

        public List<Workspace> Load()
        {
            if (!File.Exists(_filePath))
                return new List<Workspace>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Workspace>>(json) ?? new List<Workspace>();
        }
    }
}