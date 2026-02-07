using JinChanChanTool.DataClass;
using JinChanChanTool.Services.RuntimeLoop;

namespace JinChanChanTool.Tests;

public class MatchDecisionServiceTests
{
    [Fact]
    public void StrictMatching_ShouldMatchOnlyExactName()
    {
        MatchDecisionService service = new MatchDecisionService();
        string[] results = ["艾克", "艾克123", "", "阿狸", "测试"];
        List<LineUp.LineUpUnit> units =
        [
            new LineUp.LineUpUnit { HeroName = "艾克" },
            new LineUp.LineUpUnit { HeroName = "阿狸" }
        ];

        bool[] matched = service.MatchTargets(results, units, strictMatching: true);

        Assert.Equal([true, false, false, true, false], matched);
    }

    [Fact]
    public void NonStrictMatching_ShouldAllowContainsMatchAndRewriteResult()
    {
        MatchDecisionService service = new MatchDecisionService();
        string[] results = ["艾克123", "测试阿狸", "", "", ""];
        List<LineUp.LineUpUnit> units =
        [
            new LineUp.LineUpUnit { HeroName = "艾克" },
            new LineUp.LineUpUnit { HeroName = "阿狸" }
        ];

        bool[] matched = service.MatchTargets(results, units, strictMatching: false);

        Assert.Equal([true, true, false, false, false], matched);
        Assert.Equal("艾克", results[0]);
        Assert.Equal("阿狸", results[1]);
    }
}
