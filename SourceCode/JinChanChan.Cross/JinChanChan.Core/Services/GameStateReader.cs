using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class GameStateReader : IGameStateReader
{
    public LiveGameState Read(
        IReadOnlyList<string> shopCards,
        IReadOnlyList<string> preferredTargets,
        bool autoPickEnabled,
        bool autoRefreshEnabled,
        DateTimeOffset timestamp,
        IReadOnlyList<string>? benchCards = null,
        string stage = "live")
    {
        string[] sanitizedShop = (shopCards ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(5)
            .ToArray();

        string[] sanitizedBench = (benchCards ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(9)
            .ToArray();

        string[] sanitizedTargets = (preferredTargets ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new LiveGameState
        {
            Timestamp = timestamp,
            ShopCards = sanitizedShop,
            BenchCards = sanitizedBench,
            PreferredTargets = sanitizedTargets,
            AutoPickEnabled = autoPickEnabled,
            AutoRefreshEnabled = autoRefreshEnabled,
            Stage = string.IsNullOrWhiteSpace(stage) ? "live" : stage.Trim()
        };
    }
}
