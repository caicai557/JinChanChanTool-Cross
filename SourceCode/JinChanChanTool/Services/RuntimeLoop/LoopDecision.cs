namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class LoopDecision
    {
        public bool HasTargetCards { get; init; }

        public bool ShouldRefreshStore { get; init; }

        public bool IsStoreEmpty { get; init; }
    }
}
