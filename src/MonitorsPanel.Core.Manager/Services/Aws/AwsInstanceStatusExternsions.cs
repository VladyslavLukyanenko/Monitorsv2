namespace MonitorsPanel.Core.Manager.Services.Aws
{
  public static class AwsInstanceStatusExtensions
  {
    public static ServerInstanceStatus ToAwsInstanceStatus(this string status) => status switch
    {
      "pending" => ServerInstanceStatus.Pending,
      "running" => ServerInstanceStatus.Running,
      "shutting-down" => ServerInstanceStatus.ShuttingDown,
      "terminated" => ServerInstanceStatus.Terminated,
      "stopping" => ServerInstanceStatus.Stopping,
      "stopped" => ServerInstanceStatus.Stopped,
      _ => ServerInstanceStatus.Unknown,
    };
  }
}