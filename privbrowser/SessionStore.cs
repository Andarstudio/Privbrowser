using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PrivBrowser
{
    public static class SessionStore
    {
        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivBrowser", "session.json");

        public static void Save(IEnumerable<string> tabUrls)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(tabUrls.ToList());
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Session save is best-effort; never crash the browser over it.
            }
        }

        public static List<string> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<string>();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
