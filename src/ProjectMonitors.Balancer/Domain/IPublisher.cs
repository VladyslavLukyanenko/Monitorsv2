using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Balancer.Domain
{
  public interface IPublisher
  {
    ValueTask PublishNotificationAsync(PublishPayload payload, CancellationToken ct);
    ValueTask PublishStatsAsync(IEnumerable<BalancerSubscriptionEntry> entries, CancellationToken ct);
  }
}