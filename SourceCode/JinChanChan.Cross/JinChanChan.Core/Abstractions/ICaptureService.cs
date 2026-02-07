using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface ICaptureService
{
    Task<FrameImage> CaptureAsync(ScreenRect region, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FrameImage>> CaptureBatchAsync(IReadOnlyList<ScreenRect> regions, CancellationToken cancellationToken = default);
}
