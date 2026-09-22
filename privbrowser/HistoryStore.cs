using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PrivBrowser
{
    public class HistoryEntry
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public DateTime VisitedAtUtc { get; set; }
    }

    public static class HistoryStore
    {
        private const int MaxEntries = 500;
        private static List<HistoryEntry> _entries = new();
        private static bool _loaded = false;

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivBrowser", "history.json");

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    _entries = JsonSerializer.Deserialize<List<HistoryEntry>>(json) ?? new List<HistoryEntry>();
                }
            }
            catch { _entries = new List<HistoryEntry>(); }
        }

        private static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_entries));
            }
            catch { /* best effort */ }
        }

        public static void Record(string title, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("about:")) return;
            EnsureLoaded();
            _entries.Insert(0, new HistoryEntry { Title = title, Url = url, VisitedAtUtc = DateTime.UtcNow });
            if (_entries.Count > MaxEntries)
                _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
            Save();
        }

        public static List<HistoryEntry> GetAll()
        {
            EnsureLoaded();
            return _entries.ToList();
        }

        public static void Clear()
        {
            EnsureLoaded();
            _entries.Clear();
            Save();
        }
    }
}
