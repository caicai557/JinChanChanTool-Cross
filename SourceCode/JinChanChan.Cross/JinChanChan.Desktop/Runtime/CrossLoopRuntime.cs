using JinChanChan.Core.Config;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;
using JinChanChan.Core.Services;

namespace JinChanChan.Desktop.Runtime;

public sealed class CrossLoopRuntime : IDisposable
{
    private readonly CardLoopEngine _loopEngine;
    private readonly AppSettings _settings;
    private readonly IGameStateReader _gameStateReader;
    private readonly StrategyAdvisorService _strategyAdvisorService;
    private readonly IOverlayPresenter _overlayPresenter;
    private readonly object _sync = new();
    private readonly Queue<string> _recommendationHistory = new();
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private volatile bool _isRefreshInProgress;

    public bool AutoPickEnabled { get; private set; }
    public bool AutoRefreshEnabled { get; private set; }
    public bool HoldRollPressed { get; private set; }
    public AdvisorSnapshot? LastAdvisorSnapshot { get; private set; }

    public event Action<string>? StatusChanged;
    public event Action<AdvisorSnapshot>? AdvisorUpdated;

    public CrossLoopRuntime(CardLoopEngine loopEngine, AppSettings settings)
        : this(
            loopEngine,
            settings,
            new GameStateReader(),
            new StrategyAdvisorService(
                new LineupRecommendationService(new LineupMatcherService()),
                new BenchAdvisorService(),
                new CarouselAdvisorService(),
                new EquipmentAdvisorService(),
                new AugmentAdvisorService()),
            new NoopOverlayPresenter())
    {
    }

    public CrossLoopRuntime(
        CardLoopEngine loopEngine,
        AppSettings settings,
        IGameStateReader gameStateReader,
        StrategyAdvisorService strategyAdvisorService,
        IOverlayPresenter overlayPresenter)
    {
        _loopEngine = loopEngine;
        _settings = settings;
        _gameStateReader = gameStateReader;
        _strategyAdvisorService = strategyAdvisorService;
        _overlayPresenter = overlayPresenter;
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

        _overlayPresenter.Clear();
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        int delayMs = _settings.EnableLineupAdvisor ? Math.Max(50, _settings.AdvisorTickMs) : 80;

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

                if (_settings.EnableLineupAdvisor)
                {
                    LiveGameState gameState = _gameStateReader.Read(
                        plan.RecognizedCards,
                        _settings.PreferredTargets,
                        AutoPickEnabled,
                        AutoRefreshEnabled || HoldRollPressed,
                        DateTimeOffset.UtcNow);

                    AdvisorSnapshot snapshot = _strategyAdvisorService.BuildSnapshot(
                        gameState,
                        _settings.EnableBenchSellHint,
                        _settings.EnableCarouselHint,
                        _settings.EnableAugmentHint,
                        cancellationToken: cancellationToken);

                    if (ShouldPublishRecommendation(snapshot.Recommendation?.LineupName))
                    {
                        LastAdvisorSnapshot = snapshot;
                        _overlayPresenter.Present(snapshot);
                        AdvisorUpdated?.Invoke(snapshot);
                    }
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

            await Task.Delay(delayMs, cancellationToken);
        }

        StatusChanged?.Invoke("运行时循环已停止。");
    }

    private bool ShouldPublishRecommendation(string? recommendationName)
    {
        if (string.IsNullOrWhiteSpace(recommendationName))
        {
            return true;
        }

        int window = Math.Max(1, _settings.RecommendationStabilityWindow);
        if (window <= 1)
        {
            return true;
        }

        _recommendationHistory.Enqueue(recommendationName);
        while (_recommendationHistory.Count > window)
        {
            _recommendationHistory.Dequeue();
        }

        if (_recommendationHistory.Count < window)
        {
            return true;
        }

        string majority = _recommendationHistory
            .GroupBy(x => x, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .First().Key;

        return string.Equals(majority, recommendationName, StringComparison.Ordinal);
    }
}
