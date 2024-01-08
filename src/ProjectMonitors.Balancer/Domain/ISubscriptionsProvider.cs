using System.Collections.Generic;

namespace ProjectMonitors.Balancer.Domain
{
  public interface ISubscriptionsProvider
  {
    bool HasSubscribersFor(string slug);
    IEnumerable<MonitorSubscription> GetSubscribersFor(string slug);
  }
}