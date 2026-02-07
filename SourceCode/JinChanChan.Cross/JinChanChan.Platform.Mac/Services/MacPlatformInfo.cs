using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Mac.Services;

public static class MacPlatformInfo
{
    public static PlatformCapabilities Capabilities => new()
    {
        SupportsWindowCapture = true,
        SupportsInputInjection = true,
        SupportsGlobalHotkey = true,
        SupportsWindowEnumeration = true,
        RequiresAccessibilityPermission = true,
        RequiresScreenRecordingPermission = true
    };
}
