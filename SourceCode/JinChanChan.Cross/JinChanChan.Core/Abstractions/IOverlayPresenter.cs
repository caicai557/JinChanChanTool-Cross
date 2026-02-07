using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IOverlayPresenter
{
    void Present(AdvisorSnapshot snapshot);

    void Clear();
}
