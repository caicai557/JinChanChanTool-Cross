namespace JinChanChan.Core.Models;

public sealed class BenchSellSuggestion
{
    public string HeroName { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public int Priority { get; init; }
}
