using System.Drawing;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class OcrPipeline : IOcrPipeline
    {
        private readonly QueuedOCRService _ocrService;

        public OcrPipeline(QueuedOCRService ocrService)
        {
            _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        }

        public async Task<string[]> RecognizeImagesAsync(Bitmap[] bitmaps, CancellationToken cancellationToken = default)
        {
            if (bitmaps == null || bitmaps.Length == 0)
            {
                return Array.Empty<string>();
            }

            Task<string>[] tasks = new Task<string>[bitmaps.Length];
            for (int i = 0; i < bitmaps.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                tasks[i] = _ocrService.RecognizeTextAsync(bitmaps[i]);
            }

            return await Task.WhenAll(tasks);
        }
    }
}
