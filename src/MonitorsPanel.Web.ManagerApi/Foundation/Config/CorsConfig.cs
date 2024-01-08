using System.Collections.Generic;

namespace MonitorsPanel.Web.ManagerApi.Foundation.Config
{
  public class CorsConfig
  {
    public bool UseCors { get; set; }
    public List<string> AllowedHosts { get; set; }
  }
}