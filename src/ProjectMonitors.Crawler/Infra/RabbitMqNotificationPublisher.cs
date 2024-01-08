using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.Crawler.Domain;
using RabbitMQ.Client;

namespace ProjectMonitors.Crawler.Infra
{
  public class RabbitMqNotificationPublisher : INotificationPublisher
  {
    private readonly StoreInfo _storeInfo;
    private readonly IModel _channel;
    private readonly IBasicProperties _publishProps;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ActivitySource _activitySource;

    public RabbitMqNotificationPublisher(StoreInfo storeInfo, IModel channel, IJsonSerializer jsonSerializer,
      ActivitySource activitySource)
    {
      _storeInfo = storeInfo;
      _channel = channel;
      _jsonSerializer = jsonSerializer;
      _activitySource = activitySource;
      _publishProps = _channel.CreateBasicProperties();
      _publishProps.ContentType = "application/json";
      _publishProps.Persistent = true;
    }

    public async ValueTask<Result> PublishAsync(string targetId, INotificationPayloadFactory payloadFactory,
      CancellationToken ct)
    {
      throw new NotImplementedException();
      // using var publishActivity = _activitySource.StartActivity("publish");
      // publishActivity?.SetTag("store", targetId);
      //
      // NotificationPayload notification;
      // using (var jsonActivity = _activitySource.StartActivity("json"))
      // {
      //   var json = await payloadFactory.ToJsonAsync(ct);
      //   jsonActivity?.SetTag("json", json);
      //   notification = new NotificationPayload(_monitorInfo.Id, targetId, json);
      // }
      //
      // using var sendActivity = _activitySource.StartActivity("send");
      // var mem = new MemoryStream();
      // await _jsonSerializer.SerializeAsync(mem, notification, ct);
      // _channel.BasicPublish(RmqRoutes.BalancerPublishExchangeName, RmqRoutes.ScraperBalancerRoutingKey, _publishProps, mem.ToArray());
      //
      // return Result.Success();
    }

    public async ValueTask<Result> PublishStatsAsync(ComponentStats stats, CancellationToken ct)
    {
      var mem = new MemoryStream();
      await _jsonSerializer.SerializeAsync(mem, stats, ct);
      _channel.BasicPublish(RmqRoutes.ComponentExchangeName, "", false, _publishProps, mem.ToArray());

      return Result.Success();
    }
  }
}