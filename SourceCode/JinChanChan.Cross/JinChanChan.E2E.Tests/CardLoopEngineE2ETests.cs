using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;
using JinChanChan.Core.Services;

namespace JinChanChan.E2E.Tests;

public class CardLoopEngineE2ETests
{
    [Fact]
    public async Task RunOnce_ShouldReturnPurchaseAndRefreshDecision()
    {
        FakeCaptureService capture = new();
        FakeOcrEngine ocr = new(["艾克", "", "", "", ""]);
        MatchDecisionService match = new();
        RefreshDecisionService refresh = new();
        FakeInputService input = new();
        LoopMetricsSink metrics = new();

        CardLoopEngine engine = new(capture, ocr, match, refresh, input, metrics);

        LoopActionPlan plan = await engine.RunOnceAsync(
            cardNameRegions: [new ScreenRect(0, 0, 1, 1), new ScreenRect(0, 0, 1, 1), new ScreenRect(0, 0, 1, 1), new ScreenRect(0, 0, 1, 1), new ScreenRect(0, 0, 1, 1)],
            cardClickRegions: [new ScreenRect(0, 0, 10, 10), new ScreenRect(10, 0, 10, 10), new ScreenRect(20, 0, 10, 10), new ScreenRect(30, 0, 10, 10), new ScreenRect(40, 0, 10, 10)],
            targetHeroes: ["艾克"],
            refreshEnabled: true,
            strictMatching: true,
            isMousePressed: false,
            isRefreshInProgress: false);

        Assert.Single(plan.PurchaseSlotIndexes);
        Assert.False(plan.ShouldRefreshStore);
        Assert.Equal("艾克", plan.RecognizedCards[0]);

        await engine.ExecuteActionPlanAsync(
            plan,
            cardClickRegions: [new ScreenRect(0, 0, 10, 10), new ScreenRect(10, 0, 10, 10), new ScreenRect(20, 0, 10, 10), new ScreenRect(30, 0, 10, 10), new ScreenRect(40, 0, 10, 10)],
            refreshRegion: new ScreenRect(100, 100, 20, 20),
            useKeyboardPurchase: false,
            useKeyboardRefresh: false);

        Assert.True(input.LeftClickCount >= 1);
    }

    [Fact]
    public async Task RunOnce_ShouldSkipWhenCoordinatesMissing()
    {
        FakeCaptureService capture = new();
        FakeOcrEngine ocr = new(["", "", "", "", ""]);
        MatchDecisionService match = new();
        RefreshDecisionService refresh = new();
        FakeInputService input = new();
        LoopMetricsSink metrics = new();

        CardLoopEngine engine = new(capture, ocr, match, refresh, input, metrics);
        LoopActionPlan plan = await engine.RunOnceAsync(
            cardNameRegions: [],
            cardClickRegions: [],
            targetHeroes: ["艾克"],
            refreshEnabled: true,
            strictMatching: true,
            isMousePressed: false,
            isRefreshInProgress: false);

        Assert.Empty(plan.PurchaseSlotIndexes);
        Assert.False(plan.ShouldRefreshStore);
        Assert.Empty(plan.RecognizedCards);
    }

    private sealed class FakeCaptureService : ICaptureService
    {
        public Task<FrameImage> CaptureAsync(ScreenRect region, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FrameImage
            {
                Pixels = new byte[region.Width * Math.Max(1, region.Height) * 4],
                Width = region.Width,
                Height = region.Height,
                Channels = 4,
                Source = "fake"
            });
        }

        public Task<IReadOnlyList<FrameImage>> CaptureBatchAsync(IReadOnlyList<ScreenRect> regions, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<FrameImage>>(regions.Select(r => new FrameImage
            {
                Pixels = new byte[r.Width * Math.Max(1, r.Height) * 4],
                Width = r.Width,
                Height = r.Height,
                Channels = 4,
                Source = "fake"
            }).ToArray());
        }
    }

    private sealed class FakeOcrEngine : IOcrEngine
    {
        private readonly string[] _values;

        public FakeOcrEngine(string[] values)
        {
            _values = values;
        }

        public Task<OcrBatchResult> RecognizeAsync(IReadOnlyList<FrameImage> frames, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new OcrBatchResult
            {
                RawTexts = _values,
                Provider = "Fake",
                Elapsed = TimeSpan.FromMilliseconds(1),
                UsedFallbackProvider = false
            });
        }
    }

    private sealed class FakeInputService : IInputService
    {
        public int LeftClickCount { get; private set; }

        public Task MoveMouseAsync(int x, int y, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LeftClickAsync(CancellationToken cancellationToken = default)
        {
            LeftClickCount++;
            return Task.CompletedTask;
        }

        public Task PressKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
