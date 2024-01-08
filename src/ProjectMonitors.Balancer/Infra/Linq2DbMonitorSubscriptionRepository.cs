using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using ProjectMonitors.Balancer.Domain;

namespace ProjectMonitors.Balancer.Infra
{
  public class Linq2DbMonitorSubscriptionRepository : IMonitorSubscriptionRepository
  {
    private readonly LinqToDbConnectionOptions<MonitorSubscribersDbConnection> _options;

    public Linq2DbMonitorSubscriptionRepository(LinqToDbConnectionOptions<MonitorSubscribersDbConnection> options)
    {
      _options = options;
    }

    public async ValueTask<IList<MonitorSubscription>> GetSubscriptionsAsync(CancellationToken ct)
    {
      await using var conn = new MonitorSubscribersDbConnection(_options);
      return await conn.Subscriptions.ToListAsync(ct);
    }
  }
}