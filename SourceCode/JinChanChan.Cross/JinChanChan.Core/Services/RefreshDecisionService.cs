using JinChanChan.Core.Abstractions;

namespace JinChanChan.Core.Services;

public sealed class RefreshDecisionService : IRefreshDecisionService
{
    public bool ShouldRefresh(bool refreshEnabled, bool isMousePressed, bool hasTargetCards, bool isStoreEmpty, bool isRefreshInProgress)
    {
        return refreshEnabled
               && !isMousePressed
               && !hasTargetCards
               && !isStoreEmpty
               && !isRefreshInProgress;
    }
}
