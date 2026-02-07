using System.Text;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Config;
using JinChanChan.Core.Models;
using JinChanChan.Core.Services;

namespace JinChanChan.Desktop.ViewModels;

public sealed class MainWindowViewModel
{
    public string Summary { get; }

    public string DiagnosticText { get; }

    public string AdvisorText { get; }

    public MainWindowViewModel(
        AppSettings settings,
        MigrationReport migrationReport,
        PlatformCapabilities capabilities,
        IWindowLocatorService windowLocatorService,
        IPermissionService permissionService,
        IHotkeyService hotkeyService,
        CardLoopEngine loopEngine,
        WindowDescriptor? boundWindow,
        IReadOnlyDictionary<PermissionKind, bool> permissionStates)
    {
        Summary = $"平台能力: Capture={capabilities.SupportsWindowCapture}, Input={capabilities.SupportsInputInjection}, Hotkey={capabilities.SupportsGlobalHotkey}";

        StringBuilder builder = new();
        builder.AppendLine("[迁移报告]");
        builder.AppendLine($"Success: {migrationReport.Success}");
        builder.AppendLine($"AppliedMappings: {migrationReport.AppliedMappings.Count}");
        builder.AppendLine($"Warnings: {migrationReport.Warnings.Count}");
        foreach (string warning in migrationReport.Warnings)
        {
            builder.AppendLine($"- {warning}");
        }

        builder.AppendLine();
        builder.AppendLine("[当前设置]");
        builder.AppendLine($"Hotkey1: {settings.Hotkeys.ToggleAutoPick}");
        builder.AppendLine($"DynamicCoordinates: {settings.Coordinates.UseDynamicCoordinates}");
        builder.AppendLine($"CardNameRects: {settings.Coordinates.CardNameRects.Count}");
        builder.AppendLine($"CardClickRects: {settings.Coordinates.CardClickRects.Count}");
        builder.AppendLine($"EnablePerfMetrics: {settings.EnablePerfMetrics}");
        builder.AppendLine($"UseNewLoopEngine: {settings.UseNewLoopEngine}");
        builder.AppendLine($"CpuOcrConsumerCount: {settings.CpuOcrConsumerCount}");
        builder.AppendLine($"StrictMatching: {settings.StrictMatching}");
        builder.AppendLine($"EnableAutoPick: {settings.EnableAutoPick}");
        builder.AppendLine($"EnableAutoRefresh: {settings.EnableAutoRefresh}");
        builder.AppendLine($"PurchaseKeys: {string.Join(",", settings.PurchaseKeys)}");
        builder.AppendLine($"RefreshKey: {settings.RefreshKey}");
        builder.AppendLine($"TargetProcessName: {settings.TargetProcessName}");
        builder.AppendLine($"TargetProcessId: {settings.TargetProcessId}");
        builder.AppendLine($"EnableLineupAdvisor: {settings.EnableLineupAdvisor}");
        builder.AppendLine($"EnableBenchSellHint: {settings.EnableBenchSellHint}");
        builder.AppendLine($"EnableCarouselHint: {settings.EnableCarouselHint}");
        builder.AppendLine($"EnableAugmentHint: {settings.EnableAugmentHint}");
        builder.AppendLine($"AdvisorTickMs: {settings.AdvisorTickMs}");
        builder.AppendLine($"LineupDataSource: {settings.LineupDataSource}");
        builder.AppendLine($"OverlayOpacity: {settings.OverlayOpacity:F2}");
        builder.AppendLine($"RecommendationStabilityWindow: {settings.RecommendationStabilityWindow}");

        builder.AppendLine();
        builder.AppendLine("[权限状态]");
        if (permissionStates.Count == 0)
        {
            builder.AppendLine("- 当前平台无需额外权限");
        }
        else
        {
            foreach (KeyValuePair<PermissionKind, bool> item in permissionStates)
            {
                builder.AppendLine($"- {item.Key}: {item.Value}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("[自动窗口绑定]");
        if (boundWindow == null)
        {
            builder.AppendLine("- 未找到匹配窗口");
        }
        else
        {
            builder.AppendLine($"- 标题: {boundWindow.Title}");
            builder.AppendLine($"- 进程: {boundWindow.ProcessName}");
            builder.AppendLine($"- 坐标: {boundWindow.Bounds.X},{boundWindow.Bounds.Y},{boundWindow.Bounds.Width},{boundWindow.Bounds.Height}");
        }

        DiagnosticText = builder.ToString();

        StringBuilder advisor = new();
        advisor.AppendLine("[智能建议]");
        advisor.AppendLine("等待运行时循环产生建议快照...");
        AdvisorText = advisor.ToString();

        _ = windowLocatorService;
        _ = permissionService;
        _ = hotkeyService;
        _ = loopEngine;
    }
}
