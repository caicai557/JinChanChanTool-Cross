using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Diagnostics;
using System.Text.Json;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;
using JinChanChan.Core.Services;
using JinChanChan.Core.Utilities;
using JinChanChan.Desktop.Runtime;
using JinChanChan.Desktop.ViewModels;
using JinChanChan.Ocr.Engines;
using JinChanChan.Ocr.Models;
#if WINDOWS
using JinChanChan.Platform.Windows.Services;
#elif MACOS
using JinChanChan.Platform.Mac.Services;
#endif

namespace JinChanChan.Desktop;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string resourcesPath = Path.Combine(AppContext.BaseDirectory, "Resources");
            ConfigMigrator migrator = new();
            var (settings, report) = await migrator.MigrateAsync(resourcesPath);
            string crossSettingsPath = Path.Combine(resourcesPath, "Cross");
            await JsonFileStore.SaveAsync(Path.Combine(crossSettingsPath, "AppSettings.json"), settings);

#if WINDOWS
            var capture = new WindowsCaptureService();
            var input = new WindowsInputService();
            var windowLocator = new WindowsWindowLocatorService();
            var permission = new WindowsPermissionService();
            var hotkey = new WindowsHotkeyService();
            var capabilities = WindowsPlatformInfo.Capabilities;
#elif MACOS
            var capture = new MacCaptureService();
            var input = new MacInputService();
            var windowLocator = new MacWindowLocatorService();
            var permission = new MacPermissionService();
            var hotkey = new MacHotkeyService();
            var capabilities = MacPlatformInfo.Capabilities;
#else
            throw new PlatformNotSupportedException("当前目标平台不受支持。");
#endif

            string recognizerPath = Path.Combine(resourcesPath, "Models", "PP-OCRv5-mobile-rec.onnx");
            OnnxPPOcrEngine ocr = new(new OnnxPPOcrOptions
            {
                RecognizerModelPath = recognizerPath,
                KeysFilePath = Path.Combine(resourcesPath, "Models", "ppocr_keys_v1.txt"),
#if WINDOWS
                PreferredProvider = OnnxExecutionProvider.DirectMl,
#else
                PreferredProvider = OnnxExecutionProvider.Cpu,
#endif
                AllowProviderFallback = true,
                IntraOpNumThreads = settings.CpuOcrConsumerCount
            });

            CardLoopEngine loopEngine = new(
                capture,
                ocr,
                new MatchDecisionService(),
                new RefreshDecisionService(),
                input,
                new LoopMetricsSink());

            ILineupMatcherService lineupMatcher = new LineupMatcherService();
            ILineupRecommendationService lineupRecommendation = new LineupRecommendationService(lineupMatcher);
            StrategyAdvisorService strategyAdvisor = new(
                lineupRecommendation,
                new BenchAdvisorService(),
                new CarouselAdvisorService(),
                new EquipmentAdvisorService(),
                new AugmentAdvisorService());

            CrossLoopRuntime runtime = new(
                loopEngine,
                settings,
                new GameStateReader(),
                strategyAdvisor,
                new NoopOverlayPresenter());
            runtime.StatusChanged += message => Trace.WriteLine(message);

            Dictionary<PermissionKind, bool> permissionStates = new();
            if (capabilities.RequiresScreenRecordingPermission)
            {
                permissionStates[PermissionKind.ScreenRecording] = await EnsurePermissionAsync(permission, PermissionKind.ScreenRecording, report);
            }

            if (capabilities.RequiresAccessibilityPermission)
            {
                permissionStates[PermissionKind.Accessibility] = await EnsurePermissionAsync(permission, PermissionKind.Accessibility, report);
            }

            string targetProcessName = ResolveTargetProcessName(settings);
            WindowDescriptor? boundWindow = null;
            if (!string.IsNullOrWhiteSpace(targetProcessName))
            {
                boundWindow = await windowLocator.FindBestGameWindowAsync(targetProcessName);
                if (boundWindow == null)
                {
                    report.Warnings.Add($"自动窗口绑定失败：未找到进程/窗口 {targetProcessName}");
                }
            }

            if (settings.OcrWarmupEnabled)
            {
                await WarmupOcrAsync(ocr);
            }

            MainWindowViewModel vm = new(
                settings,
                report,
                capabilities,
                windowLocator,
                permission,
                hotkey,
                loopEngine,
                boundWindow,
                permissionStates);

            desktop.MainWindow = new MainWindow(vm, runtime);
            RegisterHotkeys(hotkey, settings, desktop, runtime, report);
            runtime.Start();
            await JsonFileStore.SaveAsync(Path.Combine(crossSettingsPath, "MigrationReport.json"), report);
            desktop.Exit += (_, _) =>
            {
                runtime.Dispose();
                hotkey.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task<bool> EnsurePermissionAsync(
        IPermissionService permissionService,
        PermissionKind permissionKind,
        MigrationReport report)
    {
        bool granted = await permissionService.CheckPermissionAsync(permissionKind);
        if (granted)
        {
            return true;
        }

        bool requested = await permissionService.RequestPermissionAsync(permissionKind);
        if (!requested)
        {
            report.Warnings.Add($"权限申请失败：{permissionKind}");
            return false;
        }

        bool grantedAfterRequest = await permissionService.CheckPermissionAsync(permissionKind);
        if (!grantedAfterRequest)
        {
            report.Warnings.Add($"权限未授予：{permissionKind}");
        }

        return grantedAfterRequest;
    }

    private static string ResolveTargetProcessName(JinChanChan.Core.Config.AppSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.TargetProcessName))
        {
            return settings.TargetProcessName;
        }

        if (settings.Legacy.TryGetValue("TargetProcessName", out JsonElement legacyNode) && legacyNode.ValueKind == JsonValueKind.String)
        {
            string? targetProcess = legacyNode.GetString();
            if (!string.IsNullOrWhiteSpace(targetProcess))
            {
                return targetProcess;
            }
        }

        return string.Empty;
    }

    private static async Task WarmupOcrAsync(OnnxPPOcrEngine ocr)
    {
        FrameImage warmupFrame = new()
        {
            Pixels = new byte[48 * 48 * 4],
            Width = 48,
            Height = 48,
            Channels = 4,
            Source = "warmup"
        };

        _ = await ocr.RecognizeAsync([warmupFrame]);
    }

    private static void RegisterHotkeys(
        IHotkeyService hotkeyService,
        JinChanChan.Core.Config.AppSettings settings,
        IClassicDesktopStyleApplicationLifetime desktop,
        CrossLoopRuntime runtime,
        MigrationReport report)
    {
        hotkeyService.UnregisterAll();

        TryRegisterHotkey(hotkeyService, settings.Hotkeys.ToggleMainWindow, "ToggleMainWindow", report, () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (desktop.MainWindow == null)
                {
                    return;
                }

                if (desktop.MainWindow.IsVisible)
                {
                    desktop.MainWindow.Hide();
                }
                else
                {
                    desktop.MainWindow.Show();
                    desktop.MainWindow.Activate();
                }
            });
        });

        TryRegisterHotkey(hotkeyService, settings.Hotkeys.ToggleAutoPick, "ToggleAutoPick", report, runtime.ToggleAutoPick);
        TryRegisterHotkey(hotkeyService, settings.Hotkeys.ToggleRefresh, "ToggleRefresh", report, runtime.ToggleAutoRefresh);
        TryRegisterHotkey(
            hotkeyService,
            settings.Hotkeys.HoldRoll,
            "HoldRoll",
            report,
            () => runtime.SetHoldRoll(true),
            () => runtime.SetHoldRoll(false));
    }

    private static void TryRegisterHotkey(
        IHotkeyService hotkeyService,
        string key,
        string keyName,
        MigrationReport report,
        Action onPressed,
        Action? onReleased = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            report.Warnings.Add($"热键为空: {keyName}");
            return;
        }

        try
        {
            hotkeyService.Register(key, onPressed, onReleased);
        }
        catch (Exception ex)
        {
            report.Warnings.Add($"热键注册失败 {keyName}({key}): {ex.Message}");
        }
    }
}
