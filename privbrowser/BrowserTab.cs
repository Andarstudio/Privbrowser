using System;
using System.Collections.Generic;
using CefSharp.Wpf;

namespace PrivBrowser
{
    public class BrowserTab
    {
        public ChromiumWebBrowser View { get; }
        public System.Windows.Controls.Button TabButton { get; }
        public System.Windows.Controls.Button CloseButton { get; }
        public string Title { get; set; } = "New tab";
        public int BlockedCount { get; set; } = 0;

        /// <summary>Per-domain breakdown of what's been blocked on the current site.</summary>
        public Dictionary<string, int> BlockedDomainCounts { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Mirrors AllowlistStore for the currently-loaded host.</summary>
        public bool ShieldDisabledForCurrentSite { get; set; } = false;
        public string? CurrentHost { get; set; }

        public Action? OnBlockedCountChanged { get; set; }

        public BrowserTab(ChromiumWebBrowser view, System.Windows.Controls.Button tabButton, System.Windows.Controls.Button closeButton)
        {
            View = view;
            TabButton = tabButton;
            CloseButton = closeButton;
        }
    }
}
