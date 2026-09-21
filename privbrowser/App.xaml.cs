using System.Windows;
using CefSharp;

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

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}
