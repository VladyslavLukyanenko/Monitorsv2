using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ProjectMonitors.Monitor.Domain
{
  public class GenericWatcherFactory : IWatcherFactory
  {
    private readonly IProductStatusFetcherFactory _fetcherFactory;
    private readonly ActivitySource _activitySource;
    private readonly ILoggerFactory _loggerFactory;

    public GenericWatcherFactory(IProductStatusFetcherFactoryProvider fetcherFactoryProvider, MonitorInfo monitorInfo,
      ActivitySource activitySource, ILoggerFactory loggerFactory)
    {
      _activitySource = activitySource;
      _loggerFactory = loggerFactory;
      _fetcherFactory = fetcherFactoryProvider.GetFactoryFor(monitorInfo);
    }

    public IWatcher Create(WatchTarget target, IDictionary<string, ProductStatus> initialStatuses) =>
      new GenericWatcher(target, _fetcherFactory, initialStatuses.Count, initialStatuses, _activitySource,
        _loggerFactory.CreateLogger<GenericWatcher>());
  }
}