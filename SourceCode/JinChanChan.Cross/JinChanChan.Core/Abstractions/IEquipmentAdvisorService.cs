using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IEquipmentAdvisorService
{
    CarryEquipmentPlan BuildPlan(
        LiveGameState gameState,
        LineupRecommendation? recommendation);
}
