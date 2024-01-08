using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.Monitor.Domain;
using RabbitMQ.Client;

namespace ProjectMonitors.Monitor.Infra
{
  public class RabbitMqNotificationPublisher : INotificationPublisher
  {
    private readonly MonitorInfo _monitorInfo;
    private readonly IModel _channel;
    private readonly IBasicProperties _publishProps;
    private readonly IBasicProperties _statsProps;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ActivitySource _activitySource;
    private readonly IBinarySerializer _binarySerializer;
    private readonly ILogger<RabbitMqNotificationPublisher> _logger;

    public RabbitMqNotificationPublisher(MonitorInfo monitorInfo, IModel channel, IJsonSerializer jsonSerializer,
      ActivitySource activitySource, IBinarySerializer binarySerializer, ILogger<RabbitMqNotificationPublisher> logger)
    {
      _monitorInfo = monitorInfo;
      _channel = channel;
      _jsonSerializer = jsonSerializer;
      _activitySource = activitySource;
      _binarySerializer = binarySerializer;
      _logger = logger;

      _publishProps = _channel.CreateBasicProperties();
      _publishProps.ContentType = "application/x-msgpack";
      _publishProps.Persistent = true;

      _statsProps = _channel.CreateBasicProperties();
      _statsProps.ContentType = "application/json";
      _statsProps.Persistent = true;
    }

    public async ValueTask<Result> PublishAsync(string targetId, ProductStatus status, WatchTarget spec,
      CancellationToken ct)
    {
      if (status == ProductStatus.Unavailable)
      {
        return Result.Success();
      }

      using var publishActivity = _activitySource.StartActivity("publish");
      publishActivity?.SetTag("status", status);
      publishActivity?.SetTag("url", targetId);

      var summary = spec.Products[targetId];
      var notification = new NotificationPayload
      {
        Payload = summary,
        Slug = _monitorInfo.Slug,
        ShopTitle = spec.ShopTitle,
        ShopIconUrl = spec.ShopIconUrl,
        TargetId = targetId
      };

      using var sendActivity = _activitySource.StartActivity("send");
      var mem = new MemoryStream();
      await _binarySerializer.SerializeAsync(mem, notification, ct);
      var bytes = mem.ToArray();
      if (sendActivity is not null)
      {
        sendActivity.SetTag("msgpack", Convert.ToBase64String(bytes));
      }

      _channel.BasicPublish(RmqRoutes.BalancerPublishExchangeName, RmqRoutes.BalancerPublishRoutingKey, _publishProps,
        bytes);
      
      _logger.LogDebug("Notification published");

      return Result.Success();
    }

    public async ValueTask<Result> PublishStatsAsync(ComponentStats stats, CancellationToken ct)
    {
      var mem = new MemoryStream();
      await _jsonSerializer.SerializeAsync(mem, stats, ct);
      _channel.BasicPublish(RmqRoutes.ComponentExchangeName, "", false, _statsProps, mem.ToArray());

      return Result.Success();
    }
  }
}