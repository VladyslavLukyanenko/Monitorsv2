using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.Monitor.Infra;
using ProjectMonitors.Monitor.Infra.Config;
using ProjectMonitors.Monitor.Workers;
using ProjectMonitors.SeedWork;

namespace ProjectMonitors.Monitor
{
  class Program
  {
    static async Task Main(string[] args)
    {
      await CreateHostBuilder(args).BootstrapWithErrorHandlingAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .UseDefaultConsoleAppConfig()
        .ConfigureServices((hostCtx, services) =>
        {
          services
            .AddMonitorsFrameworkWithDefaults(hostCtx.Configuration["MONITORS_SLUG"])
            .Configure<ConnectionStrings>(hostCtx.Configuration.GetSection("ConnectionStrings"))
            .AddSingleton(ctx => ctx.GetRequiredService<IOptions<ConnectionStrings>>().Value)
            .AddSingleton<BackgroundMonitorStatsCollector>()
            .AddSingleton<MonitorSettingsFetchWorker>()
            .AddSingleton<IMonitorStatsCollector>(ctx => ctx.GetRequiredService<BackgroundMonitorStatsCollector>())
            .AddSingleton<IMonitorSettingsService>(ctx => ctx.GetRequiredService<MonitorSettingsFetchWorker>())
            .AddHostedService<WatchersManagerWorker>()
            .AddHostedService(ctx => ctx.GetRequiredService<BackgroundMonitorStatsCollector>())
            .AddHostedService(ctx => ctx.GetRequiredService<MonitorSettingsFetchWorker>());
        });
  }
}