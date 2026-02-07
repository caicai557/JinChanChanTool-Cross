using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class AugmentAdvisorService : IAugmentAdvisorService
{
    public AugmentSuggestion BuildSuggestion(
        LiveGameState gameState,
        LineupRecommendation? recommendation)
    {
        if (recommendation == null || recommendation.RecommendedAugments.Count == 0)
        {
            return new AugmentSuggestion
            {
                PrimaryChoices = ["成吨利息"],
                BackupChoices = ["治疗法球", "珠光莲花"],
                Reason = "未命中特定阵容，采用通用经济+战力组合。"
            };
        }

        string[] primary = recommendation.RecommendedAugments.Take(2).ToArray();
        string[] backup = recommendation.RecommendedAugments.Skip(2).DefaultIfEmpty("治疗法球").ToArray();

        return new AugmentSuggestion
        {
            PrimaryChoices = primary,
            BackupChoices = backup,
            Reason = $"基于 {recommendation.LineupName} 的符文契合度排序。"
        };
    }
}
