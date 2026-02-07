using System.Drawing;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface IOcrPipeline
    {
        Task<string[]> RecognizeImagesAsync(Bitmap[] bitmaps, CancellationToken cancellationToken = default);
    }
}
