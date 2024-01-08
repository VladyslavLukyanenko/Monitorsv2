using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using ProjectMonitors.SeedWork.Domain;
using StackExchange.Redis;

namespace ProjectMonitors.Senders.Redis
{
  public class RedisSender : ISender
  {
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<RedisSender> _logger;
    private readonly IBinarySerializer _binarySerializer;
    private readonly ConnectionMultiplexer _connection;

    public RedisSender(IJsonSerializer jsonSerializer,
      ActivitySource activitySource, ILogger<RedisSender> logger, IBinarySerializer binarySerializer,
      ConnectionMultiplexer connection)
    {
      _jsonSerializer = jsonSerializer;
      _activitySource = activitySource;
      _logger = logger;
      _binarySerializer = binarySerializer;
      _connection = connection;
    }

    public async ValueTask<Result> SendAsync(PublishPayload payload, CancellationToken ct)
    {
      using var submitActivity = _activitySource.StartActivity("submit_webhook");
      var notification = await _binarySerializer.DeserializeAsync<NotificationPayload>(
        new MemoryStream(payload.Payload), ct);
      if (notification == null)
      {
        return Result.Failure("Can't deserialize notification");
      }

      var redisNotification = await _jsonSerializer.SerializeAsync(notification, ct);
      _connection.GetDatabase().Publish(notification.Slug, new RedisValue(redisNotification));
      submitActivity?.Dispose();
      return Result.Success();
    }
  }
}