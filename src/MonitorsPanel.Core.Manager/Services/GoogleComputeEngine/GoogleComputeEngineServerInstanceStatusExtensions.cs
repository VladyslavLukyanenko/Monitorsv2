namespace MonitorsPanel.Core.Manager.Services.GoogleComputeEngine
{
  public static class GoogleComputeEngineServerInstanceStatusExtensions
  {
    public static ServerInstanceStatus ToGoogleComputeEngineInstanceStatus(this string status) => status switch
    {
      _ when status == "PROVISIONING" || status == "STAGING" => ServerInstanceStatus.Pending, 
      "RUNNING" => ServerInstanceStatus.Running,
      "STOPPING" => ServerInstanceStatus.Stopping,
      "TERMINATED" => ServerInstanceStatus.Stopped,
      _ => ServerInstanceStatus.Unknown,
    };
  }
}