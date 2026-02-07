using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IBenchAdvisorService
{
    IReadOnlyList<BenchSellSuggestion> BuildSuggestions(
        LiveGameState gameState,
        LineupRecommendation? recommendation);
}
