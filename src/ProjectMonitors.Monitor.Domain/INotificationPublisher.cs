using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Domain
{
  public interface INotificationPublisher
  {
    ValueTask<Result> PublishAsync(string targetId, ProductStatus status, WatchTarget spec, CancellationToken ct);
    ValueTask<Result> PublishStatsAsync(ComponentStats stats, CancellationToken ct);
  }
}