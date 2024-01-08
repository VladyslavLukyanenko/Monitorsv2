using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectMonitors.Managers.Discord.Modules;
using ProjectMonitors.Managers.Discord.Services;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.Monitor.Infra;
using ProjectMonitors.Monitor.Infra.Config;
using ProjectMonitors.SeedWork;
using Serilog;

namespace ProjectMonitors.Managers.Discord
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var builder = CreateHostBuilder(args);
      var host = builder.Build();
      var activitySource = host.Services.GetRequiredService<ActivitySource>();
      var logger = host.Services.GetRequiredService<ILogger<Program>>();
      try
      {
        await host.RunAsync();
      }
      catch (Exception e)
      {
        logger.LogCritical(e, "Host terminated unexpectedly");
        throw;
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((_, configurationBuilder) =>
        {
          configurationBuilder.AddJsonFile("serilogsettings.json", false, true);
        })
        .ConfigureServices((hostCtx, services) =>
        {
          services
            .AddMonitorsFrameworkWithDefaults("STUB")
            .AddSingleton<IMonitorStatsCollector, StubMonitorStatsCollector>()
            .Configure<ConnectionStrings>(hostCtx.Configuration.GetSection("ConnectionStrings"))
            .AddSingleton(ctx => ctx.GetRequiredService<IOptions<ConnectionStrings>>().Value)
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
              LogLevel = LogSeverity.Verbose,
              MessageCacheSize = 1000
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
              DefaultRunMode = RunMode.Async,
              LogLevel = LogSeverity.Verbose,
              CaseSensitiveCommands = false,
              ThrowOnError = false
            }))
            .AddSingleton<CommandHandler>()
            .AddSingleton<MonitorSettingsModule>()
            .AddSingleton<CommandService>()
            .AddHostedService<StartupService>()
            .AddConfiguredCoreServices();
        }).UseConsoleLifetime();
  }
}