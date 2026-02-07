namespace JinChanChan.Core.Models;

public sealed class AugmentSuggestion
{
    public IReadOnlyList<string> PrimaryChoices { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> BackupChoices { get; init; } = Array.Empty<string>();

    public string Reason { get; init; } = string.Empty;
}
