namespace JinChanChan.Core.Models;

public sealed class WindowDescriptor
{
    public required long Id { get; init; }

    public required string Title { get; init; }

    public string ProcessName { get; init; } = string.Empty;

    public required ScreenRect Bounds { get; init; }

    public bool IsVisible { get; init; } = true;
}
