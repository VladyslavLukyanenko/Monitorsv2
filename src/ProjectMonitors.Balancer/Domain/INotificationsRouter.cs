using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Balancer.Domain
{
  public interface INotificationsRouter
  {
    ValueTask RouteAsync(NotificationPayload payload, CancellationToken ct = default);
  }
}