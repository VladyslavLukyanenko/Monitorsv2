using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Balancer.Domain
{
  public interface IMonitorSubscriptionRepository
  {
    ValueTask<IList<MonitorSubscription>> GetSubscriptionsAsync(CancellationToken ct);
  }
}