using System;

namespace MonitorsPanel.Core.Manager
{
  public static class HostingTargetsExtensions
  {
    public static SupportedHostingTargets ToSupportedTargets(this ImageType self) => self switch
    {
      ImageType.Monitor => SupportedHostingTargets.Monitors,
      ImageType.Publisher => SupportedHostingTargets.Publishers,
      _ => throw new ArgumentOutOfRangeException()
    };
  }
}