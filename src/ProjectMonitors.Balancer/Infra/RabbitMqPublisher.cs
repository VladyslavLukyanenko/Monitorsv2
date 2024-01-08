using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProjectMonitors.Balancer.Domain;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using RabbitMQ.Client;

namespace ProjectMonitors.Balancer.Infra
{
  public class RabbitMqPublisher : IPublisher
  {
    private readonly IModel _model;
    private readonly IBasicProperties _props;
    private readonly IBasicProperties _statsProps;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ActivitySource _activitySource;
    private readonly ISendersProvider _sendersProvider;
    private readonly IBinarySerializer _binarySerializer;

    public RabbitMqPublisher(IModel model, IJsonSerializer jsonSerializer, ActivitySource activitySource,
      ISendersProvider sendersProvider, IBinarySerializer binarySerializer)
    {
      _model = model;
      _jsonSerializer = jsonSerializer;
      _activitySource = activitySource;
      _sendersProvider = sendersProvider;
      _binarySerializer = binarySerializer;
      _props = model.CreateBasicProperties();
      _props.ContentType = "application/x-msgpack";
      _props.Persistent = true;

      _statsProps = model.CreateBasicProperties();
      _statsProps.ContentType = "application/json";
      _statsProps.Persistent = true;
    }

    public async ValueTask PublishNotificationAsync(PublishPayload payload, CancellationToken ct)
    {
      using var publishActivity = _activitySource.StartActivity("publish_notification");
      publishActivity?.SetTag("webhook_url", payload.Subscriber);
      var mem = new MemoryStream();
      await _binarySerializer.SerializeAsync(mem, payload, ct);
      var serializedPayload = mem.ToArray();
      foreach (var senderConfig in _sendersProvider.Senders)
      {
        _model.BasicPublish(RmqRoutes.SenderPublishExchangeName, senderConfig.RoutingKey, _props, serializedPayload);
      }
    }

    public async ValueTask PublishStatsAsync(IEnumerable<BalancerSubscriptionEntry> entries, CancellationToken ct)
    {
      using var publishActivity = _activitySource.StartActivity("publish_stats");
      var mem = new MemoryStream();
      await _jsonSerializer.SerializeAsync(mem, new ComponentStats
      {
        ComponentName = "balancer",
        ComponentType = "balancer",
        Stats = await _jsonSerializer.SerializeAsync(entries, ct)
      }, ct);
      _model.BasicPublish(RmqRoutes.ComponentExchangeName, "", _statsProps, mem.ToArray());
    }
  }
}