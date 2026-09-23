using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using PrivBrowser.Handlers;
using PrivBrowser.Helpers;
using PrivBrowser.Models;

namespace PrivBrowser
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<BrowserTab> Tabs { get; } = new();
        private readonly CustomDownloadHandler _downloadHandler = new();

        public MainWindow()
        {
            InitializeComponent();
            MainTabControl.ItemsSource = Tabs;
            AddNewTab("privbrowser://home");
        }

        private void AddNewTab(string url)
        {
            var browser = new ChromiumWebBrowser
            {
                RequestHandler = new AdBlockRequestHandler(),
                DownloadHandler = _downloadHandler
            };

            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("proxyBoundObject", new ProxyManager(), isAsync: true);
            browser.JavascriptObjectRepository.Register("fcodeBoundObject", new FCodeManager(), isAsync: true);

            var tab = new BrowserTab
            {
                Title = "New Tab",
                Address = url,
                Browser = browser
            };

            browser.AddressChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    tab.Address = e.Address;
                    if (MainTabControl.SelectedItem == tab)
                    {
                        TxtAddressBar.Text = e.Address;
                        TxtSecurityIcon.Visibility = e.Address.StartsWith("https://") ? Visibility.Visible : Visibility.Collapsed;
                    }
                });
            };

            browser.TitleChanged += (s, e) =>
            {
                Dispatcher.Invoke(() => tab.Title = e.Title);
            };

            browser.Load(url);
            Tabs.Add(tab);
            MainTabControl.SelectedItem = tab;
        }

        private BrowserTab? GetCurrentTab() => MainTabControl.SelectedItem as BrowserTab;

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetCurrentTab() is BrowserTab currentTab)
            {
                BrowserContainer.Children.Clear();
                BrowserContainer.Children.Add(currentTab.Browser);
                TxtAddressBar.Text = currentTab.Address;
            }
        }

        private void TxtAddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && GetCurrentTab() is BrowserTab tab)
            {
                string input = TxtAddressBar.Text.Trim();
                if (string.IsNullOrEmpty(input)) return;

                if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
                {
                    tab.Browser.Load(input);
                }
                else if (input.Contains('.') && !input.Contains(' '))
                {
                    tab.Browser.Load("https://" + input);
                }
                else
                {
                    tab.Browser.Load($"https://duckduckgo.com/?q={Uri.EscapeDataString(input)}");
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => GetCurrentTab()?.Browser.Back();
        private void BtnForward_Click(object sender, RoutedEventArgs e) => GetCurrentTab()?.Browser.Forward();
        private void BtnReload_Click(object sender, RoutedEventArgs e) => GetCurrentTab()?.Browser.Reload();
        private void BtnHome_Click(object sender, RoutedEventArgs e) => GetCurrentTab()?.Browser.Load("privbrowser://home");
        private void BtnSettings_Click(object sender, RoutedEventArgs e) => AddNewTab("privbrowser://settings");
        private void BtnNewTab_Click(object sender, RoutedEventArgs e) => AddNewTab("privbrowser://home");

        private void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is BrowserTab tab)
            {
                tab.Browser.Dispose();
                Tabs.Remove(tab);
                if (Tabs.Count == 0) AddNewTab("privbrowser://home");
            }
        }

        private async void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            var cookieManager = Cef.GetGlobalCookieManager();
            await cookieManager.DeleteCookiesAsync("", "");
            MessageBox.Show("Browsing cookies, cache, and session data cleared successfully.", "PrivBrowser", MessageBoxButton.OK, MessageBoxImage.Information);
            GetCurrentTab()?.Browser.Reload(ignoreCache: true);
        }
    }
}