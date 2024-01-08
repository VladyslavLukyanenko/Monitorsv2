using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace MonitorsPanel.Core.Manager.Services.Aws
{
  // todo: implement fault tolerance + retry policy
  public class AwsInfrastructureClient : InfrastructureClientBase
  {
    private static readonly IDictionary<string, AwsServerInstance> CachedInstances =
      new Dictionary<string, AwsServerInstance>();

    private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

    private readonly AwsConfig _awsConfig;
    private readonly AmazonEC2Client _amazonEc2Client;
    private readonly IImagesRuntimeInfoService _imagesRuntimeInfoService;

    public AwsInfrastructureClient(AwsConfig awsConfig, AmazonEC2Client amazonEc2Client, IImagesManager imagesManager,
      IImagesRuntimeInfoService imagesRuntimeInfoService, IDockerClientsProvider dockerClientsProvider)
      : base("AWS", awsConfig.HostingTargets, imagesManager, dockerClientsProvider)
    {
      _awsConfig = awsConfig;
      _amazonEc2Client = amazonEc2Client;
      _imagesRuntimeInfoService = imagesRuntimeInfoService;
    }

    public override IReadOnlyList<ServerInstance> Instances => CachedInstances.Values.ToList().AsReadOnly();

    public override async Task RefreshInstancesAsync(CancellationToken ct = default)
    {
      var instancesResponse = await _amazonEc2Client.DescribeInstancesAsync(new DescribeInstancesRequest(), ct);

      var srcInstances = instancesResponse.Reservations.SelectMany(_ => _.Instances).ToArray();
      var instances = srcInstances
        .Where(_ => _.KeyName == _awsConfig.KeyName)
        .Select(MapToAwsServerInstance)
        .ToList();

      MergeInstances(instances, srcInstances);
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

        await _amazonEc2Client.StartInstancesAsync(new StartInstancesRequest(new List<string> {instance.Id}), ct);
        return;
      }

      await _amazonEc2Client.RunInstancesAsync(new RunInstancesRequest(_awsConfig.AmiId, 1, 1)
      {
        InstanceType = InstanceType.FindValue(_awsConfig.InstanceType),
        KeyName = _awsConfig.KeyName,
        SecurityGroupIds = _awsConfig.SecurityGroupIds
      }, ct);
    }

    protected override async Task StopRemoteInstanceAsync(ServerInstance instance, CancellationToken ct = default)
    {
      await _amazonEc2Client.StopInstancesAsync(new StopInstancesRequest(new List<string> {instance.Id}), ct);
    }

    protected override async Task TerminateRemoteInstanceAsync(ServerInstance instance, CancellationToken ct = default)
    {
      var request = new TerminateInstancesRequest(new List<string> {instance.Id});
      await _amazonEc2Client.TerminateInstancesAsync(request, ct);
    }

    private AwsServerInstance MapToAwsServerInstance(Instance i) =>
      new AwsServerInstance(i.InstanceId, _awsConfig.HostingTargets, i.Placement.AvailabilityZone, i.KeyName,
        i.InstanceType);

    private async Task RefreshStatsAsync(List<AwsServerInstance> instances, CancellationToken ct)
    {
      var statusResponse = await _amazonEc2Client.DescribeInstanceStatusAsync(new DescribeInstanceStatusRequest
      {
        InstanceIds = instances.Select(_ => _.Id).ToList(),
        IncludeAllInstances = true
      }, ct);

      foreach (var status in statusResponse.InstanceStatuses)
      {
        var instance = CachedInstances[status.InstanceId];

        instance.Status = status.InstanceState.Name.Value.ToAwsInstanceStatus();
        instance.AdditionalStats = status.Status.Details.ToDictionary(_ => _.Name.Value, _ => _.Status.Value);
        foreach (var detail in status.SystemStatus.Details)
        {
          instance.AdditionalStats[detail.Name.Value] = detail.Status.Value;
        }
      }

      await _imagesRuntimeInfoService.RefreshStateAsync(CachedInstances.Values, ct);
    }

    private static void MergeInstances(List<AwsServerInstance> instances, IEnumerable<Instance> srcInstances)
    {
      var srcDict = srcInstances.ToDictionary(_ => _.InstanceId);
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
          curr.PublicDnsName = src.PublicDnsName;
          curr.LaunchTime = src.LaunchTime;
        }
      }
      finally
      {
        SemaphoreSlim.Release();
      }
    }
  }
}