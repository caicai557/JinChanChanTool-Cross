using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class BenchAdvisorService : IBenchAdvisorService
{
    public IReadOnlyList<BenchSellSuggestion> BuildSuggestions(
        LiveGameState gameState,
        LineupRecommendation? recommendation)
    {
        if (gameState.BenchCards.Count == 0)
        {
            return Array.Empty<BenchSellSuggestion>();
        }

        HashSet<string> keepSet = new(
            (recommendation?.MatchedHeroes ?? Array.Empty<string>())
                .Concat(recommendation?.MissingHeroes ?? Array.Empty<string>())
                .Concat(gameState.PreferredTargets),
            StringComparer.Ordinal);

        List<BenchSellSuggestion> suggestions = new();
        foreach (string hero in gameState.BenchCards)
        {
            if (string.IsNullOrWhiteSpace(hero))
            {
                continue;
            }

            if (keepSet.Contains(hero))
            {
                continue;
            }

            suggestions.Add(new BenchSellSuggestion
            {
                HeroName = hero,
                Reason = "当前阵容匹配度较低，可考虑卖出换取利息。",
                Priority = 1
            });
        }

        return suggestions;
    }
}
