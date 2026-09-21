using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PrivBrowser
{
    public static class ShieldsPanel
    {
        /// <summary>
        /// Shows the shields breakdown for the given tab. Returns true if the
        /// allow/block state changed (caller should reload the page).
        /// </summary>
        public static bool Show(Window owner, BrowserTab tab)
        {
            bool changed = false;
            var host = tab.CurrentHost ?? "this site";

            var win = new Window
            {
                Title = "Shields — " + host,
                Width = 380,
                Height = 460,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = System.Windows.Media.Brushes.White
            };

            var root = new StackPanel { Margin = new Thickness(16) };

            root.Children.Add(new TextBlock
            {
                Text = host,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            });

            root.Children.Add(new TextBlock
            {
                Text = $"{tab.BlockedCount} trackers & ads blocked on this page",
                Margin = new Thickness(0, 0, 0, 12),
                Foreground = System.Windows.Media.Brushes.DimGray
            });

            var isAllowed = AllowlistStore.IsAllowed(tab.CurrentHost);
            var toggle = new CheckBox
            {
                Content = "Disable blocking on this site",
                IsChecked = isAllowed,
                Margin = new Thickness(0, 0, 0, 14)
            };
            toggle.Checked += (_, _) =>
            {
                if (tab.CurrentHost == null) return;
                AllowlistStore.SetAllowed(tab.CurrentHost, true);
                tab.ShieldDisabledForCurrentSite = true;
                changed = true;
            };
            toggle.Unchecked += (_, _) =>
            {
                if (tab.CurrentHost == null) return;
                AllowlistStore.SetAllowed(tab.CurrentHost, false);
                tab.ShieldDisabledForCurrentSite = false;
                changed = true;
            };
            root.Children.Add(toggle);

            root.Children.Add(new Separator { Margin = new Thickness(0, 0, 0, 10) });
            root.Children.Add(new TextBlock { Text = "Blocked by domain", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });

            var listBox = new ListBox { MaxHeight = 240, BorderThickness = new Thickness(0) };
            foreach (var kv in tab.BlockedDomainCounts.OrderByDescending(k => k.Value))
            {
                listBox.Items.Add(new TextBlock { Text = $"{kv.Key}  —  {kv.Value}" });
            }
            if (tab.BlockedDomainCounts.Count == 0)
            {
                listBox.Items.Add(new TextBlock { Text = "Nothing blocked yet on this page.", FontStyle = FontStyles.Italic, Foreground = System.Windows.Media.Brushes.Gray });
            }
            root.Children.Add(listBox);

            var closeBtn = new Button { Content = "Close", Margin = new Thickness(0, 14, 0, 0), Padding = new Thickness(10, 6, 10, 6), HorizontalAlignment = HorizontalAlignment.Right };
            closeBtn.Click += (_, _) => win.Close();
            root.Children.Add(closeBtn);

            win.Content = root;
            win.ShowDialog();

            return changed;
        }
    }
}
