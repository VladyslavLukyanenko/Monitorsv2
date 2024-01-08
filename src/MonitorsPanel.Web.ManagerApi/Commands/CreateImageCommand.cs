using System.Collections.Generic;
using MonitorsPanel.Core.Manager;

namespace MonitorsPanel.Web.ManagerApi.Commands
{
  public class CreateImageCommand
  {
    public string ImageName { get; set; }
    public string Slug { get; set; }
    public ImageType ImageType { get; set; }
    public List<string> RequiredSpawnParameters { get; set; } = new List<string>();
  }
}