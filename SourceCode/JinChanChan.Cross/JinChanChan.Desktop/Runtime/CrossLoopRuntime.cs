using JinChanChan.Core.Config;
using JinChanChan.Core.Models;
using JinChanChan.Core.Services;

namespace JinChanChan.Desktop.Runtime;

public sealed class CrossLoopRuntime : IDisposable
{
    private readonly CardLoopEngine _loopEngine;
    private readonly AppSettings _settings;
    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private volatile bool _isRefreshInProgress;

    public bool AutoPickEnabled { get; private set; }
    public bool AutoRefreshEnabled { get; private set; }
    public bool HoldRollPressed { get; private set; }

    public event Action<string>? StatusChanged;

    public CrossLoopRuntime(CardLoopEngine loopEngine, AppSettings settings)
    {
        _loopEngine = loopEngine;
        _settings = settings;
        AutoPickEnabled = settings.EnableAutoPick;
        AutoRefreshEnabled = settings.EnableAutoRefresh;
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_worker != null)
            {
                return;
            }

            _cts = new CancellationTokenSource();
            _worker = Task.Run(() => RunLoopAsync(_cts.Token));
            StatusChanged?.Invoke("运行时循环已启动。");
        }
    }

    public void ToggleAutoPick()
    {
        AutoPickEnabled = !AutoPickEnabled;
        StatusChanged?.Invoke($"自动拿牌: {(AutoPickEnabled ? "开启" : "关闭")}");
    }

    public void ToggleAutoRefresh()
    {
        AutoRefreshEnabled = !AutoRefreshEnabled;
        StatusChanged?.Invoke($"自动刷新: {(AutoRefreshEnabled ? "开启" : "关闭")}");
    }

    public void SetHoldRoll(bool pressed)
    {
        HoldRollPressed = pressed;
        StatusChanged?.Invoke($"长按D牌: {(pressed ? "按下" : "抬起")}");
    }

    public void Dispose()
    {
        CancellationTokenSource? cts;
        Task? worker;
        lock (_sync)
        {
            cts = _cts;
            worker = _worker;
            _cts = null;
            _worker = null;
        }

        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }

        if (worker != null)
        {
            try
            {
                worker.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_settings.Coordinates.CardNameRects.Count == 0
                    || _settings.Coordinates.CardClickRects.Count == 0
                    || _settings.Coordinates.RefreshButtonRect.IsEmpty)
                {
                    await Task.Delay(300, cancellationToken);
                    continue;
                }

                LoopActionPlan plan = await _loopEngine.RunOnceAsync(
                    _settings.Coordinates.CardNameRects,
                    _settings.Coordinates.CardClickRects,
                    _settings.PreferredTargets,
                    refreshEnabled: AutoRefreshEnabled || HoldRollPressed,
                    strictMatching: _settings.StrictMatching,
                    isMousePressed: false,
                    isRefreshInProgress: _isRefreshInProgress,
                    cancellationToken: cancellationToken);

                bool shouldPurchase = AutoPickEnabled && plan.PurchaseSlotIndexes.Count > 0;
                bool shouldRefresh = (AutoRefreshEnabled || HoldRollPressed) && plan.ShouldRefreshStore;
                if (shouldPurchase || shouldRefresh)
                {
                    LoopActionPlan executePlan = new()
                    {
                        PurchaseSlotIndexes = shouldPurchase ? plan.PurchaseSlotIndexes : [],
                        ShouldRefreshStore = shouldRefresh,
                        Reason = plan.Reason
                    };

                    _isRefreshInProgress = executePlan.ShouldRefreshStore;
                    await _loopEngine.ExecuteActionPlanAsync(
                        executePlan,
                        _settings.Coordinates.CardClickRects,
                        _settings.Coordinates.RefreshButtonRect,
                        _settings.UseKeyboardPurchase,
                        _settings.UseKeyboardRefresh,
                        _settings.PurchaseKeys,
                        _settings.RefreshKey,
                        cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"运行时循环异常: {ex.Message}");
            }
            finally
            {
                _isRefreshInProgress = false;
            }

            await Task.Delay(80, cancellationToken);
        }

        StatusChanged?.Invoke("运行时循环已停止。");
    }
}
