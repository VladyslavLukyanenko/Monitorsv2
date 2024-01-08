using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectMonitors.Balancer.Domain;
using ProjectMonitors.SeedWork.Bus.Rmq;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProjectMonitors.Balancer.Infra
{
  public class NotificationsConsumerWorker : BackgroundService
  {
    private static readonly TimeSpan ClearSendersDelay = TimeSpan.FromSeconds(3);

    private readonly INotificationsRouter _router;
    private readonly IModel _model;
    private readonly ILogger<NotificationsConsumerWorker> _logger;
    private readonly IBinarySerializer _binarySerializer;
    private readonly ISendersRegistry _sendersRegistry;

    private string? _balancerConsumerTag;
    private readonly CompositeDisposable _disposable = new();

    public NotificationsConsumerWorker(IModel model, ILogger<NotificationsConsumerWorker> logger,
      ISendersRegistry sendersRegistry, INotificationsRouter router, IBinarySerializer binarySerializer)
    {
      _model = model;
      _logger = logger;
      _sendersRegistry = sendersRegistry;
      _router = router;
      _binarySerializer = binarySerializer;

      var d = _sendersRegistry.SendersCount
        .ObserveOn(Scheduler.Default)
        .Throttle(TimeSpan.FromMilliseconds(100))
        .DistinctUntilChanged()
        .Subscribe(
          _ => ConfigsOnSendersCountChanged(_sendersRegistry.Senders.ToList()),
          exc => _logger.LogError(exc, "Error on processing senders count change")
        );
      _disposable.Add(d);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var connectConsumer = new AsyncEventingBasicConsumer(_model);
      connectConsumer.Received += ConnectConsumerOnReceived;
      connectConsumer.Shutdown += ConnectConsumerOnShutdown;
      _model.BasicConsume(RmqRoutes.BalancerConnectQueueName, false, connectConsumer);

      _ = Task.Factory.StartNew(async () =>
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          try
          {
            _sendersRegistry.ClearOutdated();
          }
          catch (TimeoutException)
          {
            // noop
          }
          // catch (AlreadyClosedException)
          // {
          // }
          catch (Exception unexpected)
          {
            _logger.LogError(unexpected, "An error on clear outdated");
          }

          await Task.Delay(ClearSendersDelay, stoppingToken);
        }
      }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);


      _logger.LogDebug("Notifications consumer started");
      await Task.Delay(-1, stoppingToken);
      _logger.LogDebug("Notifications consumer stopped");
    }

    private async Task ConsumerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
      var payload = await _binarySerializer.DeserializeAsync<NotificationPayload>(e.Body);
      if (payload == null)
      {
        _logger.LogWarning("Invalid payload received. Can't deserialize.");
        return;
      }

      try
      {
        await _router.RouteAsync(payload);
        _model.BasicAck(e.DeliveryTag, false);
      }
      catch (BalancerException exc) when (exc is SubscriberNotFoundException)
      {
        _model.BasicAck(e.DeliveryTag, false);
        _logger.LogError(exc, "No subscriptions for monitor");
      }
      catch (BalancerException exc) when (exc is CantRouteMessageException)
      {
        _model.BasicNack(e.DeliveryTag, false, true);
        _logger.LogError(exc, "Error occurred on message routing");
      }
      catch (Exception exc)
      {
        _model.BasicNack(e.DeliveryTag, false, true);
        _logger.LogCritical(exc, "Unexpected exception occurred on message routing");
      }
    }

    private void ConfigsOnSendersCountChanged(IList<SenderConfig> configs)
    {
      try
      {
        _logger.LogDebug("Senders count changed. Connected senders: {@SenderRoutingKeys}",
          configs.Select(_ => _.RoutingKey));
        if (configs.Any())
        {
          if (!string.IsNullOrEmpty(_balancerConsumerTag))
          {
            return;
          }

          ConsumeBalancerNotifications();
        }
        else
        {
          if (string.IsNullOrEmpty(_balancerConsumerTag))
          {
            return;
          }

          DisconnectNotificationsConsumer();
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Can't refresh connection state due to error");
      }


      void DisconnectNotificationsConsumer()
      {
        _model.BasicCancel(_balancerConsumerTag);
        _balancerConsumerTag = null;
        _logger.LogDebug("No active senders. Router consumer stopped");
      }

      void ConsumeBalancerNotifications()
      {
        var consumer = new AsyncEventingBasicConsumer(_model);
        consumer.Received += ConsumerOnReceived;

        _balancerConsumerTag = _model.BasicConsume(RmqRoutes.BalancerPublishQueueName, false, consumer);
        _logger.LogDebug("Received heartbeat from senders. Router consumer reconnected");
      }
    }

    private Task ConnectConsumerOnShutdown(object sender, ShutdownEventArgs @event)
    {
      return Task.CompletedTask;
    }

    private async Task ConnectConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
    {
      try
      {
        var senderCfg = await _binarySerializer.DeserializeAsync<SenderConfig>(@event.Body);
        _sendersRegistry.AddOrUpdate(senderCfg!);
        _model.BasicAck(@event.DeliveryTag, false);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Error on receiving sender config");
        _model.BasicReject(@event.DeliveryTag, false);
      }
    }

    public override void Dispose()
    {
      base.Dispose();
      _disposable.Dispose();
    }
  }
}