using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.SeedWork.Infra;
using RabbitMQ.Client;

namespace ProjectMonitors.Senders.Discord
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
        .AddConfiguredCoreServices()
        .AddConfiguredTelemetry()
        .AddSingleton<ISender, HttpWebhookSender>()
        .AddHostedService<NotificationConsumer>()
        .AddHttpClient<HttpWebhookSender>(HttpWebhookSender.HttpClientName, b =>
        {
          b.BaseAddress = new Uri("https://discordapp.com/");
          b.Timeout = TimeSpan.FromSeconds(10);
        });
    }
  }
}