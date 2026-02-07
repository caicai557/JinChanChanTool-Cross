using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IPermissionService
{
    Task<bool> CheckPermissionAsync(PermissionKind permissionKind, CancellationToken cancellationToken = default);

    Task<bool> RequestPermissionAsync(PermissionKind permissionKind, CancellationToken cancellationToken = default);
}
