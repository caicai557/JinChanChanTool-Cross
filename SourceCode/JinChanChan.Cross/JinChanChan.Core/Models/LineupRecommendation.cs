namespace JinChanChan.Core.Models;

public sealed class LineupRecommendation
{
    public string LineupName { get; init; } = string.Empty;

    public string Tier { get; init; } = "A";

    public LineupMatchScore MatchScore { get; init; } = new();

    public IReadOnlyList<string> MatchedHeroes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> MissingHeroes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Traits { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> SearchTokens { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> OnePageGuide { get; init; } = Array.Empty<string>();

    public string CarryHero { get; init; } = string.Empty;

    public IReadOnlyList<string> RecommendedItems { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> RecommendedAugments { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> CarouselPriorities { get; init; } = Array.Empty<string>();
}
