using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Compute.v1;
using Google.Apis.Compute.v1.Data;

namespace MonitorsPanel.Core.Manager.Services.GoogleComputeEngine
{
  public class GoogleComputeEngineInfrastructureClient : InfrastructureClientBase
  {
    private static readonly IDictionary<string, GoogleComputeEngineServerInstance> CachedInstances =
      new Dictionary<string, GoogleComputeEngineServerInstance>();

    private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

    private readonly GoogleComputeEngineConfig _config;
    private readonly ComputeService _computeService;
    private readonly IImagesRuntimeInfoService _imagesRuntimeInfoService;

    public GoogleComputeEngineInfrastructureClient(GoogleComputeEngineConfig config, ComputeService computeService,
      IImagesManager imagesManager, IImagesRuntimeInfoService imagesRuntimeInfoService,
      IDockerClientsProvider dockerClientsProvider)
      : base("GoogleComputeEngine", config.HostingTargets, imagesManager, dockerClientsProvider)
    {
      _config = config;
      _computeService = computeService;
      _imagesRuntimeInfoService = imagesRuntimeInfoService;
    }

    public override IReadOnlyList<ServerInstance> Instances => CachedInstances.Values.ToList().AsReadOnly();

    public override async Task RefreshInstancesAsync(CancellationToken ct = default)
    {
      var instancesResponse = await _computeService.Instances.List(_config.ProjectId, _config.Zone).ExecuteAsync(ct);

      var items = instancesResponse.Items ?? Array.Empty<Instance>();
      var instances = items
        .Select(MapToAwsServerInstance)
        .ToList();

      MergeInstances(instances, items);
      await RefreshStatsAsync(instances, ct);
    }

    protected override async Task RunOrStartRemoteInstanceAsync(ServerInstance instance = null,
      CancellationToken ct = default)
    {
      if (instance != null)
      {
        if (!instance.IsStopped)
        {
          throw new InvalidOperationException(
            $"Server should be stopped to start it. Id: {instance.Id}, Status: {instance.Status}");
        }

        var startRequest = _computeService.Instances.Start(_config.ProjectId, _config.Zone, instance.Id);
        await startRequest.ExecuteAsync(ct);
      }
      else
      {
        var instanceToCreate = new Instance
        {
          MachineType = $"zones/{_config.Zone}/machineTypes/{_config.MachineType}",
          Zone = _config.Zone,
          Name = "instance-" + DateTimeOffset.Now.ToUnixTimeSeconds(),
          Tags = new Tags
          {
            Items = new List<string>
            {
              "http-server"
            }
          }
        };
        var insertRequest = _computeService.Instances.Insert(instanceToCreate, _config.ProjectId, _config.Zone);
        insertRequest.SourceInstanceTemplate = "global/instanceTemplates/" + _config.SourceInstanceTemplate;
        await insertRequest.ExecuteAsync(ct);
      }
    }

    protected override Task StopRemoteInstanceAsync(ServerInstance instance, CancellationToken ct = default)
    {
      var stopRequest = _computeService.Instances.Stop(_config.ProjectId, _config.Zone, instance.Id);
      return stopRequest.ExecuteAsync(ct);
    }

    protected override async Task TerminateRemoteInstanceAsync(ServerInstance instance, CancellationToken ct = default)
    {
      await StopRemoteInstanceAsync(instance, ct);
      var deleteRequest = _computeService.Instances.Delete(_config.ProjectId, _config.Zone, instance.Id);
      await deleteRequest.ExecuteAsync(ct);
    }

    private GoogleComputeEngineServerInstance MapToAwsServerInstance(Instance i) =>
      new GoogleComputeEngineServerInstance(i.Name, _config.HostingTargets, i.Zone, i.MachineType,
        DateTimeOffset.MinValue)
      {
        Status = i.Status.ToGoogleComputeEngineInstanceStatus()
      };

    private async Task RefreshStatsAsync(List<GoogleComputeEngineServerInstance> instances, CancellationToken ct)
    {
      //
      // foreach (var status in statusResponse.InstanceStatuses)
      // {
      //   var instance = Instances[status.InstanceId];
      //
      //   instance.Status = status.InstanceState.Name.Value.ToAwsInstanceStatus();
      //   instance.AdditionalStats = status.Status.Details.ToDictionary(_ => _.Name.Value, _ => _.Status.Value);
      //   foreach (var detail in status.SystemStatus.Details)
      //   {
      //     instance.AdditionalStats[detail.Name.Value] = detail.Status.Value;
      //   }
      // }

      await _imagesRuntimeInfoService.RefreshStateAsync(CachedInstances.Values, ct);
    }

    private void MergeInstances(List<GoogleComputeEngineServerInstance> instances, IEnumerable<Instance> srcInstances)
    {
      var srcDict = srcInstances.ToDictionary(_ => _.Name);
      try
      {
        SemaphoreSlim.Wait();
        var ids = instances.Select(_ => _.Id).Distinct().ToArray();
        foreach (var key in CachedInstances.Keys.ToArray())
        {
          if (!ids.Contains(key))
          {
            CachedInstances.Remove(key);
          }
        }

        foreach (var instance in instances)
        {
          if (!CachedInstances.ContainsKey(instance.Id))
          {
            CachedInstances[instance.Id] = instance;
          }

          var src = srcDict[instance.Id];

          var curr = CachedInstances[instance.Id];
          curr.Status = instance.Status;
          curr.PublicDnsName = src.NetworkInterfaces.SelectMany(_ => _.AccessConfigs)
            .Select(_ => _.NatIP)
            .FirstOrDefault();
          // curr.LaunchTime = DateTimeOffset.Parse(src.CreationTimestamp);
          // curr.PublicDnsName = src.;
          // curr.LaunchTime = src.;
        }
      }
      finally
      {
        SemaphoreSlim.Release();
      }
    }
  }
}