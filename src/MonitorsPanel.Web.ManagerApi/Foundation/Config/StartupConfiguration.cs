namespace MonitorsPanel.Web.ManagerApi.Foundation.Config
{
  public class StartupConfiguration
  {
    public bool UseHttps { get; set; }
    public string AllowedHosts { get; set; } = null!;
  }
}