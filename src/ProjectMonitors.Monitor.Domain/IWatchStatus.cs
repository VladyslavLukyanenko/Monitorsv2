using System;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IWatchStatus
  {
    bool IsAvailable { get; }
    string TargetId { get; }
    public TimeSpan? DelayRequest { get; }
  }
}