using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonitorsPanel.Core.Manager;
using MonitorsPanel.Core.Manager.Services;
using MonitorsPanel.Web.ManagerApi.Commands;
using MonitorsPanel.Web.ManagerApi.Foundation.Model;
using MonitorsPanel.Web.ManagerApi.Foundation.Mvc.Controllers;

namespace MonitorsPanel.Web.ManagerApi.Controllers
{
  public class ImagesController : SecuredControllerBase
  {
    private readonly IImagesManager _imagesManager;
    private readonly IImageInfoRepository _imageInfoRepository;
    private readonly IEnumerable<IInfrastructureClient> _infrastructureClients;

    public ImagesController(IImagesManager imagesManager, IImageInfoRepository imageInfoRepository,
      IServiceProvider serviceProvider, IEnumerable<IInfrastructureClient> infrastructureClients)
      : base(serviceProvider)
    {
      _imagesManager = imagesManager;
      _imageInfoRepository = imageInfoRepository;
      _infrastructureClients = infrastructureClients;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiContract<List<ImageInfo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableImagesList(CancellationToken ct)
    {
      var all = await _imageInfoRepository.ListAllAsync(ct);
      return Ok(all);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiContract<long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateImageCommand cmd,
      CancellationToken ct)
    {
      var created = await _imagesManager.AddImageAsync(cmd.ImageName, cmd.Slug, cmd.ImageType,
        cmd.RequiredSpawnParameters, ct);

      return Ok(created);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAsync(string imageId, CancellationToken ct)
    {
      ImageInfo image = await _imageInfoRepository.GetByIdAsync(imageId, ct);
      if (image == null)
      {
        return NotFound();
      }

      var nodes = _infrastructureClients.SelectMany(_ => _.GetRangeRunningImage(image));
      await _imagesManager.TryStopContainerAsync(nodes, image, ct);
      _imageInfoRepository.Remove(image);

      return NoContent();
    }

    [HttpPost("spawn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SpawnImageAsync([FromBody] SpawnImageCommand cmd, CancellationToken ct)
    {
      ImageInfo image = await _imageInfoRepository.GetByIdAsync(cmd.ImageId, ct);
      if (image == null)
      {
        return NotFound($"Image '{cmd.ImageId}' not found");
      }

      var serverSpecified = !string.IsNullOrEmpty(cmd.ServerId);
      var result = _infrastructureClients
        .Where(_ => _.IsSupportedImageType(image.ImageType) && _.HasAvailableInstances)
        .Select(client =>
        {
          var s = serverSpecified
            ? client.GetRunningById(cmd.ServerId)
            : client.Instances.FirstOrDefault();

          return (Client: client, Server: s);
        })
        .FirstOrDefault(_ => _.Server != null);

      var server = result.Server;
      if (server == null)
      {
        return NotFound(serverSpecified
          ? $"Server '{cmd.ServerId}' not found"
          : "No available idle servers found to spawn image");
      }

      await _imagesManager.SpawnImageAsync(image, server, cmd.Parameters, ct);
      await result.Client.RefreshInstancesAsync(ct);

      return NoContent();
    }

    [HttpDelete("spawn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ShutdownContainerAsync(string serverId, string providerId, string imageId,
      string containerId, CancellationToken ct)
    {
      ImageInfo image = await _imageInfoRepository.GetByIdAsync(imageId, ct);
      if (image == null)
      {
        return NotFound($"Image '{imageId}' not found");
      }

      var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == providerId);
      var server = client?.GetRunningById(serverId);
      if (server == null)
      {
        return NotFound($"Server '{serverId}' not found");
      }

      await _imagesManager.TryStopContainerAsync(server, image, containerId, ct);
      await client.RefreshInstancesAsync(ct);

      return NoContent();
    }
  }
}