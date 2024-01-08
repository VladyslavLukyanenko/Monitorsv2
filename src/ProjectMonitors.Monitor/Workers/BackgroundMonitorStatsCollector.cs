using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Workers
{
  public class BackgroundMonitorStatsCollector : BackgroundService, IMonitorStatsCollector
  {
    private static readonly TimeSpan SyncDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan EntryLifetime = TimeSpan.FromSeconds(5);

    private readonly INotificationPublisher _publisher;
    private readonly MonitorInfo _monitorInfo;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ActivitySource _activitySource;

    private readonly BehaviorSubject<Unit> _syncActivity = new(Unit.Default);
    private readonly CompositeDisposable _disposables = new();
    private readonly ConcurrentDictionary<string, MonitorWatcherStats> _entries = new();

    public BackgroundMonitorStatsCollector(INotificationPublisher publisher, MonitorInfo monitorInfo,
      IJsonSerializer jsonSerializer, ActivitySource activitySource)
    {
      _publisher = publisher;
      _monitorInfo = monitorInfo;
      _jsonSerializer = jsonSerializer;
      _activitySource = activitySource;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var d = _syncActivity.AsObservable()
        .ObserveOn(Scheduler.Default)
        .Sample(SyncDelay)
        .Subscribe(async _ =>
        {
          Activity.Current = null;
          using var publishActivity = _activitySource.StartActivity("publish_stats");
          var keys = _entries.Keys.ToArray();
          foreach (var key in keys)
          {
            var e = _entries[key];
            if (DateTimeOffset.UtcNow > e.Timestamp + EntryLifetime)
            {
              _entries.Remove(key, out var __);
            }
          }

          var stats = new ComponentStats
          {
            Stats = await _jsonSerializer.SerializeAsync(_entries.Values, stoppingToken),
            ComponentType = "monitor",
            ComponentName = _monitorInfo.Slug
          };

          var r = await _publisher.PublishStatsAsync(stats, stoppingToken);
          publishActivity?.SetStatusIfUnset(r);
        });
      _disposables.Add(d);

      await Task.Delay(-1, stoppingToken);
    }

    public void UpdateWatcherEntry(MonitorWatcherStats e, CancellationToken ct)
    {
      _entries[e.TargetId] = e;
      _syncActivity.OnNext(Unit.Default);
    }

    public override void Dispose()
    {
      base.Dispose();
      _disposables.Dispose();
    }
  }
}