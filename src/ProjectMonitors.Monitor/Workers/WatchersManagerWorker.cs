using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.Workers
{
  public class WatchersManagerWorker : BackgroundService
  {
    private static readonly TimeSpan SettingsSyncDelay = TimeSpan.FromSeconds(1);

    private readonly IMonitorSettingsService _monitorSettingsService;
    private readonly IWatchersManager _watchersManager;
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<WatchersManagerWorker> _logger;
    private readonly MonitorInfo _monitorInfo;

    private MonitorSettings? _lastSettings;
    private IEnumerable<WatchTarget>? _lastTargets;

    public WatchersManagerWorker(IMonitorSettingsService monitorSettingsService, IWatchersManager watchersManager,
      IMonitorHttpClientFactory monitorHttpClientFactory, ActivitySource activitySource,
      ILogger<WatchersManagerWorker> logger, MonitorInfo monitorInfo)
    {
      _monitorSettingsService = monitorSettingsService;
      _watchersManager = watchersManager;
      _monitorHttpClientFactory = monitorHttpClientFactory;
      _activitySource = activitySource;
      _logger = logger;
      _monitorInfo = monitorInfo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _monitorSettingsService.Settings
        .ObserveOn(ThreadPoolScheduler.Instance)
        .Where(s => s.UpdatedAt != _lastSettings?.UpdatedAt)
        .Subscribe(async newSettings => await RefreshSpawnedWorkers(newSettings, stoppingToken));

      _ = Task.Factory.StartNew(async () =>
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          try
          {
            var targets = _lastTargets;
            var settings = _lastSettings;
            if (targets == null || settings == null)
            {
              continue;
            }

            var changed = await _monitorSettingsService.UpdateTargetsIfNotChangedAsync(_monitorInfo, settings.UpdatedAt,
              targets, stoppingToken);
            if (changed != null)
            {
              _lastSettings = changed;
            }

            await Task.Delay(SettingsSyncDelay, stoppingToken);
          }
          catch (Exception exc)
          {
            _logger.LogError(exc, "Failed to sync settings");
          }
        }
      }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      await Task.Delay(-1, stoppingToken);
    }

    private async ValueTask RefreshSpawnedWorkers(MonitorSettings newSettings, CancellationToken ct)
    {
      _lastSettings = newSettings;
      _lastTargets = newSettings.Targets;
      Activity.Current = null;
      using var applySettingsActivity = _activitySource.StartActivity("apply_monitor_settings");
      await _watchersManager.StopAllAsync(ct);
      _monitorHttpClientFactory.Configure(newSettings);

      _watchersManager.UseSettings(newSettings);
      if (newSettings.Targets.Count == 0)
      {
        _logger.LogWarning("Nothing to spawn. No targets defined");
        applySettingsActivity?.SetTag("nothing_to_spawn", "no_targets_defined");
        return;
      }

      foreach (var target in newSettings.Targets)
      {
        await _watchersManager.SpawnWatcherAsync(target, newSettings.InitialStatus, ct);
      }
    }
  }
}