using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Domain
{
  public class GenericWatcher : IWatcher
  {
    private readonly WatchTarget _target;
    private readonly IProductStatusFetcherFactory _productStatusFetcherFactory;
    private readonly int _targetsCount;
    private readonly IDictionary<string, ProductStatus> _statuses;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<GenericWatcher> _logger;

    private CancellationTokenSource? _cts;

    public GenericWatcher(WatchTarget target, IProductStatusFetcherFactory productStatusFetcherFactory,
      int targetsCount, IDictionary<string, ProductStatus> statuses, ActivitySource activitySource,
      ILogger<GenericWatcher> logger)
    {
      _target = target;
      _productStatusFetcherFactory = productStatusFetcherFactory;
      _targetsCount = targetsCount;
      _statuses = statuses;
      _activitySource = activitySource;
      _logger = logger;
    }

    public ValueTask DisposeAsync()
    {
      Cancel();

      return default;
    }

    public async ValueTask SpawnAsync(WatchTarget target, CancellationToken ct)
    {
      Cancel();
      _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
      var fetchers = Enumerable.Range(0, target.WatchersCount)
        .Select(_ => _productStatusFetcherFactory.CreateFetcher(_target))
        .ToArray();

      var sharedGates = new SemaphoreSlim(1, 1);
      while (!ct.IsCancellationRequested)
      {
        Activity.Current = null;
        using var checkActivity = _activitySource.StartActivity("check_status");
        try
        {
          await CheckStatusAsync(target, fetchers, sharedGates, _cts.Token);
        }
        catch (Exception exc)
        {
          checkActivity?.RecordException(exc);
          if (exc is OperationCanceledException)
          {
            _logger.LogWarning(exc, "Operation cancelled");
          }
          else
          {
            _logger.LogCritical(exc, "An error occurred on check status");
          }
        }
        finally
        {
          checkActivity?.Dispose();
        }
      }

      _logger.LogDebug("Watcher {FetchUrl} finished execution", _target.Input);
    }

    private async Task CheckStatusAsync(WatchTarget target, IEnumerable<IProductStatusFetcher> fetchers,
      SemaphoreSlim sharedGates, CancellationToken ct)
    {
      var responsesDict = new ConcurrentDictionary<string, ProductStatus>();
      var pendingChecks = fetchers.Select((f, idx) =>
      {
        return CheckAvailabilityAsync(idx, f);

        async Task CheckAvailabilityAsync(int ix, IProductStatusFetcher fetcher)
        {
          using var fetcherActivity = _activitySource.StartActivity("check_worker_" + ix);
          try
          {
            var result = await fetcher.FetchAsync(ct);
            fetcherActivity?.SetStatusIfUnset(result);
            if (result.IsFailure)
            {
              return;
            }

            if (result.Value.Targets.Count != _targetsCount)
            {
              RecordFatalError(target, result);
              return;
            }

            var pendingNotifications = result.Value.Targets.Select(async t =>
            {
              using var targetActivity = _activitySource.StartActivity("target_" + t.TargetId);
              var currStatus = responsesDict.GetOrAdd(t.TargetId, _ => ProductStatus.Unavailable);
              var prevStatus = ProductStatus.Unset;
              try
              {
                await sharedGates.WaitAsync(CancellationToken.None);
                if (t.IsAvailable)
                {
                  currStatus = ProductStatus.Available;
                }


                responsesDict[t.TargetId] = currStatus;
                _statuses.TryGetValue(t.TargetId, out prevStatus);
              }
              finally
              {
                sharedGates.Release();
              }

              var args = new WatcherStatusChangedEventArgs(t, target, currStatus, prevStatus, ct);
              await StatusChanged.InvokeIfNotEmptyAsync(this, args);

              _statuses[t.TargetId] = currStatus;
            });

            await Task.WhenAll(pendingNotifications);
          }
          catch (Exception exc)
          {
            fetcherActivity.RecordException(exc);
            throw;
          }
        }
      });

      await Task.WhenAll(pendingChecks);
    }

    public event AsyncEventHandler<WatcherStatusChangedEventArgs>? StatusChanged;

    private void RecordFatalError(WatchTarget spec, Result<StatusFetchResult> result)
    {
      Activity.Current = null;
      using var fatalErrorActivity = _activitySource.StartActivity("fatal_error");
      fatalErrorActivity?.SetStatus(Status.Error.WithDescription(
        "targets count returned by watchersFactory must match count of fetched targets"));

      fatalErrorActivity?.SetTag("expected_targets_count", _targetsCount);
      fatalErrorActivity?.SetTag("actual_targets_count", result.Value.Targets.Count);
      fatalErrorActivity?.SetTag("fetch_url", _target.Input);
      for (var tix = 0; tix < result.Value.Targets.Count; tix++)
      {
        var t = result.Value.Targets[tix];
        var summary = spec.Products[t.TargetId];
        fatalErrorActivity?.SetTag("target_" + tix,
          $"id='{t.TargetId}' available='{t.IsAvailable}' title='{summary.Title}' pic='{summary.Picture}'");
      }
    }

    private void Cancel()
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }
  }
}