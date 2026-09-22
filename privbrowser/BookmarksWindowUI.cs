using System;
using System.Windows;
using System.Windows.Controls;

namespace PrivBrowser
{
    public static class BookmarksWindowUI
    {
        public static void Show(Window owner, Action<string> navigateTo)
        {
            var win = new Window
            {
                Title = "Bookmarks",
                Width = 420,
                Height = 480,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var root = new DockPanel { Margin = new Thickness(12) };

            var list = new ListBox();
            DockPanel.SetDock(list, Dock.Top);

            void Refresh()
            {
                list.Items.Clear();
                foreach (var b in BookmarkStore.GetAll())
                {
                    var row = new DockPanel { Margin = new Thickness(2) };

                    var text = new TextBlock
                    {
                        Text = string.IsNullOrWhiteSpace(b.Title) ? b.Url : b.Title,
                        ToolTip = b.Url,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    DockPanel.SetDock(text, Dock.Left);

                    var removeBtn = new Button { Content = "Remove", Padding = new Thickness(6, 2, 6, 2) };
                    removeBtn.Click += (_, _) => { BookmarkStore.Remove(b.Url); Refresh(); };
                    DockPanel.SetDock(removeBtn, Dock.Right);

                    row.Children.Add(removeBtn);
                    row.Children.Add(text);

                    var item = new ListBoxItem { Content = row, Tag = b.Url };
                    item.MouseDoubleClick += (_, _) => { navigateTo(b.Url); win.Close(); };
                    list.Items.Add(item);
                }
                if (list.Items.Count == 0)
                    list.Items.Add(new TextBlock { Text = "No bookmarks yet. Use the ☆ button to add one.", FontStyle = FontStyles.Italic, Margin = new Thickness(4) });
            }

            Refresh();

            var hint = new TextBlock { Text = "Double-click a bookmark to open it.", Foreground = System.Windows.Media.Brushes.Gray, Margin = new Thickness(0, 8, 0, 0) };
            DockPanel.SetDock(hint, Dock.Bottom);

            root.Children.Add(hint);
            root.Children.Add(list);

            win.Content = root;
            win.ShowDialog();
        }
    }
}
