using JinChanChan.Core.Services;

namespace JinChanChan.Core.Tests;

public class GameStateReaderTests
{
    [Fact]
    public void Read_ShouldKeepBenchCardsAndStage()
    {
        GameStateReader reader = new();
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;

        var state = reader.Read(
            ["德莱文", "蕾欧娜", "锤石"],
            ["德莱文", "锤石"],
            autoPickEnabled: true,
            autoRefreshEnabled: false,
            timestamp,
            benchCards: ["锤石", "亚索", " "],
            stage: "roll");

        Assert.Equal(3, state.ShopCards.Count);
        Assert.Equal(["锤石", "亚索"], state.BenchCards);
        Assert.Equal("roll", state.Stage);
    }
}
