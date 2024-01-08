using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonitorsPanel.Core.Manager;
using MonitorsPanel.Web.ManagerApi.App.Model;
using MonitorsPanel.Web.ManagerApi.Commands;
using MonitorsPanel.Web.ManagerApi.Foundation.Model;
using MonitorsPanel.Web.ManagerApi.Foundation.Mvc.Controllers;

namespace MonitorsPanel.Web.ManagerApi.Controllers
{
  public class ServerInstancesController : SecuredControllerBase
  {
    private readonly IEnumerable<IInfrastructureClient> _infrastructureClients;

    public ServerInstancesController(IServiceProvider serviceProvider,
      IEnumerable<IInfrastructureClient> infrastructureClients)
      : base(serviceProvider)
    {
      _infrastructureClients = infrastructureClients;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiContract<GroupedInstanceList[]>), StatusCodes.Status200OK)]
    public IActionResult GetServersList()
    {
      var list = _infrastructureClients.Select(_ => new GroupedInstanceList
        {
          ProviderName = _.ProviderName,
          Instances = _.Instances.ToList()
        })
        .ToArray();

      return Ok(list);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiContract<long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartOrRunServerAsync(StartOrRunServerInstanceCommand cmd, CancellationToken ct)
    {
      var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == cmd.ProviderName);
      if (client == null)
      {
        return BadRequest($"Invalid provider name '{cmd.ProviderName}'");
      }

      ServerInstance instance = null;
      if (!string.IsNullOrEmpty(cmd.StoppedInstanceId))
      {
        instance = client.GetStoppedById(cmd.StoppedInstanceId);
        if (instance == null)
        {
          return NotFound($"Instance '{cmd.StoppedInstanceId}' not found.");
        }
      }

      await client.RunOrStartInstanceAsync(instance, ct);

      return Ok();
    }

    [HttpDelete("{provider}/{serverId}/permanent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TerminateServerAsync(string provider, string serverId, CancellationToken ct)
    {
      var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == provider);
      if (client == null)
      {
        return NotFound();
      }

      var instance = client.GetRunningById(serverId) ?? client.GetStoppedById(serverId);
      if (instance == null)
      {
        return BadRequest("Instance not found or it not running");
      }

      await client.TerminateInstanceAsync(instance, ct);
      return NoContent();
    }

    [HttpDelete("{provider}/{serverId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> StopServerAsync(string provider, string serverId, CancellationToken ct)
    {
      var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == provider);
      if (client == null)
      {
        return NotFound();
      }

      var instance = client.GetRunningById(serverId);
      if (instance == null)
      {
        return BadRequest("Instance not found or it not running");
      }

      await client.StopInstanceAsync(instance, ct);
      return NoContent();
    }
  }
}