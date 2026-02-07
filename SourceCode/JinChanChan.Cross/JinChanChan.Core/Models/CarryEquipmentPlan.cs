namespace JinChanChan.Core.Models;

public sealed class CarryEquipmentPlan
{
    public string CarryHero { get; init; } = string.Empty;

    public IReadOnlyList<string> BestItems { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> BuildPath { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> CollectedItems { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> MissingItems { get; init; } = Array.Empty<string>();

    public double CompletionRate { get; init; }
}
