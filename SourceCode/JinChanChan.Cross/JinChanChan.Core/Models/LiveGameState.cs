namespace JinChanChan.Core.Models;

public sealed class LiveGameState
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<string> ShopCards { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> BenchCards { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> PreferredTargets { get; init; } = Array.Empty<string>();

    public bool AutoPickEnabled { get; init; }

    public bool AutoRefreshEnabled { get; init; }

    public string Stage { get; init; } = "unknown";
}
