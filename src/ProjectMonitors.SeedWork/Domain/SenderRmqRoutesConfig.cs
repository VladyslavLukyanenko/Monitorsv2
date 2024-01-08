namespace ProjectMonitors.SeedWork.Domain
{
  public class SenderRmqRoutesConfig
  {
    public string SenderQueueName { get; init; } = null!;
    public string SenderRoutingKey { get; init; } = null!;
  }
}