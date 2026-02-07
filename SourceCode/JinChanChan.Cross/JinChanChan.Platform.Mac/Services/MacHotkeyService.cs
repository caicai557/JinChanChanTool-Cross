using System.Diagnostics;
using JinChanChan.Core.Abstractions;

namespace JinChanChan.Platform.Mac.Services;

public sealed class MacHotkeyService : IHotkeyService
{
    private const string CarbonFramework = "/System/Library/Frameworks/Carbon.framework/Carbon";
    private const uint EventClassKeyboard = 0x6B657962;   // 'keyb'
    private const uint EventHotKeyPressed = 6;
    private const uint EventHotKeyReleased = 9;
    private const uint EventParamDirectObject = 0x2D2D2D2D; // '----'
    private const uint TypeEventHotKeyId = 0x686B6964; // 'hkid'
    private const uint Signature = 0x4A434354; // 'JCCT'
    private const int NoErr = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct EventTypeSpec
    {
        public uint EventClass;
        public uint EventKind;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct EventHotKeyId
    {
        public uint Signature;
        public uint Id;
    }

    [DllImport(CarbonFramework)]
    private static extern IntPtr GetApplicationEventTarget();

    [DllImport(CarbonFramework)]
    private static extern int InstallEventHandler(
        IntPtr inTarget,
        EventHandlerProcPtr inHandler,
        uint inNumTypes,
        EventTypeSpec[] inList,
        IntPtr inUserData,
        out IntPtr outRef);

    [DllImport(CarbonFramework)]
    private static extern int RemoveEventHandler(IntPtr inHandlerRef);

    [DllImport(CarbonFramework)]
    private static extern int RegisterEventHotKey(
        uint inHotKeyCode,
        uint inHotKeyModifiers,
        EventHotKeyId inHotKeyID,
        IntPtr inTarget,
        uint inOptions,
        out IntPtr outRef);

    [DllImport(CarbonFramework)]
    private static extern int UnregisterEventHotKey(IntPtr inHotKeyRef);

    [DllImport(CarbonFramework)]
    private static extern int GetEventParameter(
        IntPtr inEvent,
        uint inName,
        uint inDesiredType,
        IntPtr outActualType,
        uint inBufferSize,
        out uint outActualSize,
        out EventHotKeyId outData);

    [DllImport(CarbonFramework)]
    private static extern uint GetEventKind(IntPtr inEvent);

    private delegate int EventHandlerProcPtr(IntPtr inHandlerCallRef, IntPtr inEvent, IntPtr inUserData);

    private readonly object _sync = new();
    private readonly Dictionary<string, uint> _keyToId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<uint, (Action Pressed, Action? Released, IntPtr Ref)> _handlersById = new();
    private readonly EventHandlerProcPtr _eventHandlerProc;
    private IntPtr _eventHandlerRef;
    private uint _nextId = 1;

    public MacHotkeyService()
    {
        _eventHandlerProc = HandleCarbonHotKeyEvent;
        EnsureEventHandlerInstalled();
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

        if (!MacKeyCodeMapper.TryMap(key, out ushort keyCode))
        {
            throw new ArgumentException($"无效热键: {key}", nameof(key));
        }

        lock (_sync)
        {
            if (_keyToId.TryGetValue(key, out uint existingId))
            {
                UnregisterById(existingId, key);
            }

            uint id = _nextId++;
            EventHotKeyId hotKeyId = new()
            {
                Signature = Signature,
                Id = id
            };

            int status = RegisterEventHotKey(keyCode, 0, hotKeyId, GetApplicationEventTarget(), 0, out IntPtr hotKeyRef);
            if (status != NoErr)
            {
                throw new InvalidOperationException($"注册mac全局热键失败: key={key}, status={status}");
            }

            _keyToId[key] = id;
            _handlersById[id] = (onPressed, onReleased, hotKeyRef);
        }
    }

    public void Unregister(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        lock (_sync)
        {
            if (!_keyToId.TryGetValue(key, out uint id))
            {
                return;
            }

            UnregisterById(id, key);
        }
    }

    public void UnregisterAll()
    {
        lock (_sync)
        {
            foreach (KeyValuePair<string, uint> pair in _keyToId.ToArray())
            {
                UnregisterById(pair.Value, pair.Key);
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            UnregisterAll();
            if (_eventHandlerRef != IntPtr.Zero)
            {
                int status = RemoveEventHandler(_eventHandlerRef);
                if (status != NoErr)
                {
                    Trace.WriteLine($"移除mac热键事件处理器失败: status={status}");
                }

                _eventHandlerRef = IntPtr.Zero;
            }
        }
    }

    private void EnsureEventHandlerInstalled()
    {
        if (_eventHandlerRef != IntPtr.Zero)
        {
            return;
        }

        EventTypeSpec[] events =
        [
            new EventTypeSpec { EventClass = EventClassKeyboard, EventKind = EventHotKeyPressed },
            new EventTypeSpec { EventClass = EventClassKeyboard, EventKind = EventHotKeyReleased }
        ];

        int status = InstallEventHandler(
            GetApplicationEventTarget(),
            _eventHandlerProc,
            (uint)events.Length,
            events,
            IntPtr.Zero,
            out _eventHandlerRef);

        if (status != NoErr)
        {
            throw new InvalidOperationException($"安装mac热键事件处理器失败: status={status}");
        }
    }

    private int HandleCarbonHotKeyEvent(IntPtr nextHandler, IntPtr inEvent, IntPtr userData)
    {
        int status = GetEventParameter(
            inEvent,
            EventParamDirectObject,
            TypeEventHotKeyId,
            IntPtr.Zero,
            (uint)Marshal.SizeOf<EventHotKeyId>(),
            out uint actualSize,
            out EventHotKeyId eventId);

        _ = nextHandler;
        _ = userData;
        _ = actualSize;

        if (status != NoErr || eventId.Signature != Signature)
        {
            return status;
        }

        uint kind = GetEventKind(inEvent);
        lock (_sync)
        {
            if (!_handlersById.TryGetValue(eventId.Id, out (Action Pressed, Action? Released, IntPtr Ref) handler))
            {
                return NoErr;
            }

            if (kind == EventHotKeyPressed)
            {
                QueueInvoke(handler.Pressed);
            }
            else if (kind == EventHotKeyReleased && handler.Released != null)
            {
                QueueInvoke(handler.Released);
            }
        }

        return NoErr;
    }

    private void UnregisterById(uint id, string key)
    {
        if (!_handlersById.TryGetValue(id, out (Action Pressed, Action? Released, IntPtr Ref) handler))
        {
            _keyToId.Remove(key);
            return;
        }

        int status = UnregisterEventHotKey(handler.Ref);
        if (status != NoErr)
        {
            Trace.WriteLine($"注销mac全局热键失败: key={key}, status={status}");
        }

        _handlersById.Remove(id);
        _keyToId.Remove(key);
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
                Trace.WriteLine($"mac热键回调执行失败: {ex.Message}");
            }
        });
    }
}
