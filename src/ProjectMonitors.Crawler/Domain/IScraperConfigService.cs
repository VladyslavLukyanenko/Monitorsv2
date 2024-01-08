using System;

namespace ProjectMonitors.Crawler.Domain
{
  public interface IScraperConfigService
  {
    IObservable<ScraperConfig> Config { get; }
  }
}