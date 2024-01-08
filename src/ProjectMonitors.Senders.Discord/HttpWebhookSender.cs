using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Data.Discord;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Senders.Discord
{
  public class HttpWebhookSender : ISender
  {
    public const string HttpClientName = "Discord";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<HttpWebhookSender> _logger;
    private readonly IBinarySerializer _binarySerializer;

    public HttpWebhookSender(IHttpClientFactory httpClientFactory, IJsonSerializer jsonSerializer,
      ActivitySource activitySource, ILogger<HttpWebhookSender> logger, IBinarySerializer binarySerializer)
    {
      _httpClientFactory = httpClientFactory;
      _jsonSerializer = jsonSerializer;
      _activitySource = activitySource;
      _logger = logger;
      _binarySerializer = binarySerializer;
    }

    public async ValueTask<Result> SendAsync(PublishPayload payload, CancellationToken ct)
    {
      while (true)
      {
        using var submitActivity = _activitySource.StartActivity("submit_webhook");
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var notification = await _binarySerializer.DeserializeAsync<NotificationPayload>(
          new MemoryStream(payload.Payload), ct);
        if (notification == null)
        {
          return Result.Failure("Can't deserialize notification");
        }

        var formattedMessage = FormatMessage(notification);

        var webhookPayload = await _jsonSerializer.SerializeAsync(formattedMessage, ct);
        var message = new HttpRequestMessage(HttpMethod.Post, payload.Subscriber)
        {
          Content = new StringContent(webhookPayload, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead, ct);
        submitActivity?.SetHttpRequestMessage(message);
        submitActivity?.SetHttpResponseMessage(response);
        if (!response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsStringAsync(ct);
          submitActivity?.SetTag("response", responseContent);
          if (response.StatusCode == HttpStatusCode.TooManyRequests)
          {
            var err = await _jsonSerializer.DeserializeAsync<DiscordRateLimitedError>(responseContent, ct);

            var delay = TimeSpan.FromMilliseconds(err!.RetryAfter);
            submitActivity?.SetTag("rate_limited", delay);
            using (var rld = _activitySource.StartActivity("rate_limit_delay"))
            {
              _logger.LogWarning("Rate limited for {Delay}", delay);
              rld?.SetTag("rate_limited_for", delay);
              await Task.Delay(delay, ct);
            }

            submitActivity?.Dispose();
            continue;
          }

          if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Unauthorized)
          {
            submitActivity.SetStatus(Status.Error.WithDescription("Invalid webhook url."));
            submitActivity?.Dispose();
            _logger.LogError(
              "Invalid webhook url. Discord respond with {StatusCode}. Probably it was removed or typo in url. URL: {WebhookURL}",
              response.StatusCode, payload.Subscriber);
          }

          return Result.Failure("failed to submit webhook");
        }

        return Result.Success();
      }
    }

    private DiscordWebhookBody FormatMessage(NotificationPayload n)
    {
      var productSummary = n.Payload;
      var embed = new Embed {Title = productSummary.Title};
      var body = new DiscordWebhookBody
      {
        Username = "Monitor " + n.Slug,
        Embeds = {embed}
      };

      if (!string.IsNullOrEmpty(productSummary.Picture))
      {
        body.AvatarUrl = productSummary.Picture;
        embed.Thumbnail.Url = productSummary.Picture;
      }

      if (!string.IsNullOrEmpty(n.ShopIconUrl))
      {
        body.AvatarUrl = n.ShopIconUrl;
        embed.Author.IconUrl = n.ShopIconUrl;
      }

      if (!string.IsNullOrEmpty(n.ShopTitle))
      {
        embed.Author.Name = n.ShopTitle;
      }

      if (productSummary.PageUrl != null)
      {
        embed.Url = productSummary.PageUrl.ToString();
      }

      if (!string.IsNullOrEmpty(productSummary.Price))
      {
        embed.Fields.Add(new Field
        {
          Name = "Price",
          Value = productSummary.Price
        });
      }

      embed.Fields.Add(new Field
      {
        Name = "SKU",
        Value = productSummary.Sku
      });

      foreach (var attr in productSummary.Attributes)
      {
        embed.Fields.Add(new Field {Name = attr.Name, Value = attr.Value});
      }

      foreach (var link in productSummary.Links)
      {
        embed.Fields.Add(new Field {Name = link.Text, Value = $"[Link]({link.Url})"});
      }

      return body;
    }
  }
}