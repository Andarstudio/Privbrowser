using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace PrivBrowser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize CefSharp for .NET 8
            var settings = new CefSettings();
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
    }
}
