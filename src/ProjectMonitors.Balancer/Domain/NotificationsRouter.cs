using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Balancer.Domain
{
  public class NotificationsRouter : INotificationsRouter
  {
    private readonly IPublisher _publisher;
    private readonly ActivitySource _activitySource;
    private readonly ISubscriptionsProvider _subscriptionsProvider;
    private readonly ILogger<NotificationsRouter> _logger;
    private readonly IBinarySerializer _binarySerializer;

    public NotificationsRouter(IPublisher publisher, ActivitySource activitySource,
      ISubscriptionsProvider subscriptionsProvider, ILogger<NotificationsRouter> logger,
      IBinarySerializer binarySerializer)
    {
      _publisher = publisher;
      _activitySource = activitySource;
      _subscriptionsProvider = subscriptionsProvider;
      _logger = logger;
      _binarySerializer = binarySerializer;
    }

    public async ValueTask RouteAsync(NotificationPayload payload, CancellationToken ct = default)
    {
      _logger.LogDebug("Consumed notification to route");
      Activity.Current = null;
      using var consumeActivity = _activitySource.StartActivity("notification_routing");
      if (!_subscriptionsProvider.HasSubscribersFor(payload!.Slug))
      {
        var errorMessage = $"Can't find config for {payload.Slug}";
        consumeActivity?.SetStatus(Status.Error.WithDescription(errorMessage));
        _logger.LogWarning("No subscriber for monitor {MonitorSlug} notifications", payload.Slug);
        var exc = new SubscriberNotFoundException(payload.Slug);
        consumeActivity?.RecordException(exc);
        throw exc;
      }

      var subscribers = _subscriptionsProvider.GetSubscribersFor(payload!.Slug).ToArray();
      consumeActivity?.SetTag("listeners_count", subscribers.Length);
      consumeActivity?.SetTag("monitor", payload.Slug);
      consumeActivity?.SetTag("timestamp", payload.Timestamp.ToString("O"));
      try
      {
        var mem = new MemoryStream();
        await _binarySerializer.SerializeAsync(mem, payload, ct);
        var serializedPayload = mem.ToArray();
        var subscriberTasks = subscribers
          .Where(s => s.AllowedTargets.Count == 0 || s.AllowedTargets.Any(sku => payload.TargetId == sku))
          .Select(subscriber => new PublishPayload
          {
            Payload = serializedPayload,
            Subscriber = subscriber.DiscordWebhookUrl,
            Timestamp = DateTimeOffset.UtcNow
          })
          .Select(async p => await _publisher.PublishNotificationAsync(p, CancellationToken.None));

        await Task.WhenAll(subscriberTasks);
        _logger.LogDebug("Notification routed");
      }
      catch (Exception exc)
      {
        consumeActivity?.RecordException(exc);
        _logger.LogError(exc, "Error on notification routing");
        throw new CantRouteMessageException(subscribers, payload);
      }
    }
  }
}