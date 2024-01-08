using System.Collections.Generic;
using MonitorsPanel.Core.Manager;

namespace MonitorsPanel.Web.ManagerApi.App.Model
{
  public class GroupedInstanceList
  {
    public string ProviderName { get; set; }
    public List<ServerInstance> Instances { get; set; } = new List<ServerInstance>();
  }
}