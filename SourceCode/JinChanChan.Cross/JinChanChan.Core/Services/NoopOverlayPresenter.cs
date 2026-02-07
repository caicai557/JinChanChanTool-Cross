using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class NoopOverlayPresenter : IOverlayPresenter
{
    public void Present(AdvisorSnapshot snapshot)
    {
        _ = snapshot;
    }

    public void Clear()
    {
    }
}
