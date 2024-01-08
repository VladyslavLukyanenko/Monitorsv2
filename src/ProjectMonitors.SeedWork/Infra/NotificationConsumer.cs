using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork.Bus.Rmq;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProjectMonitors.SeedWork.Infra
{
  public class NotificationConsumer : BackgroundService
  {
    private static readonly TimeSpan SenderHeartbeatDelay = TimeSpan.FromMilliseconds(500);

    private readonly IModel _model;
    private readonly ISender _sender;
    private readonly IBinarySerializer _binarySerializer;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly SenderRmqRoutesConfig _routes;

    public NotificationConsumer(IModel model, ISender sender, IBinarySerializer binarySerializer,
      ActivitySource activitySource, ILogger<NotificationConsumer> logger, SenderRmqRoutesConfig routes)
    {
      _model = model;
      _sender = sender;
      _binarySerializer = binarySerializer;
      _activitySource = activitySource;
      _logger = logger;
      _routes = routes;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var consumer = new AsyncEventingBasicConsumer(_model);
      consumer.Received += ConsumerOnReceived;
      _model.BasicConsume(_routes.SenderQueueName, false, consumer);

      SpawnHeartbeatPublisherAsync(stoppingToken);

      _logger.LogDebug("Notification consumer started");
      await Task.Delay(-1, stoppingToken);
      _logger.LogDebug("Notification consumer stopped");
    }

    private void SpawnHeartbeatPublisherAsync(CancellationToken stoppingToken)
    {
      var props = _model.CreateBasicProperties();
      props.Persistent = true;
      props.ContentType = "application/x-msgpack";
      props.ContentEncoding = Encoding.UTF8.EncodingName;
      _ = Task.Factory.StartNew(async () =>
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          var jsonmem = new MemoryStream();
          var config = new SenderConfig
          {
            RoutingKey = _routes.SenderRoutingKey
          };
          await _binarySerializer.SerializeAsync(jsonmem, config, stoppingToken);
          _model.BasicPublish(RmqRoutes.BalancerConnectExchangeName, RmqRoutes.BalancerConnectRoutingKey, props,
            jsonmem.ToArray());

          await Task.Delay(SenderHeartbeatDelay, stoppingToken);
        }
      }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
    {
      _logger.LogDebug("Received notification");
      Activity.Current = null;
      using var consumeActivity = _activitySource.StartActivity("notification");
      try
      {
        var message = await _binarySerializer.DeserializeAsync<PublishPayload>(@event.Body);
        if (message == null)
        {
          _logger.LogWarning("Can't deserialize payload notification");
          return;
        }

        consumeActivity?.SetTag("subscriber", message!.Subscriber);
        consumeActivity?.SetTag("timestamp", message.Timestamp.ToString("O"));
        consumeActivity?.SetTag("payload", message.Payload);

        var result = await _sender.SendAsync(message, CancellationToken.None);
        consumeActivity?.SetStatusIfUnset(result);
        consumeActivity?.SetTag("delivery_time", (DateTimeOffset.UtcNow - message.Timestamp).ToString("c"));

        // need to think what to do with bad messages
        if (result.IsSuccess)
        {
          _model.BasicAck(@event.DeliveryTag, false);
        }
        else
        {
          _model.BasicReject(@event.DeliveryTag, false);
        }

        _logger.LogDebug("Notification processed");
      }
      catch (Exception exc)
      {
        consumeActivity.RecordException(exc);
        _logger.LogError(exc, "Error on notification sending");
        _model.BasicNack(@event.DeliveryTag, false, true);
      }
    }
  }
}