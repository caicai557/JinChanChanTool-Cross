using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IAugmentAdvisorService
{
    AugmentSuggestion BuildSuggestion(
        LiveGameState gameState,
        LineupRecommendation? recommendation);
}
