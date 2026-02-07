using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Mac.Services;

public sealed class MacCaptureService : ICaptureService
{
    private const int KCGWindowListOptionOnScreenOnly = 1;
    private const int KCGNullWindowID = 0;
    private const int KCGWindowImageDefault = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double Width;
        public double Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public CGPoint Origin;
        public CGSize Size;
    }

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, int listOption, int windowId, int imageOption);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cfTypeRef);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern nuint CGImageGetWidth(IntPtr image);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern nuint CGImageGetHeight(IntPtr image);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern nuint CGImageGetBytesPerRow(IntPtr image);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGImageGetDataProvider(IntPtr image);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGDataProviderCopyData(IntPtr provider);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern nint CFDataGetLength(IntPtr data);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFDataGetBytePtr(IntPtr data);

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
            return EmptyFrame();
        }

        CGRect rect = new()
        {
            Origin = new CGPoint { X = region.X, Y = region.Y },
            Size = new CGSize { Width = region.Width, Height = region.Height }
        };

        IntPtr image = CGWindowListCreateImage(rect, KCGWindowListOptionOnScreenOnly, KCGNullWindowID, KCGWindowImageDefault);
        if (image == IntPtr.Zero)
        {
            return EmptyFrame();
        }

        try
        {
            int width = checked((int)CGImageGetWidth(image));
            int height = checked((int)CGImageGetHeight(image));
            int bytesPerRow = checked((int)CGImageGetBytesPerRow(image));
            if (width <= 0 || height <= 0 || bytesPerRow <= 0)
            {
                return EmptyFrame();
            }

            IntPtr provider = CGImageGetDataProvider(image);
            if (provider == IntPtr.Zero)
            {
                return EmptyFrame();
            }

            IntPtr data = CGDataProviderCopyData(provider);
            if (data == IntPtr.Zero)
            {
                return EmptyFrame();
            }

            try
            {
                int length = checked((int)CFDataGetLength(data));
                IntPtr sourcePtr = CFDataGetBytePtr(data);
                if (length <= 0 || sourcePtr == IntPtr.Zero)
                {
                    return EmptyFrame();
                }

                byte[] source = new byte[length];
                Marshal.Copy(sourcePtr, source, 0, length);

                int packedRow = width * 4;
                byte[] packed = new byte[Math.Max(0, packedRow * height)];
                if (packed.Length == 0)
                {
                    return EmptyFrame();
                }

                int copyRow = Math.Min(bytesPerRow, packedRow);
                for (int y = 0; y < height; y++)
                {
                    int srcOffset = y * bytesPerRow;
                    int dstOffset = y * packedRow;
                    if (srcOffset + copyRow > source.Length || dstOffset + copyRow > packed.Length)
                    {
                        break;
                    }

                    Buffer.BlockCopy(source, srcOffset, packed, dstOffset, copyRow);
                }

                return new FrameImage
                {
                    Pixels = packed,
                    Width = width,
                    Height = height,
                    Channels = 4,
                    Source = "mac"
                };
            }
            finally
            {
                CFRelease(data);
            }
        }
        finally
        {
            CFRelease(image);
        }
    }

    private static FrameImage EmptyFrame()
    {
        return new FrameImage
        {
            Pixels = Array.Empty<byte>(),
            Width = 0,
            Height = 0,
            Channels = 4,
            Source = "mac"
        };
    }
}
