using JinChanChan.Core.Services;

namespace JinChanChan.Core.Tests;

public class RefreshDecisionServiceTests
{
    [Fact]
    public void ShouldRefresh_WhenAllConditionsSatisfied()
    {
        RefreshDecisionService service = new();

        bool result = service.ShouldRefresh(
            refreshEnabled: true,
            isMousePressed: false,
            hasTargetCards: false,
            isStoreEmpty: false,
            isRefreshInProgress: false);

        Assert.True(result);
    }

    [Fact]
    public void ShouldNotRefresh_WhenStoreEmpty()
    {
        RefreshDecisionService service = new();

        bool result = service.ShouldRefresh(
            refreshEnabled: true,
            isMousePressed: false,
            hasTargetCards: false,
            isStoreEmpty: true,
            isRefreshInProgress: false);

        Assert.False(result);
    }
}
