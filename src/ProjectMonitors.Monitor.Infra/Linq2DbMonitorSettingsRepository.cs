using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.Infra
{
  public class Linq2DbMonitorSettingsRepository : IMonitorSettingsRepository
  {
    private readonly LinqToDbConnectionOptions<MonitorSettingsDbConnection> _options;

    public Linq2DbMonitorSettingsRepository(LinqToDbConnectionOptions<MonitorSettingsDbConnection> options)
    {
      _options = options;
    }

    public async ValueTask<MonitorSettings?> GetSettingsAsync(MonitorInfo monitorInfo, CancellationToken ct)
    {
      await using var conn = new MonitorSettingsDbConnection(_options);
      return await conn.Settings.SingleOrDefaultAsync(
        _ => _.MonitorSlug == monitorInfo.Slug && _.MonitorName == monitorInfo.Name, ct);
    }

    public async ValueTask<bool> UpdateTargetsIfNotChangedAsync(MonitorInfo monitorInfo, DateTimeOffset targetChanges,
      IEnumerable<WatchTarget> targets, CancellationToken ct = default)
    {
      await using var conn = new MonitorSettingsDbConnection(_options);

      var changesCount = await conn.Settings
        .Where(_ => _.MonitorName == monitorInfo.Name
                    && _.MonitorSlug == monitorInfo.Slug
                    && _.UpdatedAt == targetChanges)
        .Set(_ => _.Targets, targets)
        .Set(_ => _.UpdatedAt, () => Sql.CurrentTimestampUtc)
        .UpdateAsync(ct);

      return changesCount == 1;
    }
  }
}