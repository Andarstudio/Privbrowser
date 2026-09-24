using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using PrivBrowser.Handlers;

namespace PrivBrowser
{
    public class BrowserTabItem
    {
        public string HeaderText { get; set; } = "New Tab";
        public ChromiumWebBrowser Browser { get; set; } = null!;
    }

    public partial class MainWindow : Window
    {
        private readonly List<BrowserTabItem> _tabs = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateNewTab("https://www.google.com");
        }

        private void CreateNewTab(string url)
        {
            var browser = new ChromiumWebBrowser
            {
                RequestHandler = new AdBlockRequestHandler()
            };

            var tabItem = new BrowserTabItem
            {
                HeaderText = "Loading...",
                Browser = browser
            };

            browser.TitleChanged += (s, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    tabItem.HeaderText = args.NewValue?.ToString() ?? "New Tab";
                    MainTabControl.Items.Refresh();
                });
            };

            browser.AddressChanged += (s, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (MainTabControl.SelectedItem == tabItem)
                    {
                        UrlTextBox.Text = args.NewValue?.ToString() ?? "";
                    }
                });
            };

            _tabs.Add(tabItem);
            MainTabControl.Items.Add(tabItem);
            MainTabControl.SelectedItem = tabItem;

            browser.LoadUrl(url);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem is BrowserTabItem item)
            {
                BrowserContainer.Children.Clear();
                BrowserContainer.Children.Add(item.Browser);
                UrlTextBox.Text = item.Browser.Address;
            }
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTab("https://www.google.com");
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is BrowserTabItem item)
            {
                item.Browser.Dispose();
                _tabs.Remove(item);
                MainTabControl.Items.Remove(item);

                if (_tabs.Count == 0)
                {
                    Close();
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is BrowserTabItem item && item.Browser.CanGoBack)
                item.Browser.Back();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is BrowserTabItem item && item.Browser.CanGoForward)
                item.Browser.Forward();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is BrowserTabItem item)
                item.Browser.Reload();
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && MainTabControl.SelectedItem is BrowserTabItem item)
            {
                string input = UrlTextBox.Text.Trim();
                if (!input.StartsWith("http://") && !input.StartsWith("https://") && !input.StartsWith("about:"))
                {
                    if (input.Contains(".") && !input.Contains(" "))
                    {
                        input = "https://" + input;
                    }
                    else
                    {
                        input = $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
                    }
                }
                item.Browser.LoadUrl(input);
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings panel clicked");
        }

        private void ToggleTerminal_Click(object sender, RoutedEventArgs e)
        {
            TerminalContainer.Visibility = TerminalContainer.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}