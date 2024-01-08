using System.Collections.Generic;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Balancer.Domain
{
  public class CantRouteMessageException : BalancerException
  {
    public CantRouteMessageException(IEnumerable<MonitorSubscription> subscriptions, NotificationPayload notification)
      : base("Can't route notification")
    {
      Subscriptions = subscriptions;
      Notification = notification;
    }

    public IEnumerable<MonitorSubscription> Subscriptions { get; }
    public NotificationPayload Notification { get; }
  }
}