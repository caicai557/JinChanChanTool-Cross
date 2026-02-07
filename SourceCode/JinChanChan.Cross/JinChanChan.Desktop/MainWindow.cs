using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using JinChanChan.Desktop.ViewModels;

namespace JinChanChan.Desktop;

public sealed class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        Title = "JinChanChan Cross";
        Width = 980;
        Height = 640;

        TextBlock title = new()
        {
            Text = "JinChanChan 双端重构版",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 12)
        };

        TextBlock summary = new()
        {
            Text = viewModel.Summary,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 12)
        };

        TextBox logs = new()
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Text = viewModel.DiagnosticText
        };

        StackPanel root = new()
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                title,
                summary,
                logs
            }
        };

        Content = root;
    }
}
