using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PrivBrowser
{
    public static class VpnPanel
    {
        /// <summary>
        /// Shows the VPN/proxy settings window. Returns true if the saved
        /// config changed in a way that requires an app restart to apply
        /// (proxy settings are applied once at CEF startup).
        /// </summary>
        public static bool Show(Window owner)
        {
            bool changed = false;
            var config = VpnSettingsStore.Load();

            var win = new Window
            {
                Title = "VPN / Proxy",
                Width = 380,
                Height = 420,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = System.Windows.Media.Brushes.White
            };

            var root = new StackPanel { Margin = new Thickness(16) };

            root.Children.Add(new TextBlock
            {
                Text = "Route browser traffic through a proxy/VPN server.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            });
            root.Children.Add(new TextBlock
            {
                Text = "PrivBrowser doesn't operate its own VPN servers — enter " +
                       "the server details from your own VPN or proxy provider below.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.DimGray,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var freeNote = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 14)
            };
            freeNote.Inlines.Add(new Run("Note: "){ FontWeight = FontWeights.SemiBold });
            freeNote.Inlines.Add(new Run(
                "proxy (SOCKS5/HTTP) access is a paid-tier feature on essentially every " +
                "major VPN provider — free plans generally don't include it. If you want " +
                "a genuinely free VPN, install a provider's own app (e.g. Proton VPN Free) " +
                "and connect it system-wide — PrivBrowser will automatically be protected " +
                "along with everything else on your PC, no setup needed here."));
            root.Children.Add(freeNote);

            var enabledBox = new CheckBox { Content = "Enable proxy", IsChecked = config.Enabled, Margin = new Thickness(0, 0, 0, 10) };
            root.Children.Add(enabledBox);

            root.Children.Add(new TextBlock { Text = "Protocol", Margin = new Thickness(0, 0, 0, 2) });
            var protocolBox = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
            protocolBox.Items.Add("socks5");
            protocolBox.Items.Add("http");
            protocolBox.SelectedItem = config.Protocol;
            root.Children.Add(protocolBox);

            root.Children.Add(new TextBlock { Text = "Server host", Margin = new Thickness(0, 0, 0, 2) });
            var hostBox = new TextBox { Text = config.Host, Margin = new Thickness(0, 0, 0, 10) };
            root.Children.Add(hostBox);

            root.Children.Add(new TextBlock { Text = "Port", Margin = new Thickness(0, 0, 0, 2) });
            var portBox = new TextBox { Text = config.Port.ToString(), Margin = new Thickness(0, 0, 0, 10) };
            root.Children.Add(portBox);

            root.Children.Add(new TextBlock { Text = "Username (optional)", Margin = new Thickness(0, 0, 0, 2) });
            var userBox = new TextBox { Text = config.Username, Margin = new Thickness(0, 0, 0, 10) };
            root.Children.Add(userBox);

            root.Children.Add(new TextBlock { Text = "Password (optional)", Margin = new Thickness(0, 0, 0, 2) });
            var passBox = new PasswordBox { Password = config.Password, Margin = new Thickness(0, 0, 0, 14) };
            root.Children.Add(passBox);

            var note = new TextBlock
            {
                Text = "Restart PrivBrowser after saving for changes to take effect.",
                FontStyle = FontStyles.Italic,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 10)
            };
            root.Children.Add(note);

            var buttonRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancelBtn = new Button { Content = "Cancel", Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(0, 0, 8, 0) };
            var saveBtn = new Button { Content = "Save", Padding = new Thickness(10, 6, 10, 6) };

            cancelBtn.Click += (_, _) => win.Close();
            saveBtn.Click += (_, _) =>
            {
                if (!int.TryParse(portBox.Text, out var port)) port = config.Port;

                var newConfig = new VpnConfig
                {
                    Enabled = enabledBox.IsChecked == true,
                    Protocol = protocolBox.SelectedItem as string ?? "socks5",
                    Host = hostBox.Text.Trim(),
                    Port = port,
                    Username = userBox.Text.Trim(),
                    Password = passBox.Password
                };

                VpnSettingsStore.Save(newConfig);
                changed = true;
                win.Close();
            };

            buttonRow.Children.Add(cancelBtn);
            buttonRow.Children.Add(saveBtn);
            root.Children.Add(buttonRow);

            win.Content = root;
            win.ShowDialog();

            return changed;
        }
    }
}
