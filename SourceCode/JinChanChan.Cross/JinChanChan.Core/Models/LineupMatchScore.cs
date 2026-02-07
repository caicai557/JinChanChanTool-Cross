namespace JinChanChan.Core.Models;

public sealed class LineupMatchScore
{
    public double HeroMatchRatio { get; init; }

    public double TraitMatchRatio { get; init; }

    public double KeyUnitWeight { get; init; }

    public double TotalScore { get; init; }
}
