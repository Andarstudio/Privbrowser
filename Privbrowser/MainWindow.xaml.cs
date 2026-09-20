using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace PrivBrowser
{
    public partial class MainWindow : Window
    {
        private const string SearchEngineUrl = "https://duckduckgo.com/?q={0}";
        private const string HomeUrl = "https://duckduckgo.com";

        private readonly List<BrowserTab> _tabs = new();
        private BrowserTab? _activeTab;
        private CoreWebView2Environment? _sharedEnvironment;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await TrackerBlockList.InitializeAsync();

            var restored = SessionStore.Load();
            if (restored.Count == 0)
            {
                await OpenNewTabAsync(HomeUrl);
            }
            else
            {
                foreach (var url in restored)
                    await OpenNewTabAsync(url);
            }
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var urls = _tabs
                .Select(t => t.View.CoreWebView2?.Source)
                .Where(u => !string.IsNullOrEmpty(u))
                .Select(u => u!)
                .ToList();
            SessionStore.Save(urls);
        }

        private async System.Threading.Tasks.Task EnsureEnvironmentAsync()
        {
            if (_sharedEnvironment != null) return;

            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PrivBrowser", "UserData");

            var options = new CoreWebView2EnvironmentOptions(
                allowSingleSignOnUsingOSPrimaryAccount: false);

            _sharedEnvironment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder,
                options: options);
        }

        private async System.Threading.Tasks.Task OpenNewTabAsync(string url)
        {
            await EnsureEnvironmentAsync();

            var webView = new WebView2();
            await webView.EnsureCoreWebView2Async(_sharedEnvironment);
            ConfigurePrivacySettings(webView.CoreWebView2);

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

            var tab = new BrowserTab(webView, tabButton, closeButton);
            tabButton.Click += (_, _) => ActivateTab(tab);
            closeButton.Click += (_, _) => CloseTab(tab);

            webView.CoreWebView2.WebResourceRequested += (_, args) => OnWebResourceRequested(tab, args);

            webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                tab.Title = string.IsNullOrWhiteSpace(webView.CoreWebView2.DocumentTitle)
                    ? "New tab" : webView.CoreWebView2.DocumentTitle;
                tabButton.Content = Truncate(tab.Title, 16);
            };

            webView.CoreWebView2.SourceChanged += (_, _) =>
            {
                var source = webView.CoreWebView2.Source;
                if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
                {
                    if (tab.CurrentHost != uri.Host)
                    {
                        tab.CurrentHost = uri.Host;
                        tab.ShieldDisabledForCurrentSite = false;
                        tab.BlockedCount = 0;
                    }
                }
                if (tab == _activeTab)
                {
                    AddressBar.Text = source;
                    UpdateShieldButton(tab);
                }
            };

            var containerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 4, 0, 0) };
            containerPanel.Children.Add(tabButton);
            containerPanel.Children.Add(closeButton);

            _tabs.Add(tab);
            TabStrip.Items.Add(containerPanel);
            tab.TabButton.Tag = containerPanel;

            webView.CoreWebView2.Navigate(UpgradeToHttps(url));
            ActivateTab(tab);
        }

        private void ConfigurePrivacySettings(CoreWebView2 core)
        {
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDevToolsEnabled = true;
            core.Settings.IsGeneralAutofillEnabled = false;
            core.Settings.IsPasswordAutosaveEnabled = false;
            core.Settings.IsStatusBarEnabled = false;

            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

            core.NavigationStarting += (_, args) =>
            {
                if (args.Uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    args.Cancel = true;
                    core.Navigate(UpgradeToHttps(args.Uri));
                }
            };
        }

        private static string UpgradeToHttps(string url) =>
            url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                ? "https://" + url.Substring("http://".Length)
                : url;

        private void OnWebResourceRequested(BrowserTab tab, CoreWebView2WebResourceRequestedEventArgs args)
        {
            if (tab.ShieldDisabledForCurrentSite) return;

            var url = args.Request.Uri;
            if (TrackerBlockList.IsBlocked(url))
            {
                var env = _sharedEnvironment!;
                args.Response = env.CreateWebResourceResponse(null, 403, "Blocked by PrivBrowser", "");

                tab.BlockedCount++;
                if (tab == _activeTab)
                    Dispatcher.Invoke(() => UpdateShieldButton(tab));
            }
        }

        private void UpdateShieldButton(BrowserTab tab)
        {
            ShieldButton.Content = tab.ShieldDisabledForCurrentSite
                ? $"🛡 Off — {tab.BlockedCount} blocked"
                : $"🛡 {tab.BlockedCount} blocked";
        }

        private void ShieldButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null) return;
            _activeTab.ShieldDisabledForCurrentSite = !_activeTab.ShieldDisabledForCurrentSite;
            UpdateShieldButton(_activeTab);
            _activeTab.View.CoreWebView2?.Reload();
        }

        private void ActivateTab(BrowserTab tab)
        {
            _activeTab = tab;
            BrowserHost.Children.Clear();
            BrowserHost.Children.Add(tab.View);
            AddressBar.Text = tab.View.CoreWebView2?.Source ?? "";
            UpdateShieldButton(tab);
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

        private async void NewTabButton_Click(object sender, RoutedEventArgs e) =>
            await OpenNewTabAsync(HomeUrl);

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.View.CoreWebView2?.CanGoBack == true)
                _activeTab.View.CoreWebView2.GoBack();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.View.CoreWebView2?.CanGoForward == true)
                _activeTab.View.CoreWebView2.GoForward();
        }

        private void Reload_Click(object sender, RoutedEventArgs e) =>
            _activeTab?.View.CoreWebView2?.Reload();

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || _activeTab?.View.CoreWebView2 == null) return;

            var input = AddressBar.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            string target = LooksLikeUrl(input)
                ? UpgradeToHttps(NormalizeUrl(input))
                : string.Format(SearchEngineUrl, Uri.EscapeDataString(input));

            _activeTab.View.CoreWebView2.Navigate(target);
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
