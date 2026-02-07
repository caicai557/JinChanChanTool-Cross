using JinChanChanTool.Tools;
using System.Diagnostics;
using System.Drawing;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class FrameCaptureService : IFrameCaptureService
    {
        public LoopFrame Capture(Rectangle[] nameRectangles, Rectangle[] cardRectangles)
        {
            if (nameRectangles == null || nameRectangles.Length != 5)
            {
                throw new ArgumentException("nameRectangles must contain exactly 5 entries.", nameof(nameRectangles));
            }

            try
            {
                int minX = nameRectangles.Min(r => r.X);
                int minY = nameRectangles.Min(r => r.Y);
                int maxX = nameRectangles.Max(r => r.X + r.Width);
                int maxY = nameRectangles.Max(r => r.Y + r.Height);

                Rectangle boundingBox = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                using Bitmap bigImage = ImageProcessingTool.AreaScreenshots(boundingBox);

                Bitmap[] bitmaps = new Bitmap[5];
                for (int i = 0; i < 5; i++)
                {
                    int offsetX = nameRectangles[i].X - minX;
                    int offsetY = nameRectangles[i].Y - minY;
                    bitmaps[i] = ImageProcessingTool.CropBitmap(
                        bigImage,
                        offsetX,
                        offsetY,
                        nameRectangles[i].Width,
                        nameRectangles[i].Height);
                }

                return new LoopFrame
                {
                    Bitmaps = bitmaps,
                    CardRectangles = cardRectangles
                };
            }
            catch (Exception ex)
            {
                string errorMessage = $"FrameCaptureService 截图失败: {ex.Message}";
                LogTool.Log(errorMessage);
                Debug.WriteLine(errorMessage);
                Bitmap[] errorBitmaps = new Bitmap[5];
                for (int i = 0; i < 5; i++)
                {
                    errorBitmaps[i] = new Bitmap(10, 10);
                }

                return new LoopFrame
                {
                    Bitmaps = errorBitmaps,
                    CardRectangles = cardRectangles
                };
            }
        }
    }
}
