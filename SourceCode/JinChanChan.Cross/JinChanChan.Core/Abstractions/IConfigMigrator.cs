using JinChanChan.Core.Config;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IConfigMigrator
{
    Task<(AppSettings Settings, MigrationReport Report)> MigrateAsync(string resourcesPath, CancellationToken cancellationToken = default);
}
