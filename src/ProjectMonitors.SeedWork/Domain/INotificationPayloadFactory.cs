using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.SeedWork.Domain
{
  public interface INotificationPayloadFactory
  {
    ValueTask<string> ToJsonAsync(CancellationToken ct);
  }
}