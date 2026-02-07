namespace JinChanChan.Core.Models;

public sealed class CarouselSuggestion
{
    public IReadOnlyList<string> Priorities { get; init; } = Array.Empty<string>();

    public string Reason { get; init; } = string.Empty;
}
