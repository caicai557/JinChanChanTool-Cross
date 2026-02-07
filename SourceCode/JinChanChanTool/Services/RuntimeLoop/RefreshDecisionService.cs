namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class RefreshDecisionService : IRefreshDecisionService
    {
        public LoopDecision Decide(bool refreshEnabled, bool isMousePressed, bool hasTargetCards, bool isStoreEmpty, bool isRefreshInProgress)
        {
            bool shouldRefresh = refreshEnabled
                && !isMousePressed
                && !hasTargetCards
                && !isStoreEmpty
                && !isRefreshInProgress;

            return new LoopDecision
            {
                HasTargetCards = hasTargetCards,
                IsStoreEmpty = isStoreEmpty,
                ShouldRefreshStore = shouldRefresh
            };
        }
    }
}
