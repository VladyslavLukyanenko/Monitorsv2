using System;

namespace MonitorsPanel.Web.ManagerApi.App.Model
{
  public class ComponentStatsEntry
  {
    public string ComponentType { get; set; }
    public string ComponentName { get; set; }
    public string Stats { get; set; }
    public DateTimeOffset Timestamp { get; set; }
  }
}