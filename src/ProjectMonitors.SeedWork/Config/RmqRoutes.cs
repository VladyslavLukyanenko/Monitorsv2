#nullable disable

namespace ProjectMonitors.SeedWork.Config
{
  public static class RmqRoutes
  {
    public const string ScraperBalancerRoutingKey = "scrapers.discord_balancer";

    public const string ComponentExchangeName = "component_stats";

    public const string SenderPublishExchangeName = "senders.publish";


    public const string BalancerPublishRoutingKey = "balancer.publish";
    public const string BalancerPublishExchangeName = "balancer.publish";
    public const string BalancerPublishQueueName = "balancer.publish_queue";

    public const string BalancerConnectRoutingKey = "balancer.connect";
    public const string BalancerConnectExchangeName = "balancer.connect";
    public const string BalancerConnectQueueName = "balancer.connect_queue";
  }
}