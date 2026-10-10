using Avalonia.Controls;
using Avalonia.Layout;

namespace PrivBrowser.Linux;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "PrivBrowser Linux — Starter";
        Width = 1000;
        Height = 700;
        MinWidth = 640;
        MinHeight = 420;

        var heading = new TextBlock
        {
            Text = "PrivBrowser Linux",
            FontSize = 26,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var note = new TextBlock
        {
            Text = "Linux UI starter only. The browser engine and existing Windows features still need to be ported.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            MaxWidth = 700
        };
        Content = new StackPanel
        {
            Spacing = 18,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { heading, note }
        };
    }
}
