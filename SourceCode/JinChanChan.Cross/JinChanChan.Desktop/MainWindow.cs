using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using JinChanChan.Core.Models;
using JinChanChan.Desktop.Runtime;
using JinChanChan.Desktop.ViewModels;
using System.Text;

namespace JinChanChan.Desktop;

public sealed class MainWindow : Window
{
    private readonly TextBox _advisorBox;

    public MainWindow(MainWindowViewModel viewModel, CrossLoopRuntime runtime)
    {
        Title = "JinChanChan Cross";
        Width = 1200;
        Height = 720;

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
            Text = viewModel.DiagnosticText
        };

        _advisorBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Text = viewModel.AdvisorText
        };

        TextBlock advisorTitle = new()
        {
            Text = "实时建议面板",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 8)
        };

        Grid contentGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,*")
        };

        contentGrid.Children.Add(summary);
        Grid.SetColumn(summary, 0);
        Grid.SetColumnSpan(summary, 2);

        StackPanel leftPanel = new()
        {
            Margin = new Avalonia.Thickness(0),
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                logs
            }
        };

        StackPanel rightPanel = new()
        {
            Margin = new Avalonia.Thickness(12, 0, 0, 0),
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                advisorTitle,
                _advisorBox
            }
        };

        contentGrid.Children.Add(leftPanel);
        Grid.SetRow(leftPanel, 1);
        Grid.SetColumn(leftPanel, 0);

        contentGrid.Children.Add(rightPanel);
        Grid.SetRow(rightPanel, 1);
        Grid.SetColumn(rightPanel, 1);

        StackPanel root = new()
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                title,
                contentGrid
            }
        };

        Content = root;

        runtime.AdvisorUpdated += OnAdvisorUpdated;
        Closed += (_, _) => runtime.AdvisorUpdated -= OnAdvisorUpdated;
    }

    private void OnAdvisorUpdated(AdvisorSnapshot snapshot)
    {
        Dispatcher.UIThread.Post(() => _advisorBox.Text = FormatAdvisorSnapshot(snapshot));
    }

    private static string FormatAdvisorSnapshot(AdvisorSnapshot snapshot)
    {
        StringBuilder builder = new();
        builder.AppendLine("[最新建议]");
        builder.AppendLine($"时间: {snapshot.GeneratedAt:HH:mm:ss}");
        builder.AppendLine($"摘要: {snapshot.Summary}");

        if (snapshot.Recommendation != null)
        {
            builder.AppendLine();
            builder.AppendLine("[动态阵容推荐]");
            builder.AppendLine($"阵容: {snapshot.Recommendation.LineupName} ({snapshot.Recommendation.Tier})");
            builder.AppendLine($"匹配分: {snapshot.Recommendation.MatchScore.TotalScore:P0}");
            builder.AppendLine($"命中英雄: {string.Join("、", snapshot.Recommendation.MatchedHeroes)}");
            builder.AppendLine($"缺失英雄: {string.Join("、", snapshot.Recommendation.MissingHeroes)}");
            builder.AppendLine("一图流:");
            foreach (string step in snapshot.Recommendation.OnePageGuide)
            {
                builder.AppendLine($"- {step}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("[资源管理辅助]");
        if (snapshot.BenchSellSuggestions.Count == 0)
        {
            builder.AppendLine("- 当前无冗余棋子出售建议");
        }
        else
        {
            foreach (BenchSellSuggestion suggestion in snapshot.BenchSellSuggestions.Take(5))
            {
                builder.AppendLine($"- {suggestion.HeroName}: {suggestion.Reason}");
            }
        }

        if (snapshot.CarouselSuggestion != null)
        {
            builder.AppendLine($"选秀优先级: {string.Join(" > ", snapshot.CarouselSuggestion.Priorities)}");
        }

        builder.AppendLine();
        builder.AppendLine("[装备与符文决策]");
        if (snapshot.CarryEquipmentPlan != null)
        {
            builder.AppendLine($"主C: {snapshot.CarryEquipmentPlan.CarryHero}");
            builder.AppendLine($"三件套: {string.Join("、", snapshot.CarryEquipmentPlan.BestItems)}");
            builder.AppendLine($"收集进度: {snapshot.CarryEquipmentPlan.CompletionRate:P0}");
        }

        if (snapshot.AugmentSuggestion != null)
        {
            builder.AppendLine($"符文推荐: {string.Join("、", snapshot.AugmentSuggestion.PrimaryChoices)}");
            builder.AppendLine($"备选符文: {string.Join("、", snapshot.AugmentSuggestion.BackupChoices)}");
        }

        return builder.ToString();
    }
}
