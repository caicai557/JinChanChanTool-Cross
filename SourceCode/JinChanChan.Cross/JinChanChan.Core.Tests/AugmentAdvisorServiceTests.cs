using JinChanChan.Core.Models;
using JinChanChan.Core.Services;

namespace JinChanChan.Core.Tests;

public class AugmentAdvisorServiceTests
{
    [Fact]
    public void BuildSuggestion_ShouldUseRecommendationAugments()
    {
        AugmentAdvisorService service = new();
        LiveGameState state = new();
        LineupRecommendation recommendation = new()
        {
            LineupName = "法转九五",
            RecommendedAugments = ["法杖工坊", "学习拼图", "升级咯"]
        };

        AugmentSuggestion suggestion = service.BuildSuggestion(state, recommendation);

        Assert.Equal(["法杖工坊", "学习拼图"], suggestion.PrimaryChoices);
        Assert.Contains("升级咯", suggestion.BackupChoices);
    }

    [Fact]
    public void BuildSuggestion_ShouldFallbackWhenRecommendationMissing()
    {
        AugmentAdvisorService service = new();

        AugmentSuggestion suggestion = service.BuildSuggestion(new LiveGameState(), null);

        Assert.Contains("成吨利息", suggestion.PrimaryChoices);
        Assert.NotEmpty(suggestion.BackupChoices);
    }
}
