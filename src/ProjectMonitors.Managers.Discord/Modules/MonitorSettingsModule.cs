using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LinqToDB;
using LinqToDB.Configuration;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.Monitor.Infra;

namespace ProjectMonitors.Managers.Discord.Modules
{
  [Group("monitor")]
  public class MonitorSettingsModule : ModuleBase<SocketCommandContext>
  {
    private readonly IProductStatusFetcherFactoryProvider _fetcherFactoryProvider;
    private readonly LinqToDbConnectionOptions<MonitorSettingsDbConnection> _options;
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public MonitorSettingsModule(LinqToDbConnectionOptions<MonitorSettingsDbConnection> options,
      IProductStatusFetcherFactoryProvider fetcherFactoryProvider, IMonitorHttpClientFactory monitorHttpClientFactory)
    {
      _options = options;
      _fetcherFactoryProvider = fetcherFactoryProvider;
      _monitorHttpClientFactory = monitorHttpClientFactory;
    }

    [Command("add")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    public async Task AddCommand(string monitor, string input)
    {
      var factory = _fetcherFactoryProvider.GetFactoryOrNullFor(new MonitorInfo
      {
        Slug = monitor,
        Name = monitor
      });

      try
      {
        if (factory == null)
        {
          await Context.Channel.SendMessageAsync("Can't find monitor " + monitor);
          return;
        }

        var result = factory.ParseRawTargetInput(input);
        if (result.IsFailure)
        {
          await Context.Channel.SendMessageAsync("Failed to validate input: " + result.Error);
          return;
        }

        await using var db = new MonitorSettingsDbConnection(_options);
        var monitorEntry = await db.Settings.FirstOrDefaultAsync(_ => _.MonitorName == monitor);
        if (monitorEntry == null)
        {
          await Context.Channel.SendMessageAsync("Can't find settings for input");
          return;
        }

        _monitorHttpClientFactory.Configure(monitorEntry);
        var target = await factory.CreateTargetAsync(input);
        monitorEntry.Targets.Add(target);
        await db.Settings.Where(_ => _.MonitorName == monitor)
          .Set(_ => _.UpdatedAt, () => Sql.CurrentTimestampUtc)
          .Set(_ => _.Targets, monitorEntry.Targets)
          .UpdateAsync();

        await Context.Channel.SendMessageAsync($"Added successfully target {input}");
      }
      catch (Exception e)
      {
        await Context.Channel.SendMessageAsync($"Failed to validate input: {e.Message}");
      }
    }

    [Command("remove")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [Alias("delete", "del")]
    public async Task RemoveCommand(string monitor, string input)
    {
      var factory = _fetcherFactoryProvider.GetFactoryOrNullFor(new MonitorInfo
      {
        Slug = monitor,
        Name = monitor
      });

      try
      {
        if (factory == null)
        {
          await Context.Channel.SendMessageAsync("Can't find monitor " + monitor);
          return;
        }

        var result = factory.ParseRawTargetInput(input);
        if (result.IsFailure)
        {
          await Context.Channel.SendMessageAsync("Failed to validate input: " + result.Error);
          return;
        }

        var sku = result.Value;
        await using var db = new MonitorSettingsDbConnection(_options);
        var monitorEntry = await db.Settings.FirstOrDefaultAsync(_ => _.MonitorName == monitor);
        var target = monitorEntry?.Targets.FirstOrDefault(_ => _.Input == sku);
        if (monitorEntry == null || target == null)
        {
          await Context.Channel.SendMessageAsync($"Failed to find target {sku}");
          return;
        }

        monitorEntry.Targets.Remove(target);
        await db.Settings.Where(_ => _.MonitorName == monitor)
          .Set(_ => _.UpdatedAt, () => Sql.CurrentTimestampUtc)
          .Set(_ => _.Targets, monitorEntry.Targets)
          .UpdateAsync();

        await Context.Channel.SendMessageAsync($"Removed successfully target {sku}");
      }
      catch (Exception e)
      {
        await Context.Channel.SendMessageAsync($"Failed to validate input: {e.Message}");
      }
    }
  }
}