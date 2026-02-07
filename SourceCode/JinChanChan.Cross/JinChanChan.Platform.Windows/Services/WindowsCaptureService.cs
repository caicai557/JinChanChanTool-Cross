using System.Drawing;
using System.Drawing.Imaging;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Windows.Services;

public sealed class WindowsCaptureService : ICaptureService
{
    public Task<FrameImage> CaptureAsync(ScreenRect region, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CaptureInternal(region));
    }

    public Task<IReadOnlyList<FrameImage>> CaptureBatchAsync(IReadOnlyList<ScreenRect> regions, CancellationToken cancellationToken = default)
    {
        List<FrameImage> frames = new(regions.Count);
        for (int i = 0; i < regions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            frames.Add(CaptureInternal(regions[i]));
        }

        return Task.FromResult<IReadOnlyList<FrameImage>>(frames);
    }

    private static FrameImage CaptureInternal(ScreenRect region)
    {
        if (region.IsEmpty)
        {
            return new FrameImage
            {
                Pixels = Array.Empty<byte>(),
                Width = 0,
                Height = 0,
                Channels = 4,
                Source = "windows"
            };
        }

        using Bitmap bitmap = new(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(region.X, region.Y, 0, 0, new Size(region.Width, region.Height), CopyPixelOperation.SourceCopy);

        Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
        try
        {
            int byteCount = Math.Abs(data.Stride) * bitmap.Height;
            byte[] bytes = new byte[byteCount];
            Marshal.Copy(data.Scan0, bytes, 0, byteCount);

            return new FrameImage
            {
                Pixels = bytes,
                Width = bitmap.Width,
                Height = bitmap.Height,
                Channels = 4,
                Source = "windows"
            };
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }
}
