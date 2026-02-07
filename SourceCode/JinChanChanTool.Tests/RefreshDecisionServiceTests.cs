using JinChanChanTool.Services.RuntimeLoop;

namespace JinChanChanTool.Tests;

public class RefreshDecisionServiceTests
{
    [Fact]
    public void ShouldRefresh_WhenAllConditionsSatisfied()
    {
        RefreshDecisionService service = new RefreshDecisionService();
        LoopDecision decision = service.Decide(
            refreshEnabled: true,
            isMousePressed: false,
            hasTargetCards: false,
            isStoreEmpty: false,
            isRefreshInProgress: false);

        Assert.True(decision.ShouldRefreshStore);
    }

    [Fact]
    public void ShouldNotRefresh_WhenStoreIsEmpty()
    {
        RefreshDecisionService service = new RefreshDecisionService();
        LoopDecision decision = service.Decide(
            refreshEnabled: true,
            isMousePressed: false,
            hasTargetCards: false,
            isStoreEmpty: true,
            isRefreshInProgress: false);

        Assert.False(decision.ShouldRefreshStore);
        Assert.True(decision.IsStoreEmpty);
    }
}
