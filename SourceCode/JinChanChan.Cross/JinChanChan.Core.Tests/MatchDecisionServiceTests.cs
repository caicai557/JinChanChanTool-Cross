using JinChanChan.Core.Services;

namespace JinChanChan.Core.Tests;

public class MatchDecisionServiceTests
{
    [Fact]
    public void StrictMatching_ShouldOnlyMatchExactText()
    {
        MatchDecisionService service = new();
        string[] results = ["艾克", "测试阿狸", "", "德莱文", "xxx"];
        string[] targets = ["艾克", "阿狸"];

        bool[] matched = service.MatchTargets(results, targets, strictMatching: true);

        Assert.Equal([true, false, false, false, false], matched);
    }

    [Fact]
    public void NonStrictMatching_ShouldMatchContainsAndRewrite()
    {
        MatchDecisionService service = new();
        string[] results = ["测试阿狸", "艾克123", "", "", ""];
        string[] targets = ["艾克", "阿狸"];

        bool[] matched = service.MatchTargets(results, targets, strictMatching: false);

        Assert.Equal([true, true, false, false, false], matched);
        Assert.Equal("阿狸", results[0]);
        Assert.Equal("艾克", results[1]);
    }
}
