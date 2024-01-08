using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.SeedWork.Infra;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace ProjectMonitors.Senders.Redis
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services, HostBuilderContext hostCtx)
    {
      services
        .Configure<SenderRmqRoutesConfig>(hostCtx.Configuration.GetSection("SenderRmqRoutes"))
        .AddSingleton(ctx => ctx.GetRequiredService<IOptions<SenderRmqRoutesConfig>>().Value)
        .AddConfiguredRabbitMq((ctx, model) =>
        {
          var rmqRoutesConfig =  ctx.GetRequiredService<SenderRmqRoutesConfig>();
          model.ExchangeDeclare(RmqRoutes.SenderPublishExchangeName, "direct", true);

          var senderQueue = model.QueueDeclare(rmqRoutesConfig.SenderQueueName, true, false, false);
          model.QueueBind(senderQueue.QueueName, RmqRoutes.SenderPublishExchangeName, rmqRoutesConfig.SenderRoutingKey);
        })
        .AddSingleton(_ => ConnectionMultiplexer.Connect(hostCtx.Configuration.GetConnectionString("Redis")))
        .AddConfiguredCoreServices()
        .AddConfiguredTelemetry()
        .AddSingleton<ISender, RedisSender>()
        .AddHostedService<NotificationConsumer>();
    }
  }
}