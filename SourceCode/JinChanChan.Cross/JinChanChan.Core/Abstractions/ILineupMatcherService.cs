using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface ILineupMatcherService
{
    LineupMatchScore CalculateScore(
        LiveGameState gameState,
        IReadOnlyList<string> lineupHeroes,
        IReadOnlyList<string> lineupTraits,
        IReadOnlyList<string> keyUnits);
}
