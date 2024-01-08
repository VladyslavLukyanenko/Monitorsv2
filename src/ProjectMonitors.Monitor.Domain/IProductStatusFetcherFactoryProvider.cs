namespace ProjectMonitors.Monitor.Domain
{
  public interface IProductStatusFetcherFactoryProvider
  {
    IProductStatusFetcherFactory GetFactoryFor(MonitorInfo info);
    IProductStatusFetcherFactory? GetFactoryOrNullFor(MonitorInfo info);
  }
}