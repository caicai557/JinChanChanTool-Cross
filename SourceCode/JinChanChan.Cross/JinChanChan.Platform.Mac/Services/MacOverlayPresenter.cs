using System.Diagnostics;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Mac.Services;

public sealed class MacOverlayPresenter : IOverlayPresenter
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
        Trace.WriteLine($"[MacOverlay] {summary}");
    }

    public void Clear()
    {
        _lastSummary = string.Empty;
        Trace.WriteLine("[MacOverlay] Clear");
    }
}
