namespace JinChanChan.Platform.Tests;

public class PlatformCapabilitiesTests
{
#if WINDOWS
    [Fact]
    public void WindowsCapabilities_ShouldEnableCoreFeatures()
    {
        var caps = JinChanChan.Platform.Windows.Services.WindowsPlatformInfo.Capabilities;
        Assert.True(caps.SupportsWindowCapture);
        Assert.True(caps.SupportsInputInjection);
        Assert.True(caps.SupportsWindowEnumeration);
    }
#endif

#if MACOS
    [Fact]
    public void MacCapabilities_ShouldRequirePermissions()
    {
        var caps = JinChanChan.Platform.Mac.Services.MacPlatformInfo.Capabilities;
        Assert.True(caps.RequiresScreenRecordingPermission);
        Assert.True(caps.RequiresAccessibilityPermission);
    }
#endif
}
