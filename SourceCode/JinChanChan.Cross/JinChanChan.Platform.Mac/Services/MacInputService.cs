using JinChanChan.Core.Abstractions;

namespace JinChanChan.Platform.Mac.Services;

public sealed class MacInputService : IInputService
{
    private const int KCGHIDEventTap = 0;
    private const int KCGEventMouseMoved = 5;
    private const int KCGEventLeftMouseDown = 1;
    private const int KCGEventLeftMouseUp = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateMouseEvent(IntPtr source, int mouseType, CGPoint mouseCursorPosition, int mouseButton);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, ushort virtualKey, bool keyDown);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CGEventPost(int tap, IntPtr @event);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cfTypeRef);

    private readonly object _sync = new();
    private CGPoint _lastPoint;

    public Task MoveMouseAsync(int x, int y, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CGPoint point = new() { X = x, Y = y };
        lock (_sync)
        {
            _lastPoint = point;
        }

        IntPtr evt = CGEventCreateMouseEvent(IntPtr.Zero, KCGEventMouseMoved, point, 0);
        if (evt != IntPtr.Zero)
        {
            CGEventPost(KCGHIDEventTap, evt);
            CFRelease(evt);
        }

        return Task.CompletedTask;
    }

    public Task LeftClickAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CGPoint point;
        lock (_sync)
        {
            point = _lastPoint;
        }

        IntPtr down = CGEventCreateMouseEvent(IntPtr.Zero, KCGEventLeftMouseDown, point, 0);
        if (down != IntPtr.Zero)
        {
            CGEventPost(KCGHIDEventTap, down);
            CFRelease(down);
        }

        IntPtr up = CGEventCreateMouseEvent(IntPtr.Zero, KCGEventLeftMouseUp, point, 0);
        if (up != IntPtr.Zero)
        {
            CGEventPost(KCGHIDEventTap, up);
            CFRelease(up);
        }

        return Task.CompletedTask;
    }

    public Task PressKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!MacKeyCodeMapper.TryMap(key, out ushort keyCode))
        {
            return Task.CompletedTask;
        }

        IntPtr down = CGEventCreateKeyboardEvent(IntPtr.Zero, keyCode, true);
        if (down != IntPtr.Zero)
        {
            CGEventPost(KCGHIDEventTap, down);
            CFRelease(down);
        }

        IntPtr up = CGEventCreateKeyboardEvent(IntPtr.Zero, keyCode, false);
        if (up != IntPtr.Zero)
        {
            CGEventPost(KCGHIDEventTap, up);
            CFRelease(up);
        }

        return Task.CompletedTask;
    }
}
