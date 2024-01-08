using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.Extensions.Logging;
using ProjectMonitors.SeedWork.Bus.Rmq;

namespace ProjectMonitors.Balancer.Infra
{
  public class InMemorySendersRegistry : ISendersRegistry
  {
    private static readonly TimeSpan ConfigLifetime = TimeSpan.FromMilliseconds(5000);
    private readonly ILogger<InMemorySendersRegistry> _logger;
    private readonly ISourceList<SenderConfig> _configs = new SourceList<SenderConfig>();

    public InMemorySendersRegistry(ILogger<InMemorySendersRegistry> logger)
    {
      _logger = logger;

      SendersCount = _configs.Connect(_ => !_.IsOutdated(ConfigLifetime))
        .ToCollection()
        .Select(_ => _.Count)
        .Replay()
        .RefCount();
    }

    public void AddOrUpdate(SenderConfig cfg)
    {
      _configs.Edit(configs =>
      {
        if (configs.Contains(cfg))
        {
          configs.Remove(cfg);
        }

        if (!cfg.IsOutdated(ConfigLifetime))
        {
          configs.Add(cfg);
        }
      });
    }

    public void ClearOutdated()
    {
      try
      {
        _configs.Edit(configs =>
        {
          var outdated = configs.Where(_ => _.IsOutdated(ConfigLifetime)).ToArray();
          foreach (var config in outdated)
          {
            configs.Remove(config);
          }
        });
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Error on clearing outdated");
      }
    }

    public IEnumerable<SenderConfig> Senders => _configs.Items.Where(_ => !_.IsOutdated(ConfigLifetime));
    public IObservable<int> SendersCount { get; }
  }
}