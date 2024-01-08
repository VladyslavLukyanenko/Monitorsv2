using System;

namespace ProjectMonitors.Monitor.Domain
{
  public class WatchStatus : IWatchStatus
  {
    public WatchStatus(string targetId, bool isAvailable, TimeSpan? delayRequest = null)
    {
      IsAvailable = isAvailable;
      TargetId = targetId;
      DelayRequest = delayRequest;
    }

    public bool IsAvailable { get; }
    public string TargetId { get; }
    public TimeSpan? DelayRequest { get; }
  }
}