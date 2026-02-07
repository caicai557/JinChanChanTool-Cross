using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Platform.Windows.Services;

public sealed class WindowsPermissionService : IPermissionService
{
    public Task<bool> CheckPermissionAsync(PermissionKind permissionKind, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    public Task<bool> RequestPermissionAsync(PermissionKind permissionKind, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }
}
