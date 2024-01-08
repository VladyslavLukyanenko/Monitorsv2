using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using MonitorsPanel.Core.Manager;
using MonitorsPanel.Core.Manager.Services;

namespace MonitorsPanel.Web.ManagerApi.App.Services
{
  public class ImagesManager : IImagesManager
  {
    private readonly IDockerClientsProvider _dockerClientsProvider;
    private readonly IImageInfoRepository _imageInfoRepository;
    private readonly IDockerAuthProvider _authProvider;

    public ImagesManager(IDockerClientsProvider dockerClientsProvider, IImageInfoRepository imageInfoRepository,
      IDockerAuthProvider authProvider)
    {
      _dockerClientsProvider = dockerClientsProvider;
      _imageInfoRepository = imageInfoRepository;
      _authProvider = authProvider;
    }

    public async Task<ImageInfo> AddImageAsync(string imageName, string slug, ImageType imageType,
      IEnumerable<string> spawnParams, CancellationToken ct = default)
    {
      var client = _dockerClientsProvider.GetLocalDockerClient();

      await client.Images.CreateImageAsync(new ImagesCreateParameters
        {
          FromImage = imageName
        },
        await _authProvider.GetAuthConfigAsync(ct),
        new Progress<JSONMessage>(),
        ct);

      var images = await client.Images.ListImagesAsync(new ImagesListParameters {All = true}, ct);
      var id = images.Where(_ => _.RepoTags.Any(d => d == imageName))
        .Select(_ => _.ID)
        .First();

      var imageInfo = new ImageInfo(id, imageName, slug, imageType, spawnParams);
      await _imageInfoRepository.CreateAsync(imageInfo, ct);

      return imageInfo;
    }

    public async Task<bool> TryStopContainerAsync(ServerInstance node, ImageInfo image, string containerId,
      CancellationToken ct = default)
    {
      if (!node.IsAvailable)
      {
        return false;
      }

      var client = _dockerClientsProvider.GetClient(node);
      var allContainers = await client.Containers.ListContainersAsync(new ContainersListParameters
      {
        All = true
      }, ct);

      var containers = allContainers.Where(_ => _.ImageID == image.Id);
      if (!string.IsNullOrEmpty(containerId))
      {
        containers = containers.Where(c => c.ID == containerId);
      }

      foreach (var c in containers)
      {
        if (c.State == ImageInfo.RunningStateValue)
        {
          if (!await TryStopMonitorAsync(node, c.ID, ct))
          {
            return false;
          }
        }
        else
        {
          await RemoveContainerAndRefreshAsync(c.ID, ct, client);
        }
      }

      return true;
    }
    //
    // public async Task<bool> TryStopContainersAsync(ImageInfo image, CancellationToken ct = default)
    // {
    //   var nodeIds = _imagesRuntimeInfoService.GetInfo().Where(_ => _.Value.ImagesInfo.Any(m => m.ImageId == image.Id))
    //     .Select(_ => _.Key)
    //     .ToArray();
    //
    //   var nodes = await _serverNodeInfoRepository.GetByIdsAsync(nodeIds, ct);
    //   foreach (var node in nodes)
    //   {
    //     if (!await TryStopContainerAsync(node, image, null, ct))
    //     {
    //       return false;
    //     }
    //   }
    //
    //   return true;
    // }


    public async Task SpawnImageAsync(ImageInfo image, ServerInstance node, IDictionary<string, string> spawnParams,
      CancellationToken ct = default)
    {
      if (image == null)
      {
        throw new ArgumentNullException(nameof(image));
      }

      if (node == null)
      {
        throw new ArgumentNullException(nameof(node));
      }

      if (spawnParams == null)
      {
        throw new ArgumentNullException(nameof(spawnParams));
      }

      var client = _dockerClientsProvider.GetClient(node);
      var authConfig = await _authProvider.GetAuthConfigAsync(ct);
      var progress = new Progress<JSONMessage>();
      await client.Images.CreateImageAsync(new ImagesCreateParameters
        {
          FromImage = image.Name,
        },
        authConfig,
        progress,
        ct);

      var list = spawnParams.Select(p =>
        {
          if (string.IsNullOrEmpty(p.Value))
          {
            return p.Key;
          }

          return $"{p.Key}={p.Value}";
        })
        .ToList();

      list.Add($"{ImageInfo.MonitorPublicSlugParamName}={image.Slug}");
      var r = await client.Containers.CreateContainerAsync(new CreateContainerParameters
      {
        Image = image.Name,
        Env = list,
        HostConfig = new HostConfig
        {
          RestartPolicy = new RestartPolicy
          {
            Name = RestartPolicyKind.Always
          }
        }
      }, ct);

      var started = await client.Containers.StartContainerAsync(r.ID, new ContainerStartParameters(), ct);
      if (!started)
      {
        throw new InvalidOperationException("Unable to start container for image " + image.Id);
      }
    }

    private async Task<bool> TryStopMonitorAsync(ServerInstance node, string containerId,
      CancellationToken ct = default)
    {
      var client = _dockerClientsProvider.GetClient(node);
      var stopResult = await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), ct);
      if (!stopResult)
      {
        return false;
      }

      return await RemoveContainerAndRefreshAsync(containerId, ct, client);
    }

    private async Task<bool> RemoveContainerAndRefreshAsync(string containerId, CancellationToken ct,
      DockerClient client)
    {
      await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
      {
        RemoveVolumes = false,
        Force = true
      }, ct);

      return true;
    }
  }
}