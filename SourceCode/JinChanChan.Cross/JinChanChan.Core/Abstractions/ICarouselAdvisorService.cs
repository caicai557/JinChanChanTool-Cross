using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface ICarouselAdvisorService
{
    CarouselSuggestion BuildSuggestion(
        LiveGameState gameState,
        LineupRecommendation? recommendation);
}
