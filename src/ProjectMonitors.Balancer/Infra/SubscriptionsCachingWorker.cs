using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectMonitors.Balancer.Domain;

namespace ProjectMonitors.Balancer.Infra
{
  public class SubscriptionsCachingWorker : BackgroundService, ISubscriptionsProvider
  {
    private static readonly TimeSpan SyncDelay = TimeSpan.FromSeconds(10);
    private readonly IMonitorSubscriptionRepository _repository;
    private readonly IPublisher _publisher;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<SubscriptionsCachingWorker> _logger;

    private ILookup<string, MonitorSubscription> _subscriptionsCache = Enumerable.Empty<MonitorSubscription>()
      .ToLookup(_ => _.Slug);

    public SubscriptionsCachingWorker(IMonitorSubscriptionRepository repository, IPublisher publisher,
      ActivitySource activitySource, ILogger<SubscriptionsCachingWorker> logger)
    {
      _repository = repository;
      _publisher = publisher;
      _activitySource = activitySource;
      _logger = logger;
    }

    public bool HasSubscribersFor(string slug) => _subscriptionsCache.Contains(slug);
    public IEnumerable<MonitorSubscription> GetSubscribersFor(string slug) => _subscriptionsCache[slug];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await FetchSettingsAsync(stoppingToken);

        await Task.Delay(SyncDelay, stoppingToken);
      }

      _logger.LogDebug("Subscriptions caching worker stopped");
    }

    public async Task FetchSettingsAsync(CancellationToken stoppingToken)
    {
      Activity.Current = null;
      using var fetchActivity = _activitySource.StartActivity("refresh_settings");
      var settings = await _repository.GetSubscriptionsAsync(stoppingToken);
      _subscriptionsCache = settings.ToLookup(_ => _.Slug);
      await _publisher.PublishStatsAsync(
        settings.Select(s => new BalancerSubscriptionEntry(s.Slug, s.DiscordWebhookUrl)), stoppingToken);
      _logger.LogDebug("Settings refreshed");
    }
  }
}