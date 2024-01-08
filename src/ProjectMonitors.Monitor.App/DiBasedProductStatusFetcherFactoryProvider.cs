using System;
using System.Collections.Generic;
using System.Linq;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.App
{
  public class DiBasedProductStatusFetcherFactoryProvider : IProductStatusFetcherFactoryProvider
  {
    private readonly IEnumerable<IProductStatusFetcherFactory> _fetcherFactories;

    public DiBasedProductStatusFetcherFactoryProvider(IEnumerable<IProductStatusFetcherFactory> fetcherFactories)
    {
      _fetcherFactories = fetcherFactories;
    }

    public IProductStatusFetcherFactory GetFactoryFor(MonitorInfo info)
    {
      return GetFactoryOrNullFor(info)
             ?? throw new InvalidOperationException("Can't find fetchers factory for " + info.Slug);
    }

    public IProductStatusFetcherFactory? GetFactoryOrNullFor(MonitorInfo info)
    {
      return _fetcherFactories.SingleOrDefault(_ => _.TargetMonitorSlug == info.Slug);
    }
  }
}