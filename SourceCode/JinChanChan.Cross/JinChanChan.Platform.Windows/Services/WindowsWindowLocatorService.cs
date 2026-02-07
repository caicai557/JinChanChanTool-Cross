using System.Diagnostics;
using System.Text;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Windows.Services;

public sealed class WindowsWindowLocatorService : IWindowLocatorService
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public Task<IReadOnlyList<WindowDescriptor>> ListWindowsAsync(CancellationToken cancellationToken = default)
    {
        List<WindowDescriptor> windows = new();

        EnumWindows((hWnd, _) =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsWindowVisible(hWnd))
            {
                return true;
            }

            int textLength = GetWindowTextLength(hWnd);
            if (textLength <= 0)
            {
                return true;
            }

            StringBuilder sb = new(textLength + 1);
            _ = GetWindowText(hWnd, sb, sb.Capacity);

            if (!GetWindowRect(hWnd, out RECT rect))
            {
                return true;
            }

            if (rect.Right <= rect.Left || rect.Bottom <= rect.Top)
            {
                return true;
            }

            GetWindowThreadProcessId(hWnd, out uint pid);
            string processName = string.Empty;
            try
            {
                processName = Process.GetProcessById((int)pid).ProcessName;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"读取窗口进程名失败 PID={pid}: {ex.Message}");
            }

            windows.Add(new WindowDescriptor
            {
                Id = hWnd.ToInt64(),
                Title = sb.ToString(),
                ProcessName = processName,
                Bounds = new ScreenRect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top),
                IsVisible = true
            });

            return true;
        }, IntPtr.Zero);

        return Task.FromResult<IReadOnlyList<WindowDescriptor>>(windows.OrderByDescending(w => w.Bounds.Width * w.Bounds.Height).ToList());
    }

    public async Task<WindowDescriptor?> FindBestGameWindowAsync(string processName, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WindowDescriptor> windows = await ListWindowsAsync(cancellationToken);

        return windows
            .Where(w => w.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)
                        || w.Title.Contains(processName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(w => w.Bounds.Width * w.Bounds.Height)
            .FirstOrDefault();
    }
}
