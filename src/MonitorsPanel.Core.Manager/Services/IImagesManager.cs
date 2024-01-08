using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorsPanel.Core.Manager.Services
{
  public interface IImagesManager
  {
    Task<ImageInfo> AddImageAsync(string imageName, string slug, ImageType imageType,
      IEnumerable<string> spawnParams, CancellationToken ct = default);
    
    Task<bool> TryStopContainerAsync(ServerInstance node, ImageInfo image, CancellationToken ct = default)
    {
      return TryStopContainerAsync(node, image, null, ct);
    }

    Task<bool> TryStopContainerAsync(ServerInstance node, ImageInfo image, string containerId,
      CancellationToken ct = default);

    async Task<bool> TryStopContainerAsync(IEnumerable<ServerInstance> nodes, ImageInfo image,
      CancellationToken ct = default)
    {
      var areAllStopped = true;
      foreach (var node in nodes)
      {
        var runtimeImages = node.Images.Where(_ => _.ImageInfo.Id == image.Id);
        foreach (var runtimeImage in runtimeImages)
        {
          areAllStopped = await TryStopContainerAsync(node, image, runtimeImage.ContainerId, ct) && areAllStopped;
        }
      }

      return areAllStopped;
    }
    

    // Task<bool> TryStopContainersAsync(ImageInfo image, CancellationToken ct = default);

    Task SpawnImageAsync(ImageInfo image, ServerInstance server, IDictionary<string, string> spawnParams,
      CancellationToken ct = default);
  }
}