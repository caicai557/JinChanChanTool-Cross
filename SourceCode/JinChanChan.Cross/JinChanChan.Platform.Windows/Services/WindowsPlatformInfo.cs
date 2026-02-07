using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Windows.Services;

public static class WindowsPlatformInfo
{
    public static PlatformCapabilities Capabilities => new()
    {
        SupportsWindowCapture = true,
        SupportsInputInjection = true,
        SupportsGlobalHotkey = true,
        SupportsWindowEnumeration = true,
        RequiresAccessibilityPermission = false,
        RequiresScreenRecordingPermission = false
    };
}
