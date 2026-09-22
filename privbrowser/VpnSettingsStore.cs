using System;
using System.IO;
using System.Text.Json;

namespace PrivBrowser
{
    public class VpnConfig
    {
        public bool Enabled { get; set; } = false;
        public string Protocol { get; set; } = "socks5"; // "socks5" or "http"
        public string Host { get; set; } = "";
        public int Port { get; set; } = 1080;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public static class VpnSettingsStore
    {
        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivBrowser", "vpn.json");

        public static VpnConfig Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<VpnConfig>(json) ?? new VpnConfig();
                }
            }
            catch { /* fall through to default */ }
            return new VpnConfig();
        }

        public static void Save(VpnConfig config)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(config));
            }
            catch { /* best effort */ }
        }

        /// <summary>
        /// Builds the value CefSharp expects for the --proxy-server command
        /// line switch, e.g. "socks5://1.2.3.4:1080" or "http://1.2.3.4:8080".
        /// Returns null if the config is disabled or incomplete.
        /// </summary>
        public static string? BuildProxyServerArg(VpnConfig config)
        {
            if (!config.Enabled) return null;
            if (string.IsNullOrWhiteSpace(config.Host) || config.Port <= 0) return null;
            var scheme = config.Protocol == "http" ? "http" : "socks5";
            return $"{scheme}://{config.Host}:{config.Port}";
        }
    }
}
