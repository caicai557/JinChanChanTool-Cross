namespace JinChanChan.Core.Models;

public sealed class PlatformCapabilities
{
    public bool SupportsWindowCapture { get; init; }

    public bool SupportsInputInjection { get; init; }

    public bool SupportsGlobalHotkey { get; init; }

    public bool SupportsWindowEnumeration { get; init; }

    public bool RequiresScreenRecordingPermission { get; init; }

    public bool RequiresAccessibilityPermission { get; init; }
}
