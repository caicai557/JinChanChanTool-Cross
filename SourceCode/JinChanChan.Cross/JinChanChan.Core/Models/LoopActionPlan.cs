namespace JinChanChan.Core.Models;

public sealed class LoopActionPlan
{
    public required IReadOnlyList<int> PurchaseSlotIndexes { get; init; }

    public bool ShouldRefreshStore { get; init; }

    public string Reason { get; init; } = string.Empty;
}
