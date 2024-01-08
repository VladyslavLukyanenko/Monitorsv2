using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectMonitors.SeedWork.Data.Discord;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.SeedWork.Data
{
  public class DiscordNotificationPayloadFactory : INotificationPayloadFactory
  {
    private readonly Func<DiscordWebhookBody> _payloadFactory;
    private readonly IJsonSerializer _jsonSerializer;

    public DiscordNotificationPayloadFactory(Func<DiscordWebhookBody> payloadFactory, IJsonSerializer jsonSerializer)
    {
      _payloadFactory = payloadFactory;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<string> ToJsonAsync(CancellationToken ct)
    {
      var data = _payloadFactory();
      return await _jsonSerializer.SerializeAsync(data, ct);
    }
  }
}