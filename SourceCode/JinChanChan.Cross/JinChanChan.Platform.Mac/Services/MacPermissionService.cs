using System.Diagnostics;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Mac.Services;

public sealed class MacPermissionService : IPermissionService
{
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern bool CGPreflightScreenCaptureAccess();

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern bool CGRequestScreenCaptureAccess();

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern bool AXIsProcessTrusted();

    public Task<bool> CheckPermissionAsync(PermissionKind permissionKind, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool granted = permissionKind switch
        {
            PermissionKind.ScreenRecording => CGPreflightScreenCaptureAccess(),
            PermissionKind.Accessibility => AXIsProcessTrusted(),
            _ => false
        };

        return Task.FromResult(granted);
    }

    public Task<bool> RequestPermissionAsync(PermissionKind permissionKind, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool granted = permissionKind switch
        {
            PermissionKind.ScreenRecording => CGRequestScreenCaptureAccess(),
            PermissionKind.Accessibility => OpenAccessibilityPrivacyPage(),
            _ => false
        };

        return Task.FromResult(granted);
    }

    private static bool OpenAccessibilityPrivacyPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                ArgumentList = { "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility" },
                UseShellExecute = false
            });
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"打开辅助功能权限页失败: {ex.Message}");
            return false;
        }
    }
}
