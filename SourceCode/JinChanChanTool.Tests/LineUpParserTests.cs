using JinChanChanTool.Tools.LineUpCodeTools;

namespace JinChanChanTool.Tests;

public class LineUpParserTests
{
    [Fact]
    public void GenerateCode_ShouldContainSetSuffix()
    {
        string code = LineUpParser.GenerateCode(["艾尼维亚", "布里茨", "阿狸"]);

        Assert.StartsWith("02", code);
        Assert.EndsWith("TFTSet16", code);
    }

    [Fact]
    public void ParseCode_ShouldRecoverHeroesFromGeneratedCode()
    {
        List<string> original = ["艾尼维亚", "布里茨", "阿狸"];
        string code = LineUpParser.GenerateCode(original);

        List<string> parsed = LineUpParser.ParseCode(code);

        Assert.Contains("艾尼维亚", parsed);
        Assert.Contains("布里茨", parsed);
        Assert.Contains("阿狸", parsed);
    }
}
