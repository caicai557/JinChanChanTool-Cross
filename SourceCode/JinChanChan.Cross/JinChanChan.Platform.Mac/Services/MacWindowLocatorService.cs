using System.Diagnostics;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Mac.Services;

public sealed class MacWindowLocatorService : IWindowLocatorService
{
    public async Task<IReadOnlyList<WindowDescriptor>> ListWindowsAsync(CancellationToken cancellationToken = default)
    {
        List<WindowDescriptor> windows = new();

        foreach (Process process in Process.GetProcesses().OrderBy(p => p.ProcessName))
        {
            cancellationToken.ThrowIfCancellationRequested();
            WindowDescriptor? descriptor = await GetFrontWindowByProcessAsync(process.ProcessName, cancellationToken);
            if (descriptor != null)
            {
                windows.Add(descriptor);
            }
        }

        return windows;
    }

    public Task<WindowDescriptor?> FindBestGameWindowAsync(string processName, CancellationToken cancellationToken = default)
    {
        return GetFrontWindowByProcessAsync(processName, cancellationToken);
    }

    private static async Task<WindowDescriptor?> GetFrontWindowByProcessAsync(string processName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return null;
        }

        string script = $@"
            tell application ""System Events""
                if exists process ""{EscapeAppleScript(processName)}"" then
                    tell process ""{EscapeAppleScript(processName)}""
                        if exists front window then
                            set p to position of front window
                            set s to size of front window
                            set t to name of front window
                            return t & ""|"" & item 1 of p & ""|"" & item 2 of p & ""|"" & item 1 of s & ""|"" & item 2 of s
                        end if
                    end tell
                end if
            end tell
            return """"
        ";

        ProcessStartInfo psi = new()
        {
            FileName = "osascript",
            ArgumentList = { "-e", script },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        string[] parts = output.Trim().Split('|');
        if (parts.Length < 5)
        {
            return null;
        }

        if (!int.TryParse(parts[1], out int x)
            || !int.TryParse(parts[2], out int y)
            || !int.TryParse(parts[3], out int w)
            || !int.TryParse(parts[4], out int h))
        {
            return null;
        }

        return new WindowDescriptor
        {
            Id = processName.GetHashCode(StringComparison.Ordinal),
            Title = parts[0],
            ProcessName = processName,
            Bounds = new ScreenRect(x, y, w, h),
            IsVisible = true
        };
    }

    private static string EscapeAppleScript(string input)
    {
        return input.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
