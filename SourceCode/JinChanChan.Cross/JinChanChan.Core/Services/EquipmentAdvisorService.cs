using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class EquipmentAdvisorService : IEquipmentAdvisorService
{
    public CarryEquipmentPlan BuildPlan(
        LiveGameState gameState,
        LineupRecommendation? recommendation)
    {
        if (recommendation == null || recommendation.RecommendedItems.Count == 0)
        {
            return new CarryEquipmentPlan
            {
                CarryHero = string.Empty,
                BestItems = ["无尽", "轻语", "巨杀"],
                BuildPath = ["拳套", "暴风大剑", "反曲弓"],
                CollectedItems = Array.Empty<string>(),
                MissingItems = ["无尽", "轻语", "巨杀"],
                CompletionRate = 0
            };
        }

        HashSet<string> collected = new(
            gameState.ShopCards.Concat(gameState.BenchCards),
            StringComparer.Ordinal);

        string[] ready = recommendation.RecommendedItems.Where(collected.Contains).ToArray();
        string[] missing = recommendation.RecommendedItems.Where(x => !collected.Contains(x)).ToArray();
        double completion = recommendation.RecommendedItems.Count == 0
            ? 0
            : (double)ready.Length / recommendation.RecommendedItems.Count;

        return new CarryEquipmentPlan
        {
            CarryHero = recommendation.CarryHero,
            BestItems = recommendation.RecommendedItems,
            BuildPath = recommendation.CarouselPriorities,
            CollectedItems = ready,
            MissingItems = missing,
            CompletionRate = completion
        };
    }
}
