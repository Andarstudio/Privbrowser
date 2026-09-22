using System;
using System.Windows;
using System.Windows.Controls;

namespace PrivBrowser
{
    public static class HistoryWindowUI
    {
        public static void Show(Window owner, Action<string> navigateTo)
        {
            var win = new Window
            {
                Title = "History",
                Width = 460,
                Height = 520,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var root = new DockPanel { Margin = new Thickness(12) };

            var list = new ListBox();
            DockPanel.SetDock(list, Dock.Top);

            void Refresh()
            {
                list.Items.Clear();
                foreach (var h in HistoryStore.GetAll())
                {
                    var local = h.VisitedAtUtc.ToLocalTime();
                    var text = new TextBlock
                    {
                        Text = $"{local:g}  —  {(string.IsNullOrWhiteSpace(h.Title) ? h.Url : h.Title)}",
                        ToolTip = h.Url,
                        Margin = new Thickness(2)
                    };
                    var item = new ListBoxItem { Content = text, Tag = h.Url };
                    item.MouseDoubleClick += (_, _) => { navigateTo(h.Url); win.Close(); };
                    list.Items.Add(item);
                }
                if (list.Items.Count == 0)
                    list.Items.Add(new TextBlock { Text = "No history yet.", FontStyle = FontStyles.Italic, Margin = new Thickness(4) });
            }

            Refresh();

            var bottomBar = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
            DockPanel.SetDock(bottomBar, Dock.Bottom);

            var clearBtn = new Button { Content = "Clear history", Padding = new Thickness(10, 6, 10, 6) };
            clearBtn.Click += (_, _) =>
            {
                if (MessageBox.Show(win, "Clear all browsing history?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    HistoryStore.Clear();
                    Refresh();
                }
            };
            DockPanel.SetDock(clearBtn, Dock.Left);

            var hint = new TextBlock
            {
                Text = "Double-click an entry to open it.",
                Foreground = System.Windows.Media.Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            bottomBar.Children.Add(clearBtn);
            bottomBar.Children.Add(hint);

            root.Children.Add(bottomBar);
            root.Children.Add(list);

            win.Content = root;
            win.ShowDialog();
        }
    }
}
