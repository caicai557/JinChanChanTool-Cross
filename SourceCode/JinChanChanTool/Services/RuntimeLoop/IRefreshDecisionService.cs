namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface IRefreshDecisionService
    {
        LoopDecision Decide(bool refreshEnabled, bool isMousePressed, bool hasTargetCards, bool isStoreEmpty, bool isRefreshInProgress);
    }
}
