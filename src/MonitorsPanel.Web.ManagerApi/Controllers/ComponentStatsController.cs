using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonitorsPanel.Web.ManagerApi.App.Model;
using MonitorsPanel.Web.ManagerApi.App.Services;
using MonitorsPanel.Web.ManagerApi.Foundation.Model;
using MonitorsPanel.Web.ManagerApi.Foundation.Mvc.Controllers;

namespace MonitorsPanel.Web.ManagerApi.Controllers
{
  public class ComponentStatsController : SecuredControllerBase
  {
    private readonly IComponentsStateRegistry _registry;

    public ComponentStatsController(IComponentsStateRegistry registry, IServiceProvider provider)
      : base(provider)
    {
      _registry = registry;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiContract<Dictionary<string, ComponentStatsEntry[]>>), StatusCodes.Status200OK)]
    public IActionResult GetStats()
    {
      var entries = _registry.GetGroupedEntries(TimeSpan.FromMinutes(1))
        .ToDictionary(_ => _.Key, _ => _.ToArray());
      return Ok(entries);
    }
  }
}