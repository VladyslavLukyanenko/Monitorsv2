using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ProjectMonitors.SeedWork.Domain
{
    public interface ISender
    {
        ValueTask<Result> SendAsync(PublishPayload payload, CancellationToken ct);
    }
}