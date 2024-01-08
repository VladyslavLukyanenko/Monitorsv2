using System;

namespace ProjectMonitors.SeedWork.Domain
{
  public class PublishPayload
  {
    public byte[] Payload { get; set; } = null!;
    public string Subscriber { get; set; } = null!;
    public DateTimeOffset Timestamp { get; set; }
  }
}