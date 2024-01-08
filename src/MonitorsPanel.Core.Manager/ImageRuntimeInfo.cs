using System;

namespace MonitorsPanel.Core.Manager
{
  public class ImageRuntimeInfo
  {
    public ImageRuntimeInfo(ServerInstance instance, ImageInfo imageInfo, string containerId, string status,
      string state, DateTimeOffset createdAt)
    {
      ImageInfo = imageInfo;
      ServerInstance = instance;
      ContainerId = containerId;
      Status = status;
      State = state;
      CreatedAt = createdAt;
    }

    public ImageInfo ImageInfo { get; }
    public ServerInstance ServerInstance { get; }

    public string ContainerId { get; }
    public string Status { get; }
    public string State { get; }

    public DateTimeOffset CreatedAt { get; }
    // public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
  }
}