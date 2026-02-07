using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class CardLoopEngine
{
    private readonly ICaptureService _captureService;
    private readonly IOcrEngine _ocrEngine;
    private readonly IMatchDecisionService _matchDecisionService;
    private readonly IRefreshDecisionService _refreshDecisionService;
    private readonly IInputService _inputService;
    private readonly ILoopMetricsSink _metricsSink;

    public CardLoopEngine(
        ICaptureService captureService,
        IOcrEngine ocrEngine,
        IMatchDecisionService matchDecisionService,
        IRefreshDecisionService refreshDecisionService,
        IInputService inputService,
        ILoopMetricsSink metricsSink)
    {
        _captureService = captureService;
        _ocrEngine = ocrEngine;
        _matchDecisionService = matchDecisionService;
        _refreshDecisionService = refreshDecisionService;
        _inputService = inputService;
        _metricsSink = metricsSink;
    }

    public async Task<LoopActionPlan> RunOnceAsync(
        IReadOnlyList<ScreenRect> cardNameRegions,
        IReadOnlyList<ScreenRect> cardClickRegions,
        IReadOnlyList<string> targetHeroes,
        bool refreshEnabled,
        bool strictMatching,
        bool isMousePressed,
        bool isRefreshInProgress,
        CancellationToken cancellationToken = default)
    {
        var loopStart = DateTimeOffset.UtcNow;

        if (cardNameRegions.Count == 0 || cardClickRegions.Count == 0)
        {
            _metricsSink.Track(new LoopMetricsSnapshot
            {
                Timestamp = DateTimeOffset.UtcNow,
                Mode = "CrossLoop",
                CaptureMs = 0,
                OcrMs = 0,
                MatchMs = 0,
                ActionMs = 0,
                LoopTotalMs = (DateTimeOffset.UtcNow - loopStart).TotalMilliseconds
            });

            return new LoopActionPlan
            {
                PurchaseSlotIndexes = [],
                ShouldRefreshStore = false,
                Reason = "坐标未配置，跳过本轮。"
            };
        }

        DateTimeOffset t0 = DateTimeOffset.UtcNow;
        IReadOnlyList<FrameImage> frames = await _captureService.CaptureBatchAsync(cardNameRegions, cancellationToken);
        double captureMs = (DateTimeOffset.UtcNow - t0).TotalMilliseconds;

        t0 = DateTimeOffset.UtcNow;
        OcrBatchResult ocrResult = await _ocrEngine.RecognizeAsync(frames, cancellationToken);
        double ocrMs = (DateTimeOffset.UtcNow - t0).TotalMilliseconds;

        t0 = DateTimeOffset.UtcNow;
        string[] corrected = ocrResult.RawTexts.ToArray();
        bool[] targetFlags = _matchDecisionService.MatchTargets(corrected, targetHeroes, strictMatching);
        bool hasTarget = targetFlags.Any(x => x);
        bool isStoreEmpty = corrected.All(string.IsNullOrWhiteSpace);
        List<int> slots = new();
        int slotCount = Math.Min(targetFlags.Length, cardClickRegions.Count);
        for (int i = 0; i < slotCount; i++)
        {
            if (targetFlags[i])
            {
                slots.Add(i);
            }
        }

        bool shouldRefresh = _refreshDecisionService.ShouldRefresh(refreshEnabled, isMousePressed, hasTarget, isStoreEmpty, isRefreshInProgress);
        double matchMs = (DateTimeOffset.UtcNow - t0).TotalMilliseconds;

        _metricsSink.Track(new LoopMetricsSnapshot
        {
            Timestamp = DateTimeOffset.UtcNow,
            Mode = "CrossLoop",
            CaptureMs = captureMs,
            OcrMs = ocrMs,
            MatchMs = matchMs,
            ActionMs = 0,
            LoopTotalMs = (DateTimeOffset.UtcNow - loopStart).TotalMilliseconds
        });

        return new LoopActionPlan
        {
            PurchaseSlotIndexes = slots,
            ShouldRefreshStore = shouldRefresh,
            Reason = shouldRefresh ? "无目标卡，执行刷新" : "本轮不刷新"
        };
    }

    public async Task ExecuteActionPlanAsync(
        LoopActionPlan plan,
        IReadOnlyList<ScreenRect> cardClickRegions,
        ScreenRect refreshRegion,
        bool useKeyboardPurchase,
        bool useKeyboardRefresh,
        IReadOnlyList<string>? purchaseKeys = null,
        string refreshKey = "D",
        CancellationToken cancellationToken = default)
    {
        if (plan == null)
        {
            return;
        }

        DateTimeOffset t0 = DateTimeOffset.UtcNow;

        foreach (int index in plan.PurchaseSlotIndexes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (index < 0 || index >= cardClickRegions.Count)
            {
                continue;
            }

            if (useKeyboardPurchase && purchaseKeys != null && index < purchaseKeys.Count)
            {
                await _inputService.PressKeyAsync(purchaseKeys[index], cancellationToken);
            }
            else
            {
                (int x, int y) = cardClickRegions[index].Center();
                await _inputService.MoveMouseAsync(x, y, cancellationToken);
                await _inputService.LeftClickAsync(cancellationToken);
            }
        }

        if (plan.ShouldRefreshStore)
        {
            if (useKeyboardRefresh)
            {
                await _inputService.PressKeyAsync(refreshKey, cancellationToken);
            }
            else
            {
                (int x, int y) = refreshRegion.Center();
                await _inputService.MoveMouseAsync(x, y, cancellationToken);
                await _inputService.LeftClickAsync(cancellationToken);
            }
        }

        _metricsSink.Track(new LoopMetricsSnapshot
        {
            Timestamp = DateTimeOffset.UtcNow,
            Mode = "CrossLoop-Action",
            CaptureMs = 0,
            OcrMs = 0,
            MatchMs = 0,
            ActionMs = (DateTimeOffset.UtcNow - t0).TotalMilliseconds,
            LoopTotalMs = (DateTimeOffset.UtcNow - t0).TotalMilliseconds
        });
    }
}
