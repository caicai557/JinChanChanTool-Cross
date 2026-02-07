using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface ILineupRecommendationService
{
    LineupRecommendation Recommend(
        LiveGameState gameState,
        string? query = null,
        CancellationToken cancellationToken = default);

    IReadOnlyList<LineupRecommendation> Search(
        LiveGameState gameState,
        string query,
        int take = 5,
        CancellationToken cancellationToken = default);
}
