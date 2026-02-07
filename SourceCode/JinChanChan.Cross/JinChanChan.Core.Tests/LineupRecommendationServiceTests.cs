using JinChanChan.Core.Models;
using JinChanChan.Core.Services;

namespace JinChanChan.Core.Tests;

public class LineupRecommendationServiceTests
{
    [Fact]
    public void Recommend_ShouldPreferMatchedLineup()
    {
        LineupRecommendationService service = new(new LineupMatcherService());
        LiveGameState state = new()
        {
            ShopCards = ["德莱文", "蕾欧娜", "锤石"],
            PreferredTargets = ["德莱文"]
        };

        LineupRecommendation recommendation = service.Recommend(state);

        Assert.Equal("重装德莱文", recommendation.LineupName);
        Assert.True(recommendation.MatchScore.TotalScore > 0);
        Assert.Contains("德莱文", recommendation.MatchedHeroes);
    }

    [Fact]
    public void Search_ShouldSupportHeroAndTraitKeyword()
    {
        LineupRecommendationService service = new(new LineupMatcherService());
        LiveGameState state = new()
        {
            ShopCards = ["阿狸", "安妮"]
        };

        IReadOnlyList<LineupRecommendation> byHero = service.Search(state, "阿狸", 3);
        IReadOnlyList<LineupRecommendation> byTrait = service.Search(state, "法师", 3);
        IReadOnlyList<LineupRecommendation> byInitials = service.Search(state, "fzjw", 3);

        Assert.NotEmpty(byHero);
        Assert.NotEmpty(byTrait);
        Assert.NotEmpty(byInitials);
        Assert.Contains(byHero, x => x.LineupName == "法转九五");
        Assert.Contains(byTrait, x => x.LineupName == "法转九五");
        Assert.Contains(byInitials, x => x.LineupName == "法转九五");
    }
}
