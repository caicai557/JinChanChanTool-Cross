using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class CarouselAdvisorService : ICarouselAdvisorService
{
    public CarouselSuggestion BuildSuggestion(
        LiveGameState gameState,
        LineupRecommendation? recommendation)
    {
        if (recommendation == null || recommendation.CarouselPriorities.Count == 0)
        {
            return new CarouselSuggestion
            {
                Priorities = ["反曲弓", "暴风大剑", "拳套"],
                Reason = "未匹配到稳定阵容，使用通用输出优先级。"
            };
        }

        return new CarouselSuggestion
        {
            Priorities = recommendation.CarouselPriorities,
            Reason = $"基于 {recommendation.LineupName} 的选秀优先级。"
        };
    }
}
