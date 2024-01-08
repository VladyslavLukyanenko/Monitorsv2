namespace MonitorsPanel.Web.ManagerApi.Commands
{
  public class StartOrRunServerInstanceCommand
  {
    public string ProviderName { get; set; }
    public string StoppedInstanceId { get; set; }
  }
}