using System.Diagnostics;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Windows.Services;

public sealed class WindowsOverlayPresenter : IOverlayPresenter
{
    private string _lastSummary = string.Empty;

    public void Present(AdvisorSnapshot snapshot)
    {
        string summary = snapshot.Summary ?? string.Empty;
        if (string.Equals(summary, _lastSummary, StringComparison.Ordinal))
        {
            return;
        }

        _lastSummary = summary;
        Trace.WriteLine($"[WindowsOverlay] {summary}");
    }

    public void Clear()
    {
        _lastSummary = string.Empty;
        Trace.WriteLine("[WindowsOverlay] Clear");
    }
}
