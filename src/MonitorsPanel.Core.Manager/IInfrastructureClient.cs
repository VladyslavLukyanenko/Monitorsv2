using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorsPanel.Core.Manager
{
  public interface IInfrastructureClient
  {
    /*
     * GET running instances
     * GET instances statuses
     *
     * POST run stopped instance or new if there are no stopped at the moment
     * DELETE stop instance
     * DELETE /permanent terminate instance
     */

    IReadOnlyList<ServerInstance> Instances { get; }

    // SupportedHostingTargets SupportedTargets { get; }
    string ProviderName { get; }
    IList<ServerInstance> BusyInstances { get; }
    bool HasAvailableIdleInstances { get; }
    bool HasAvailableInstances { get; }
    Task RefreshInstancesAsync(CancellationToken ct = default);

    Task RunOrStartInstanceAsync(ServerInstance instance = null, CancellationToken ct = default);

    Task StopInstanceAsync(ServerInstance instance, CancellationToken ct = default);
    Task TerminateInstanceAsync(ServerInstance instance, CancellationToken ct = default);
    ServerInstance GetAvailableIdleById(string serverId);
    ServerInstance GetRunningById(string serverId);
    IList<ServerInstance> GetRangeRunningImage(ImageInfo image);
    ServerInstance GetStoppedById(string instanceId);
    ServerInstance GetFirstAvailableIdle();
    bool IsSupportedImageType(ImageType imageType);
  }
}