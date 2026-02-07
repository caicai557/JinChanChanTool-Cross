namespace JinChanChan.Core.Config;

public sealed class AppSettings
{
    public string SchemaVersion { get; set; } = "1.0.0";

    public HotkeySettings Hotkeys { get; set; } = new();

    public CoordinateSettings Coordinates { get; set; } = new();

    public bool EnableAutoPick { get; set; } = true;

    public bool EnableAutoRefresh { get; set; } = true;

    public bool UseKeyboardPurchase { get; set; }

    public bool UseMousePurchase { get; set; } = true;

    public bool UseKeyboardRefresh { get; set; }

    public bool UseMouseRefresh { get; set; } = true;

    public bool EnablePerfMetrics { get; set; } = true;

    public bool UseNewLoopEngine { get; set; } = true;

    public int CpuOcrConsumerCount { get; set; } = 1;

    public bool OcrWarmupEnabled { get; set; } = true;

    public bool StrictMatching { get; set; }

    public bool EnableLineupAdvisor { get; set; } = true;

    public bool EnableBenchSellHint { get; set; } = true;

    public bool EnableCarouselHint { get; set; } = true;

    public bool EnableAugmentHint { get; set; } = true;

    public int AdvisorTickMs { get; set; } = 200;

    public string LineupDataSource { get; set; } = "local";

    public double OverlayOpacity { get; set; } = 0.85;

    public int RecommendationStabilityWindow { get; set; } = 3;

    public List<string> PurchaseKeys { get; set; } = ["Q", "W", "E", "R", "T"];

    public string RefreshKey { get; set; } = "D";

    public string TargetProcessName { get; set; } = string.Empty;

    public int TargetProcessId { get; set; }

    public List<string> PreferredTargets { get; set; } = new();

    public Dictionary<string, JsonElement> Legacy { get; set; } = new();
}
