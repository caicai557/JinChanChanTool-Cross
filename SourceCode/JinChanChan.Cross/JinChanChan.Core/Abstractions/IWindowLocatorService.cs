using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IWindowLocatorService
{
    Task<IReadOnlyList<WindowDescriptor>> ListWindowsAsync(CancellationToken cancellationToken = default);

    Task<WindowDescriptor?> FindBestGameWindowAsync(string processName, CancellationToken cancellationToken = default);
}
