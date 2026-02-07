using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class StrategyAdvisorService
{
    private readonly ILineupRecommendationService _lineupRecommendationService;
    private readonly IBenchAdvisorService _benchAdvisorService;
    private readonly ICarouselAdvisorService _carouselAdvisorService;
    private readonly IEquipmentAdvisorService _equipmentAdvisorService;
    private readonly IAugmentAdvisorService _augmentAdvisorService;

    public StrategyAdvisorService(
        ILineupRecommendationService lineupRecommendationService,
        IBenchAdvisorService benchAdvisorService,
        ICarouselAdvisorService carouselAdvisorService,
        IEquipmentAdvisorService equipmentAdvisorService,
        IAugmentAdvisorService augmentAdvisorService)
    {
        _lineupRecommendationService = lineupRecommendationService;
        _benchAdvisorService = benchAdvisorService;
        _carouselAdvisorService = carouselAdvisorService;
        _equipmentAdvisorService = equipmentAdvisorService;
        _augmentAdvisorService = augmentAdvisorService;
    }

    public AdvisorSnapshot BuildSnapshot(
        LiveGameState gameState,
        bool enableBenchSellHint,
        bool enableCarouselHint,
        bool enableAugmentHint,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        LineupRecommendation recommendation = _lineupRecommendationService.Recommend(gameState, query, cancellationToken);

        IReadOnlyList<BenchSellSuggestion> benchSuggestions = enableBenchSellHint
            ? _benchAdvisorService.BuildSuggestions(gameState, recommendation)
            : Array.Empty<BenchSellSuggestion>();

        CarouselSuggestion carouselSuggestion = enableCarouselHint
            ? _carouselAdvisorService.BuildSuggestion(gameState, recommendation)
            : new CarouselSuggestion();

        CarryEquipmentPlan equipmentPlan = _equipmentAdvisorService.BuildPlan(gameState, recommendation);

        AugmentSuggestion augmentSuggestion = enableAugmentHint
            ? _augmentAdvisorService.BuildSuggestion(gameState, recommendation)
            : new AugmentSuggestion();

        return new AdvisorSnapshot
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            GameState = gameState,
            Recommendation = recommendation,
            BenchSellSuggestions = benchSuggestions,
            CarouselSuggestion = carouselSuggestion,
            CarryEquipmentPlan = equipmentPlan,
            AugmentSuggestion = augmentSuggestion,
            Summary = BuildSummary(recommendation, benchSuggestions, equipmentPlan)
        };
    }

    private static string BuildSummary(
        LineupRecommendation recommendation,
        IReadOnlyList<BenchSellSuggestion> benchSuggestions,
        CarryEquipmentPlan equipmentPlan)
    {
        return $"阵容:{recommendation.LineupName} | 匹配:{recommendation.MatchScore.TotalScore:P0} | 卖牌建议:{benchSuggestions.Count} | 装备进度:{equipmentPlan.CompletionRate:P0}";
    }
}
