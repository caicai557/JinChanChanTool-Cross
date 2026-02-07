using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IOcrEngine
{
    Task<OcrBatchResult> RecognizeAsync(IReadOnlyList<FrameImage> frames, CancellationToken cancellationToken = default);
}
