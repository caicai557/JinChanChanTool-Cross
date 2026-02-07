using JinChanChan.Core.Abstractions;

namespace JinChanChan.Platform.Windows.Services;

public sealed class WindowsInputService : IInputService
{
    private const int MouseLeftDown = 0x0002;
    private const int MouseLeftUp = 0x0004;
    private const int KeyEventfKeyUp = 0x0002;

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int flags, int dx, int dy, int buttons, int extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte vk, byte scan, int flags, int extraInfo);

    public Task MoveMouseAsync(int x, int y, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SetCursorPos(x, y);
        return Task.CompletedTask;
    }

    public Task LeftClickAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        mouse_event(MouseLeftDown, 0, 0, 0, 0);
        mouse_event(MouseLeftUp, 0, 0, 0, 0);
        return Task.CompletedTask;
    }

    public Task PressKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Enum.TryParse(key, true, out System.Windows.Forms.Keys parsed))
        {
            byte vk = (byte)parsed;
            keybd_event(vk, 0, 0, 0);
            keybd_event(vk, 0, KeyEventfKeyUp, 0);
        }

        return Task.CompletedTask;
    }
}
