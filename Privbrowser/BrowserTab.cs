using Microsoft.Web.WebView2.Wpf;

namespace PrivBrowser
{
    public class BrowserTab
    {
        public WebView2 View { get; }
        public System.Windows.Controls.Button TabButton { get; }
        public System.Windows.Controls.Button CloseButton { get; }
        public string Title { get; set; } = "New tab";
        public int BlockedCount { get; set; } = 0;

        /// <summary>
        /// Per-site override: when true, tracker blocking is paused for
        /// whatever domain is currently loaded in this tab.
        /// </summary>
        public bool ShieldDisabledForCurrentSite { get; set; } = false;
        public string? CurrentHost { get; set; }

        public BrowserTab(WebView2 view, System.Windows.Controls.Button tabButton, System.Windows.Controls.Button closeButton)
        {
            View = view;
            TabButton = tabButton;
            CloseButton = closeButton;
        }
    }
}
