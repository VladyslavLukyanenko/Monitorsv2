namespace ProjectMonitors.Balancer.Domain
{
  public class SubscriberNotFoundException : BalancerException
  {
    public SubscriberNotFoundException(string monitorSlug)
      : base($"Subscriber not found for monitor \"{monitorSlug}\"")
    {
      MonitorSlug = monitorSlug;
    }

    public string MonitorSlug { get; }
  }
}