using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrivBrowser
{
    /// <summary>
    /// Loads real tracker/ad domain lists (EasyList + EasyPrivacy) instead of
    /// a small hardcoded set. On first run it downloads and caches them
    /// locally; after that it loads instantly from disk and refreshes
    /// in the background every 7 days.
    /// </summary>
    public static class TrackerBlockList
    {
        private static readonly HashSet<string> BlockedDomains = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object Lock = new();

        private static readonly (string Url, string CacheFile)[] Sources = new[]
        {
            ("https://easylist.to/easylist/easylist.txt", "easylist.txt"),
            ("https://easylist.to/easylist/easyprivacy.txt", "easyprivacy.txt")
        };

        private static string CacheFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivBrowser", "FilterLists");

        /// <summary>
        /// Call once at startup. Loads from local cache immediately if present
        /// (so the browser is never blocked waiting on network), then
        /// refreshes from the network in the background if the cache is
        /// missing or older than 7 days.
        /// </summary>
        public static async Task InitializeAsync()
        {
            Directory.CreateDirectory(CacheFolder);

            bool anyLoadedFromDisk = false;
            foreach (var (_, cacheFile) in Sources)
            {
                var path = Path.Combine(CacheFolder, cacheFile);
                if (File.Exists(path))
                {
                    ParseIntoBlockList(await File.ReadAllLinesAsync(path));
                    anyLoadedFromDisk = true;
                }
            }

            // Seed a small built-in fallback list so blocking works even
            // before the first successful download ever completes.
            if (!anyLoadedFromDisk)
            {
                lock (Lock)
                {
                    foreach (var d in FallbackDomains) BlockedDomains.Add(d);
                }
            }

            _ = RefreshIfStaleAsync(); // fire-and-forget background refresh
        }

        private static async Task RefreshIfStaleAsync()
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(20);

            foreach (var (url, cacheFile) in Sources)
            {
                try
                {
                    var path = Path.Combine(CacheFolder, cacheFile);
                    var isStale = !File.Exists(path) ||
                        (DateTime.UtcNow - File.GetLastWriteTimeUtc(path)) > TimeSpan.FromDays(7);
                    if (!isStale) continue;

                    var text = await http.GetStringAsync(url);
                    await File.WriteAllTextAsync(path, text);
                    ParseIntoBlockList(text.Split('\n'));
                }
                catch
                {
                    // Offline or blocked — silently keep using whatever is
                    // already loaded (cached or fallback). Not fatal.
                }
            }
        }

        /// <summary>
        /// EasyList format is mostly one rule per line. We only extract the
        /// simple "||domain.com^" domain-blocking rules, which cover the
        /// large majority of ad/tracker entries and are cheap to match.
        /// Cosmetic rules (##selector) and complex regex rules are skipped.
        /// </summary>
        private static void ParseIntoBlockList(IEnumerable<string> lines)
        {
            lock (Lock)
            {
                foreach (var raw in lines)
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("!") || line.StartsWith("[")) continue;
                    if (!line.StartsWith("||")) continue;

                    var end = line.IndexOfAny(new[] { '^', '/', '$' });
                    var domain = end > 2 ? line.Substring(2, end - 2) : line.Substring(2);
                    domain = domain.Trim();
                    if (domain.Length > 0 && domain.Contains('.'))
                        BlockedDomains.Add(domain);
                }
            }
        }

        public static bool IsBlocked(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

            var host = uri.Host;
            lock (Lock)
            {
                return BlockedDomains.Contains(host) ||
                    BlockedDomains.Any(d => host.EndsWith("." + d, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static int LoadedCount
        {
            get { lock (Lock) return BlockedDomains.Count; }
        }

        // Used only until the first real list finishes loading/downloading.
        private static readonly string[] FallbackDomains =
        {
            "doubleclick.net", "googlesyndication.com", "googleadservices.com",
            "google-analytics.com", "googletagmanager.com", "facebook.net",
            "connect.facebook.net", "scorecardresearch.com", "adnxs.com",
            "outbrain.com", "taboola.com", "criteo.com", "hotjar.com",
            "mixpanel.com", "segment.io", "amplitude.com", "quantserve.com",
            "adsrvr.org", "moatads.com"
        };
    }
}
