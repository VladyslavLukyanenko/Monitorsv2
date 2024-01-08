﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using MonitorsPanel.Web.ManagerApi.App.Model;

namespace MonitorsPanel.Web.ManagerApi.App.Services
{
  public class InMemoryComponentsStateRegistry : IComponentsStateRegistry
  {
    private readonly ConcurrentDictionary<string, ComponentStatsEntry> _entries = new();

    public ILookup<string, ComponentStatsEntry> GetGroupedEntries(TimeSpan maxRefreshDelay) =>
      _entries.Values.Where(_ =>_.Timestamp >= DateTimeOffset.UtcNow - maxRefreshDelay)
        .ToLookup(_ => _.ComponentType);
    public void Update(ComponentStatsEntry entry)
    {
      _entries[$"{entry.ComponentType}__{entry.ComponentName}"] = entry;
    }
  }
}