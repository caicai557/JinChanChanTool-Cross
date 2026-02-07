using System.Diagnostics;
using JinChanChan.Core.Abstractions;

namespace JinChanChan.Platform.Windows.Services;

public sealed class WindowsHotkeyService : IHotkeyService
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public uint VkCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public nint DwExtraInfo;
    }

    private delegate nint HookProc(int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    private readonly object _sync = new();
    private readonly Dictionary<int, (Action Pressed, Action? Released)> _handlers = new();
    private readonly HashSet<int> _pressedKeys = new();
    private readonly HookProc _procDelegate;
    private nint _hookId;

    public WindowsHotkeyService()
    {
        _procDelegate = HookCallback;
    }

    public void Register(string key, Action onPressed, Action? onReleased = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("热键不能为空。", nameof(key));
        }

        if (onPressed == null)
        {
            throw new ArgumentNullException(nameof(onPressed));
        }

        if (!TryParseKeyCode(key, out int keyCode))
        {
            throw new ArgumentException($"无效热键: {key}", nameof(key));
        }

        lock (_sync)
        {
            _handlers[keyCode] = (onPressed, onReleased);
            EnsureHookInstalled();
        }
    }

    public void Unregister(string key)
    {
        if (!TryParseKeyCode(key, out int keyCode))
        {
            return;
        }

        lock (_sync)
        {
            _handlers.Remove(keyCode);
            _pressedKeys.Remove(keyCode);
            TryUninstallHookIfUnused();
        }
    }

    public void UnregisterAll()
    {
        lock (_sync)
        {
            _handlers.Clear();
            _pressedKeys.Clear();
            TryUninstallHookIfUnused();
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _handlers.Clear();
            _pressedKeys.Clear();
            if (_hookId != 0)
            {
                if (!UnhookWindowsHookEx(_hookId))
                {
                    int error = Marshal.GetLastWin32Error();
                    Trace.WriteLine($"卸载热键钩子失败，Win32Error=0x{error:X8}");
                }

                _hookId = 0;
            }
        }
    }

    private void EnsureHookInstalled()
    {
        if (_hookId != 0)
        {
            return;
        }

        _hookId = SetWindowsHookEx(WhKeyboardLl, _procDelegate, 0, 0);
        if (_hookId == 0)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"安装全局热键钩子失败，Win32Error=0x{error:X8}");
        }
    }

    private void TryUninstallHookIfUnused()
    {
        if (_handlers.Count > 0 || _hookId == 0)
        {
            return;
        }

        if (!UnhookWindowsHookEx(_hookId))
        {
            int error = Marshal.GetLastWin32Error();
            Trace.WriteLine($"卸载全局热键钩子失败，Win32Error=0x{error:X8}");
        }

        _hookId = 0;
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0 || lParam == 0)
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        KbdLlHookStruct data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
        int keyCode = checked((int)data.VkCode);
        int msg = checked((int)wParam);

        lock (_sync)
        {
            if (!_handlers.TryGetValue(keyCode, out (Action Pressed, Action? Released) handler))
            {
                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            switch (msg)
            {
                case WmKeyDown:
                case WmSysKeyDown:
                    bool alreadyPressed = _pressedKeys.Contains(keyCode);
                    if (!alreadyPressed || handler.Released == null)
                    {
                        QueueInvoke(handler.Pressed);
                    }

                    if (handler.Released != null)
                    {
                        _pressedKeys.Add(keyCode);
                    }
                    return (nint)1;

                case WmKeyUp:
                case WmSysKeyUp:
                    _pressedKeys.Remove(keyCode);
                    if (handler.Released != null)
                    {
                        QueueInvoke(handler.Released);
                        return (nint)1;
                    }
                    break;
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static void QueueInvoke(Action action)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"热键回调执行失败: {ex.Message}");
            }
        });
    }

    private static bool TryParseKeyCode(string key, out int keyCode)
    {
        keyCode = 0;
        if (!Enum.TryParse(key, ignoreCase: true, out System.Windows.Forms.Keys parsed))
        {
            return false;
        }

        if (parsed == System.Windows.Forms.Keys.None)
        {
            return false;
        }

        keyCode = checked((int)parsed);
        return true;
    }
}
