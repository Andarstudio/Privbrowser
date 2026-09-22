using System.Windows;
using CefSharp;
using CefSharp.Wpf; // <-- Add this namespace directive

namespace PrivBrowser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = new CefSettings
            {
                CachePath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "PrivBrowser", "CefCache"),
                PersistSessionCookies = true
            };

            settings.CefCommandLineArgs.Add("disable-background-networking", "1");

            // Apply the user's configured VPN/proxy, if enabled.
            var vpnConfig = VpnSettingsStore.Load();
            var proxyArg = VpnSettingsStore.BuildProxyServerArg(vpnConfig);
            if (proxyArg != null)
            {
                settings.CefCommandLineArgs.Add("proxy-server", proxyArg);
            }

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}
