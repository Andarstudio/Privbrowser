using System.Collections.Generic;
using CefSharp;

namespace PrivBrowser.Helpers
{
    public class ProxyManager
    {
        public static string CurrentHost { get; private set; } = string.Empty;
        public static string CurrentPort { get; private set; } = string.Empty;
        public static string CurrentProtocol { get; private set; } = "http";
        public static bool IsEnabled { get; private set; } = false;

        public void SetProxy(string host, string port, string protocol)
        {
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(port)) return;

            CurrentHost = host.Trim();
            CurrentPort = port.Trim();
            CurrentProtocol = string.IsNullOrWhiteSpace(protocol) ? "http" : protocol.Trim();
            IsEnabled = true;

            ApplyProxy();
        }

        public void ClearProxy()
        {
            CurrentHost = string.Empty;
            CurrentPort = string.Empty;
            IsEnabled = false;

            ApplyProxy();
        }

        private static void ApplyProxy()
        {
            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var requestContext = Cef.GetGlobalRequestContext();
                var proxyDict = new Dictionary<string, object>();

                if (IsEnabled && !string.IsNullOrEmpty(CurrentHost))
                {
                    proxyDict["mode"] = "fixed_servers";
                    proxyDict["server"] = $"{CurrentProtocol}://{CurrentHost}:{CurrentPort}";
                }
                else
                {
                    proxyDict["mode"] = "direct";
                }

                requestContext.SetPreference("proxy", proxyDict, out _);
            });
        }
    }
}