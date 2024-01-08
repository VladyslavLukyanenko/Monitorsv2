using System.Collections.Generic;

namespace MonitorsPanel.Web.ManagerApi.Commands
{
  public class SpawnImageCommand
  {
    public string ImageId { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    public string ServerId { get; set; }
  }
}