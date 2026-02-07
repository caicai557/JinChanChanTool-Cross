namespace JinChanChan.Core.Models;

public sealed class LoopActionPlan
{
    public required IReadOnlyList<int> PurchaseSlotIndexes { get; init; }

    public bool ShouldRefreshStore { get; init; }

    public string Reason { get; init; } = string.Empty;

    public IReadOnlyList<string> RecognizedCards { get; init; } = Array.Empty<string>();

    public IReadOnlyList<bool> MatchedSlots { get; init; } = Array.Empty<bool>();
}
