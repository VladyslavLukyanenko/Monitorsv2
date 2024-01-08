using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.App
{
  public abstract class ProductStatusFetcherFactoryBase : IProductStatusFetcherFactory
  {
    private static readonly ConcurrentDictionary<Type, string> FetcherTargetsCache = new();

    protected ProductStatusFetcherFactoryBase(IServiceProvider serviceProvider)
    { 
      TargetMonitorSlug = FetcherTargetsCache.GetOrAdd(GetType(), static t =>
      {
        var monitorAttr = t.GetCustomAttribute<MonitorAttribute>();
        if (monitorAttr == null)
        {
          throw new InvalidOperationException(
            $"Missing required attribute {nameof(MonitorAttribute)} for {t.Name}");
        }

        return monitorAttr.MonitorSlug;
      });
    }

    public string TargetMonitorSlug { get; }
    public abstract Result<string> ParseRawTargetInput(string raw);
    public abstract ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default);
    public abstract IProductStatusFetcher CreateFetcher(WatchTarget target);
  }
}