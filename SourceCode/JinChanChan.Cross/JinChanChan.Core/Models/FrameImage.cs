namespace JinChanChan.Core.Models;

public sealed class FrameImage
{
    public required byte[] Pixels { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public int Channels { get; init; } = 4;

    public DateTimeOffset CapturedAt { get; init; } = DateTimeOffset.UtcNow;

    public string Source { get; init; } = "screen";
}
