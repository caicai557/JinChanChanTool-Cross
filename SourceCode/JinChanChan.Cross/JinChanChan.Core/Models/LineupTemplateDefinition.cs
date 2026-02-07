namespace JinChanChan.Core.Models;

public sealed class LineupTemplateDefinition
{
    public string Name { get; init; } = string.Empty;

    public string Tier { get; init; } = "B";

    public IReadOnlyList<string> Heroes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Traits { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> KeyUnits { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Guide { get; init; } = Array.Empty<string>();

    public string CarryHero { get; init; } = string.Empty;

    public IReadOnlyList<string> CarryItems { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Augments { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> CarouselPriorities { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> SearchTokens { get; init; } = Array.Empty<string>();
}
