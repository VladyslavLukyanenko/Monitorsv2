using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonitorsPanel.Core.Manager.Services;

namespace MonitorsPanel.Core.Manager
{
  public abstract class InfrastructureClientBase : IInfrastructureClient
  {
    private readonly SupportedHostingTargets _hostingTargets;
    private readonly IImagesManager _imagesManager;
    private readonly IDockerClientsProvider _dockerClientsProvider;

    protected InfrastructureClientBase(string providerName, SupportedHostingTargets hostingTargets,
      IImagesManager imagesManager, IDockerClientsProvider dockerClientsProvider)
    {
      _hostingTargets = hostingTargets;
      _imagesManager = imagesManager;
      _dockerClientsProvider = dockerClientsProvider;
      ProviderName = providerName;
    }

    public abstract IReadOnlyList<ServerInstance> Instances { get; }
    public bool HasAvailableIdleInstances => Instances.Any(_ => _.IsAvailable && _.IsIdle);
    public bool HasAvailableInstances => Instances.Any(_ => _.IsAvailable);
    public abstract Task RefreshInstancesAsync(CancellationToken ct = default);

    protected abstract Task RunOrStartRemoteInstanceAsync(ServerInstance instance = null,
      CancellationToken ct = default);

    protected abstract Task StopRemoteInstanceAsync(ServerInstance instance, CancellationToken ct = default);
    protected abstract Task TerminateRemoteInstanceAsync(ServerInstance instance, CancellationToken ct = default);

    public string ProviderName { get; }
    public IList<ServerInstance> BusyInstances => Instances.Where(_ => _.IsRunning && !_.IsIdle).ToList();

    public ServerInstance GetAvailableIdleById(string serverId) =>
      Instances.FirstOrDefault(_ => _.IsAvailable && _.IsIdle && _.Id == serverId);

    public ServerInstance GetRunningById(string serverId) =>
      Instances.FirstOrDefault(_ => _.IsRunning && _.Id == serverId);

    public IList<ServerInstance> GetRangeRunningImage(ImageInfo image)
    {
      return Instances.Where(_ => _.IsRunning && _.Images.Any(i => i.ImageInfo.Id == image.Id))
        .ToList();
    }

    public ServerInstance GetStoppedById(string instanceId)
    {
      return Instances.FirstOrDefault(_ => _.IsStopped && _.Id == instanceId);
    }

    public async Task RunOrStartInstanceAsync(ServerInstance instance = null, CancellationToken ct = default)
    {
      await RunOrStartRemoteInstanceAsync(instance, ct);
      await RefreshInstancesAsync(ct);
    }

    public async Task StopInstanceAsync(ServerInstance instance, CancellationToken ct = default)
    {
      await ShutdownContainersAsync(instance, ct);
      await StopRemoteInstanceAsync(instance, ct);
      await RefreshInstancesAsync(ct);
    }

    public async Task TerminateInstanceAsync(ServerInstance instance, CancellationToken ct = default)
    {
      await ShutdownContainersAsync(instance, ct);
      await TerminateRemoteInstanceAsync(instance, ct);

      await RefreshInstancesAsync(ct);
    }

    public ServerInstance GetFirstAvailableIdle() =>
      Instances.FirstOrDefault(_ => _.IsAvailable && _.IsIdle);

    public bool IsSupportedImageType(ImageType imageType) => _hostingTargets.HasFlag(imageType.ToSupportedTargets());

    protected async Task ShutdownContainersAsync(ServerInstance instance, CancellationToken ct)
    {
      foreach (var info in instance.Images.ToArray())
      {
        await _imagesManager.TryStopContainerAsync(instance, info.ImageInfo, ct);
      }

      instance.Images.Clear();
      instance.PublicDnsName = null;
      instance.Checked(false);
      _dockerClientsProvider.RemoveClient(instance);
    }
  }
}