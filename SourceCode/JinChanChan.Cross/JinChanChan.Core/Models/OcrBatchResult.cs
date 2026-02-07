namespace JinChanChan.Core.Models;

public sealed class OcrBatchResult
{
    public required IReadOnlyList<string> RawTexts { get; init; }

    public required string Provider { get; init; }

    public TimeSpan Elapsed { get; init; }

    public bool UsedFallbackProvider { get; init; }

    public string Note { get; init; } = string.Empty;
}
