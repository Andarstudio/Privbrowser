using System;
using System.IO;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using PrivBrowser.Handlers;

namespace PrivBrowser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = new CefSettings
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache"),
                LogSeverity = LogSeverity.Disable
            };

            CefSharpSettings.WcfEnabled = true;

            settings.CefCommandLineArgs.Add("disable-telemetry", "1");
            settings.CefCommandLineArgs.Add("disable-component-update", "1");
            settings.CefCommandLineArgs.Add("no-pings", "1");
            settings.CefCommandLineArgs.Add("enable-do-not-track", "1");

            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "privbrowser",
                SchemeHandlerFactory = new PrivBrowserSchemeHandlerFactory()
            });

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
    }
}