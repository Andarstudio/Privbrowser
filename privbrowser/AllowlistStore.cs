using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PrivBrowser
{
    public static class AllowlistStore
    {
        private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase);
        private static bool _loaded = false;

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivBrowser", "allowlist.json");

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var list = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                    foreach (var h in list) AllowedHosts.Add(h);
                }
            }
            catch { /* start empty on any read error */ }
        }

        private static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(AllowedHosts.ToList()));
            }
            catch { /* best effort */ }
        }

        public static bool IsAllowed(string? host)
        {
            if (string.IsNullOrEmpty(host)) return false;
            EnsureLoaded();
            return AllowedHosts.Contains(host);
        }

        public static void SetAllowed(string host, bool allowed)
        {
            if (string.IsNullOrEmpty(host)) return;
            EnsureLoaded();
            if (allowed) AllowedHosts.Add(host);
            else AllowedHosts.Remove(host);
            Save();
        }

        public static List<string> GetAll()
        {
            EnsureLoaded();
            return AllowedHosts.OrderBy(h => h).ToList();
        }
    }
}
