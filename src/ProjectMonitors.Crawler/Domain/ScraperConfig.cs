using System;
using System.Collections.Generic;

namespace ProjectMonitors.Crawler.Domain
{
  public class ScraperConfig
  {
    public int ItemsPerPage { get; init; }
    public string StoreDomain { get; init; } = null!;
    public string Query { get; init; } = null!;

    public DateTimeOffset UpdatedAt { get; init; }
    public IList<Uri> Proxies { get; init; } = new List<Uri>();
    public IList<string> UserAgents { get; init; } = new List<string>();
  }
}