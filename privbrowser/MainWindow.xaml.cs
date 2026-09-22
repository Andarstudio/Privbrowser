using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;

namespace PrivBrowser
{
    public partial class MainWindow : Window
    {
        private const string SearchEngineUrl = "https://duckduckgo.com/?q={0}";
        private const string HomeUrl = "https://duckduckgo.com";

        private readonly List<BrowserTab> _tabs = new();
        private BrowserTab? _activeTab;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await TrackerBlockList.InitializeAsync();
            UpdateVpnButton();

            var restored = SessionStore.Load();
            if (restored.Count == 0)
                OpenNewTab(HomeUrl);
            else
                foreach (var url in restored)
                    OpenNewTab(url);
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var urls = _tabs
                .Select(t => t.View.Address)
                .Where(u => !string.IsNullOrEmpty(u))
                .Select(u => u!)
                .ToList();
            SessionStore.Save(urls);
        }

        private void OpenNewTab(string url)
        {
            var browser = new ChromiumWebBrowser();

            var tabButton = new Button
            {
                Content = "New tab",
                Margin = new Thickness(2, 2, 0, 2),
                Padding = new Thickness(10, 6, 6, 6)
            };
            var closeButton = new Button
            {
                Content = "✕",
                Margin = new Thickness(0, 2, 2, 2),
                Padding = new Thickness(6, 6, 8, 6)
            };

            var tab = new BrowserTab(browser, tabButton, closeButton);
            tab.OnBlockedCountChanged = () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (tab == _activeTab) UpdateShieldButton(tab);
                });
            };

            browser.RequestHandler = new TrackerRequestHandler(tab);

            tabButton.Click += (_, _) => ActivateTab(tab);
            closeButton.Click += (_, _) => CloseTab(tab);

            browser.TitleChanged += (_, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    tab.Title = string.IsNullOrWhiteSpace(args.NewValue as string)
                        ? "New tab" : (string)args.NewValue;
                    tabButton.Content = Truncate(tab.Title, 16);
                });
            };

            browser.AddressChanged += (_, args) =>
            {
                var address = args.NewValue as string ?? "";
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    if (tab.CurrentHost != uri.Host)
                    {
                        tab.CurrentHost = uri.Host;
                        tab.ShieldDisabledForCurrentSite = AllowlistStore.IsAllowed(uri.Host);
                        tab.BlockedCount = 0;
                        tab.BlockedDomainCounts.Clear();
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    if (tab == _activeTab)
                    {
                        AddressBar.Text = address;
                        UpdateShieldButton(tab);
                        UpdateBookmarkStar(address);
                    }
                });
            };

            browser.FrameLoadEnd += (_, args) =>
            {
                if (!args.Frame.IsMain) return;
                var url = args.Url;
                Dispatcher.Invoke(() => HistoryStore.Record(tab.Title, url));
            };

            var containerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 4, 0, 0) };
            containerPanel.Children.Add(tabButton);
            containerPanel.Children.Add(closeButton);
            tabButton.Tag = containerPanel;

            _tabs.Add(tab);
            TabStrip.Items.Add(containerPanel);

            browser.Address = UpgradeToHttps(url);
            ActivateTab(tab);
        }

        private static string UpgradeToHttps(string url) =>
            url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                ? "https://" + url.Substring("http://".Length)
                : url;

        private void UpdateShieldButton(BrowserTab tab)
        {
            ShieldButton.Content = tab.ShieldDisabledForCurrentSite
                ? $"🛡 Off — {tab.BlockedCount} blocked"
                : $"🛡 {tab.BlockedCount} blocked";
        }

        private void ShieldButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null) return;
            var changed = ShieldsPanel.Show(this, _activeTab);
            UpdateShieldButton(_activeTab);
            if (changed) _activeTab.View.Reload();
        }

        private void BookmarkStarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.View.Address == null) return;
            var url = _activeTab.View.Address;
            if (BookmarkStore.IsBookmarked(url))
                BookmarkStore.Remove(url);
            else
                BookmarkStore.Add(_activeTab.Title, url);
            UpdateBookmarkStar(url);
        }

        private void UpdateBookmarkStar(string? address)
        {
            BookmarkStarButton.Content = !string.IsNullOrEmpty(address) && BookmarkStore.IsBookmarked(address)
                ? "★" : "☆";
        }

        private void BookmarksButton_Click(object sender, RoutedEventArgs e)
        {
            BookmarksWindowUI.Show(this, url => _activeTab?.View.LoadUrl(url));
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindowUI.Show(this, url => _activeTab?.View.LoadUrl(url));
        }

        private void VpnButton_Click(object sender, RoutedEventArgs e)
        {
            var changed = VpnPanel.Show(this);
            UpdateVpnButton();
            if (changed)
            {
                MessageBox.Show(this,
                    "Restart PrivBrowser for the VPN/proxy change to take effect.",
                    "Restart required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateVpnButton()
        {
            var config = VpnSettingsStore.Load();
            VpnButton.Content = config.Enabled ? $"🌐 VPN: On ({config.Host})" : "🌐 VPN: Off";
        }

        private void ActivateTab(BrowserTab tab)
        {
            _activeTab = tab;
            BrowserHost.Children.Clear();
            BrowserHost.Children.Add(tab.View);
            AddressBar.Text = tab.View.Address ?? "";
            UpdateShieldButton(tab);
            UpdateBookmarkStar(tab.View.Address);
        }

        private void CloseTab(BrowserTab tab)
        {
            if (_tabs.Count == 1)
            {
                Close();
                return;
            }

            var index = _tabs.IndexOf(tab);
            _tabs.Remove(tab);

            if (tab.TabButton.Tag is StackPanel wrapper)
                TabStrip.Items.Remove(wrapper);

            tab.View.Dispose();

            if (tab == _activeTab)
            {
                var nextIndex = Math.Max(0, index - 1);
                ActivateTab(_tabs[nextIndex]);
            }
        }

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max) + "…";

        private void NewTabButton_Click(object sender, RoutedEventArgs e) => OpenNewTab(HomeUrl);

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.View.CanGoBack == true) _activeTab.View.Back();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.View.CanGoForward == true) _activeTab.View.Forward();
        }

        private void Reload_Click(object sender, RoutedEventArgs e) => _activeTab?.View.Reload();

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || _activeTab == null) return;

            var input = AddressBar.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            string target = LooksLikeUrl(input)
                ? UpgradeToHttps(NormalizeUrl(input))
                : string.Format(SearchEngineUrl, Uri.EscapeDataString(input));

            _activeTab.View.Address = target;
        }

        private static bool LooksLikeUrl(string input)
        {
            if (input.Contains(' ')) return false;
            if (input.StartsWith("http://") || input.StartsWith("https://")) return true;
            return input.Contains('.') && !input.Contains(' ');
        }

        private static string NormalizeUrl(string input) =>
            input.StartsWith("http://") || input.StartsWith("https://")
                ? input
                : "https://" + input;
    }
}
