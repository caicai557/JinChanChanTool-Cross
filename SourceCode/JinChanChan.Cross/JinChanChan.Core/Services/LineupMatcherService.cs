using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class LineupMatcherService : ILineupMatcherService
{
    public LineupMatchScore CalculateScore(
        LiveGameState gameState,
        IReadOnlyList<string> lineupHeroes,
        IReadOnlyList<string> lineupTraits,
        IReadOnlyList<string> keyUnits)
    {
        HashSet<string> source = new(
            (gameState.ShopCards ?? Array.Empty<string>())
                .Concat(gameState.BenchCards ?? Array.Empty<string>())
                .Concat(gameState.PreferredTargets ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.Ordinal);

        HashSet<string> heroes = new(
            (lineupHeroes ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.Ordinal);

        HashSet<string> traits = new(
            (lineupTraits ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.Ordinal);

        HashSet<string> key = new(
            (keyUnits ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.Ordinal);

        int heroHits = heroes.Count == 0 ? 0 : heroes.Count(source.Contains);
        int keyHits = key.Count == 0 ? 0 : key.Count(source.Contains);
        int traitHits = traits.Count == 0 ? 0 : traits.Count(source.Contains);

        double heroMatchRatio = heroes.Count == 0 ? 0 : (double)heroHits / heroes.Count;
        double traitMatchRatio = traits.Count == 0 ? 0 : (double)traitHits / traits.Count;
        double keyWeight = key.Count == 0 ? 0 : (double)keyHits / key.Count;

        double total = (heroMatchRatio * 0.55) + (traitMatchRatio * 0.20) + (keyWeight * 0.25);

        return new LineupMatchScore
        {
            HeroMatchRatio = heroMatchRatio,
            TraitMatchRatio = traitMatchRatio,
            KeyUnitWeight = keyWeight,
            TotalScore = Math.Clamp(total, 0, 1)
        };
    }
}
