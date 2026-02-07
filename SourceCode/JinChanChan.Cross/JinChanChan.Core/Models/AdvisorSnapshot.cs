namespace JinChanChan.Core.Models;

public sealed class AdvisorSnapshot
{
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;

    public LiveGameState GameState { get; init; } = new();

    public LineupRecommendation? Recommendation { get; init; }

    public IReadOnlyList<BenchSellSuggestion> BenchSellSuggestions { get; init; } = Array.Empty<BenchSellSuggestion>();

    public CarouselSuggestion? CarouselSuggestion { get; init; }

    public CarryEquipmentPlan? CarryEquipmentPlan { get; init; }

    public AugmentSuggestion? AugmentSuggestion { get; init; }

    public string Summary { get; init; } = string.Empty;
}
