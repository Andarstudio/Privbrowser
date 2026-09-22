using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PrivBrowser
{
    public class BookmarkEntry
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public static class BookmarkStore
    {
        private static List<BookmarkEntry> _bookmarks = new();
        private static bool _loaded = false;

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivBrowser", "bookmarks.json");

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    _bookmarks = JsonSerializer.Deserialize<List<BookmarkEntry>>(json) ?? new List<BookmarkEntry>();
                }
            }
            catch { _bookmarks = new List<BookmarkEntry>(); }
        }

        private static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_bookmarks));
            }
            catch { /* best effort */ }
        }

        public static List<BookmarkEntry> GetAll()
        {
            EnsureLoaded();
            return _bookmarks.ToList();
        }

        public static bool IsBookmarked(string url)
        {
            EnsureLoaded();
            return _bookmarks.Any(b => b.Url == url);
        }

        public static void Add(string title, string url)
        {
            EnsureLoaded();
            if (_bookmarks.Any(b => b.Url == url)) return;
            _bookmarks.Add(new BookmarkEntry { Title = string.IsNullOrWhiteSpace(title) ? url : title, Url = url });
            Save();
        }

        public static void Remove(string url)
        {
            EnsureLoaded();
            _bookmarks.RemoveAll(b => b.Url == url);
            Save();
        }
    }
}
