using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.Workers
{
  public class MonitorSettingsFetchWorker : BackgroundService, IMonitorSettingsService
  {
    private static readonly TimeSpan RefreshDelay = TimeSpan.FromSeconds(5);

    private readonly BehaviorSubject<MonitorSettings?> _settings = new(null);
    private readonly MonitorInfo _monitorInfo;
    private readonly IMonitorSettingsRepository _monitorSettingsRepository;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<MonitorSettingsFetchWorker> _logger;

    public MonitorSettingsFetchWorker(MonitorInfo monitorInfo, IMonitorSettingsRepository monitorSettingsRepository,
      ActivitySource activitySource, ILogger<MonitorSettingsFetchWorker> logger)
    {
      _monitorInfo = monitorInfo;
      _monitorSettingsRepository = monitorSettingsRepository;
      _activitySource = activitySource;
      _logger = logger;
      Settings = _settings.AsObservable()
        .Where(s => s != null)
        .DistinctUntilChanged(_ => _!.UpdatedAt)
        .Replay()
        .RefCount()!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          Activity.Current = null;
          using var activity = _activitySource.StartActivity("refresh_monitor_settings");
          var settings = await _monitorSettingsRepository.GetSettingsAsync(_monitorInfo, stoppingToken);
          _settings.OnNext(settings);

          activity?.Dispose();
          await Task.Delay(RefreshDelay, stoppingToken);
        }
        catch (Exception exc)
        {
          _logger.LogError(exc, "Can't fetch settings");
        }
      }
    }

    public IObservable<MonitorSettings> Settings { get; }

    public async ValueTask<MonitorSettings?> UpdateTargetsIfNotChangedAsync(MonitorInfo monitorInfo,
      DateTimeOffset targetChanges, IEnumerable<WatchTarget> targets, CancellationToken ct = default)
    {
      if (!await _monitorSettingsRepository.UpdateTargetsIfNotChangedAsync(monitorInfo, targetChanges, targets, ct))
      {
        return null;
      }

      return await _monitorSettingsRepository.GetSettingsAsync(_monitorInfo, ct);
    }
  }
}