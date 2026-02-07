namespace JinChanChan.Core.Abstractions;

public interface IRefreshDecisionService
{
    bool ShouldRefresh(bool refreshEnabled, bool isMousePressed, bool hasTargetCards, bool isStoreEmpty, bool isRefreshInProgress);
}
