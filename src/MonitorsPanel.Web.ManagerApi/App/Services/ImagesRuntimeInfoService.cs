using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using MonitorsPanel.Core.Manager;
using MonitorsPanel.Core.Manager.Services;

namespace MonitorsPanel.Web.ManagerApi.App.Services
{
  public class ImagesRuntimeInfoService : IImagesRuntimeInfoService
  {
    private readonly IDockerClientsProvider _dockerClientsProvider;

    private readonly IImageInfoRepository _imageInfoRepository;

    private readonly ILogger<ImagesRuntimeInfoService> _logger;

    public ImagesRuntimeInfoService(IDockerClientsProvider dockerClientsProvider,
      IImageInfoRepository imageInfoRepository, ILogger<ImagesRuntimeInfoService> logger)
    {
      _dockerClientsProvider = dockerClientsProvider;
      _imageInfoRepository = imageInfoRepository;
      _logger = logger;
    }

    public async Task RefreshStateAsync(IEnumerable<ServerInstance> nodes, CancellationToken ct = default)
    {
      _logger.LogDebug("Refreshing state");
      var monitors = await _imageInfoRepository.ListAllAsync(ct);
      var clients = new Dictionary<ServerInstance, DockerClient>();
      foreach (var serverNodeInfo in nodes)
      {
        _logger.LogDebug("Fetching docker client for node {NodeId}", serverNodeInfo.Id);
        if (!serverNodeInfo.IsRunning)
        {
          serverNodeInfo.Checked(false);
          continue;
        }

        DockerClient client;
        if (!_dockerClientsProvider.TryGetClient(serverNodeInfo, out client))
        {
          _dockerClientsProvider.AddClient(serverNodeInfo);
          client = _dockerClientsProvider.GetClient(serverNodeInfo);
        }

        _logger.LogDebug("Docker client found {IsFound} for node {NodeId}", (client != null).ToString(),
          serverNodeInfo.Id);
        clients[serverNodeInfo] = client;
      }

      _logger.LogDebug("Fetching running docker containers");
      var tasks = clients.Select(async p =>
      {
        var client = p.Value;
        var instance = p.Key;

        var isAvailable = instance.DockerRemoteApiUrl != null && await IsRemoteApiAvailableAsync(client, ct);
        _logger.LogDebug("Fetching running containers for node {NodeId}, is available", instance.Id,
          isAvailable.ToString());
        instance.Checked(isAvailable);
        if (instance.IsAvailable)
        {
          var containers = await client.Containers.ListContainersAsync(new ContainersListParameters {All = true}, ct);
          var images = monitors.SelectMany(m => containers.Where(c => c.ImageID == m.Id)
              .Select(c => new ImageRuntimeInfo(instance, m, c.ID, c.Status, c.State, c.Created)))
            .ToList();

          _logger.LogDebug("Running containers on node {NodeId} {@ImageInfos}", instance.Id, images);
          instance.Merge(images);
        }
      });

      await Task.WhenAll(tasks);
      _logger.LogDebug("Fetched");
    }

    private async Task<bool> IsRemoteApiAvailableAsync(DockerClient client, CancellationToken ct = default)
    {
      try
      {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        await client.System.PingAsync(cts.Token);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}