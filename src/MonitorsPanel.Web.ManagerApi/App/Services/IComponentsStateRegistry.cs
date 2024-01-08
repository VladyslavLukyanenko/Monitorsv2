using System;
using System.Linq;
using MonitorsPanel.Web.ManagerApi.App.Model;

namespace MonitorsPanel.Web.ManagerApi.App.Services
{
  public interface IComponentsStateRegistry
  {
    ILookup<string, ComponentStatsEntry> GetGroupedEntries(TimeSpan maxRefreshDelay);
    void Update(ComponentStatsEntry entry);
  }
}