using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Domain
{
  public class WatchersManager : IWatchersManager
  {
    private readonly IList<IWatcher> _watchers = new List<IWatcher>();

    private readonly IProductRepository _productRepository;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IWatcherFactory _watcherFactory;
    private readonly IMonitorStatsCollector _statsCollector;
    private readonly ActivitySource _activitySource;

    private readonly SemaphoreSlim _statusChangeGates = new(1, 1);
    private CancellationTokenSource _watchersCts = new();
    private readonly ILogger<WatchersManager> _logger;
    private MonitorSettings? _settings;

    public WatchersManager(IProductRepository productRepository, INotificationPublisher notificationPublisher,
      IWatcherFactory watcherFactory, IMonitorStatsCollector statsCollector, ActivitySource activitySource,
      ILogger<WatchersManager> logger)
    {
      _productRepository = productRepository;
      _notificationPublisher = notificationPublisher;
      _watcherFactory = watcherFactory;
      _statsCollector = statsCollector;
      _activitySource = activitySource;
      _logger = logger;
    }

    public async ValueTask DisposeAsync()
    {
      await StopAllAsync(CancellationToken.None);
    }

    public async ValueTask SpawnWatcherAsync(WatchTarget watchTarget, ProductStatus initialStatus,
      CancellationToken ct)
    {
      _logger.LogDebug(
        "Spawning watcher for target '{Target}', initial status {InitialStatus}, instances count {InstancesCount}",
        watchTarget.Input, initialStatus, watchTarget.WatchersCount);
      using var spawnActivity = _activitySource.StartActivity("spawn_watcher");
      spawnActivity?.SetTag("status", initialStatus);
      spawnActivity?.SetTag("url", watchTarget.Input);

      if (watchTarget.Products.Count == 0)
      {
        _logger.LogWarning("Nothing to spawn. No products defined");
        spawnActivity?.SetTag("nothing_to_spawn", "no_products_defined");
        return;
      }

      var statuses = await GetSpawnStatusesAsync(watchTarget, initialStatus, ct);
      var watcher = _watcherFactory.Create(watchTarget, statuses);
      watcher.StatusChanged += WatcherOnStatusChanged;
      _watchers.Add(watcher);
      var __ = Task.Factory.StartNew(async () =>
        {
          try
          {
            await watcher.SpawnAsync(watchTarget, _watchersCts.Token);
          }
          catch (Exception exc)
          {
            Activity.Current = null;
            using var spanWatcherErrorActivity = _activitySource.StartActivity("spawn_error");
            spanWatcherErrorActivity?.RecordException(exc);
            _logger.LogCritical(exc, "Error on spawn watcher");
            throw;
          }
        }, ct,
        TaskCreationOptions.LongRunning, TaskScheduler.Default);
      _logger.LogDebug("Target '{Target}' spawned", watchTarget.Input);
    }

    private async ValueTask<Dictionary<string, ProductStatus>> GetSpawnStatusesAsync(WatchTarget watchTarget,
      ProductStatus initialStatus, CancellationToken ct)
    {
      var targets = watchTarget.Products.Keys;
      var statuses = new Dictionary<string, ProductStatus>(targets.Count);
      foreach (var trg in targets)
      {
        var status = initialStatus;
        var prodRef = await _productRepository.GetRefAsync(trg, ct);
        if (prodRef == null)
        {
          await _productRepository.CreateAsync(new ProductRef(trg, initialStatus), ct);
        }
        else
        {
          status = prodRef.Status;
        }

        statuses[trg] = status;
      }

      return statuses;
    }

    public async ValueTask StopAllAsync(CancellationToken ct)
    {
      _watchersCts.Cancel();
      foreach (var watcher in _watchers)
      {
        watcher.StatusChanged -= WatcherOnStatusChanged;
        await watcher.DisposeAsync();
      }

      _watchers.Clear();
      _watchersCts = new CancellationTokenSource();
    }

    public void UseSettings(MonitorSettings settings)
    {
      _settings = settings;
    }

    private async ValueTask WatcherOnStatusChanged(object? sender, WatcherStatusChangedEventArgs e)
    {
      using var statusChangeActivity = _activitySource.StartActivity("status_change_handle");
      statusChangeActivity?.SetTag("curr_status", e.Curr);
      statusChangeActivity?.SetTag("prev_status", e.Prev);
      var target = e.Status;
      _statsCollector.UpdateWatcherEntry(MonitorWatcherStats.FromTarget(e.Spec, target), e.CancellationToken);
      if (e.Curr == e.Prev)
      {
        statusChangeActivity?.SetTag("nothing_change", bool.TrueString);
        await DelayAsync(e.Status, e.Curr);

        return;
      }

      try
      {
        await _statusChangeGates.WaitAsync(CancellationToken.None);
        if (!await _productRepository.AreStatusChangedAsync(target.TargetId, e.Curr, e.CancellationToken))
        {
          statusChangeActivity?.SetTag("stored_with_same_status", bool.TrueString);
          return;
        }

        var summary = e.Spec.Products[e.Status.TargetId];
        using var processingActivity = TraceChanges(e.Status, summary);

        var result = await _notificationPublisher.PublishAsync(target.TargetId, e.Curr, e.Spec, e.CancellationToken);

        processingActivity?.SetStatusIfUnset(result);
        await _productRepository.ChangeStatusAsync(target.TargetId, e.Curr, e.CancellationToken);
        _logger.LogDebug("Target's {Target} status changed from {PrevStatus} to {NewStatus}", target.TargetId, e.Prev,
          e.Curr);
      }
      finally
      {
        _statusChangeGates.Release();
      }
    }

    private Activity? TraceChanges(IWatchStatus watchStatus, ProductSummary summary)
    {
      var processingActivity = _activitySource.StartActivity("processing");
      processingActivity?.SetTag("title", summary.Title);
      processingActivity?.SetTag("id", watchStatus.TargetId);
      processingActivity?.SetTag("pic", summary.Picture);
      processingActivity?.SetTag("url", summary.PageUrl);
      processingActivity?.SetTag("is_available", watchStatus.IsAvailable);
      return processingActivity;
    }

    private async Task DelayAsync(IWatchStatus watchStatus, ProductStatus currProdStatus)
    {
      using var delayActivity = _activitySource.StartActivity("delay");
      TimeSpan delay;
      if (watchStatus.DelayRequest.HasValue)
      {
        delayActivity?.SetTag("delay_requested", bool.TrueString);
        delay = watchStatus.DelayRequest.Value;
      }
      else
      {
        delay = currProdStatus == ProductStatus.Available
          ? _settings?.DelayOnAvailable ?? MonitorSettings.DefaultDelayOnAvailable
          : _settings?.DelayOnUnavailable ?? MonitorSettings.DefaultDelayOnUnavailable;
      }

      if (delay != TimeSpan.Zero)
      {
        delayActivity?.SetTag("delay", delay.ToString());
        await Task.Delay(delay);
      }
    }
  }
}