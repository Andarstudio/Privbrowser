using CommunityToolkit.Mvvm.ComponentModel;
using CefSharp.Wpf;

namespace PrivBrowser.Models
{
    public partial class BrowserTab : ObservableObject
    {
        [ObservableProperty]
        private string _title = "New Tab";

        [ObservableProperty]
        private string _address = "privbrowser://home";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSecure;

        public ChromiumWebBrowser Browser { get; set; } = null!;
    }
}