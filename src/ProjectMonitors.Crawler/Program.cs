using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork;
using ProjectMonitors.Crawler.Domain;
using Serilog;

namespace ProjectMonitors.Crawler
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine($"Starting {AppConstants.AppName}@{AppConstants.InformationalVersion}");
      var builder = CreateHostBuilder(args);
      var host = builder.Build();
      var activitySource = host.Services.GetRequiredService<ActivitySource>();
      var logger = host.Services.GetRequiredService<ILogger<Program>>();
      var tracer = host.Services.GetRequiredService<TracerProvider>();
      try
      {
        using (activitySource.StartActivity("bootstrap"))
        {
          await host.StartAsync().ConfigureAwait(false);
        }

        var cts = new CancellationTokenSource();
        var scraper = host.Services.GetRequiredService<ICrawler>();

        await scraper.ClearIndexAsync(cts.Token);
        await scraper.IndexAsync(cts.Token);

        scraper.ProductsUpdated += ScrapperOnProductsUpdated;
        scraper.StartWatchers(cts.Token);

        await host.WaitForShutdownAsync().ConfigureAwait(false);
      }
      catch (Exception e)
      {
        logger.LogCritical(e, "Host terminated unexpectedly");
        throw;
      }
      finally
      {
        if (host is IAsyncDisposable asyncDisposable)
        {
          await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
          host.Dispose();
        }

        Log.CloseAndFlush();
        tracer.Shutdown();
      }
    }

    private static ValueTask ScrapperOnProductsUpdated(object? sender, ProductsUpdatedEventArgs e)
    {
      Log.Logger.Information("Added: '{@ProductTitles}', Removed: '{@RemovedTitles}'", e.Added.Select(r => r.Title),
        e.Removed.Select(_ => _.Title));
      return ValueTask.CompletedTask;
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((_, configurationBuilder) =>
        {
          configurationBuilder.AddEnvironmentVariables("MONITORS_")
            .AddJsonFile("serilogsettings.json", false, true);
        })
        .ConfigureServices((hostCtx, services) =>
        {
          var cfg = new Startup(hostCtx.Configuration);
          cfg.ConfigureServices(services);
        })
        .UseSerilog((context, _, logCfg) =>
          logCfg.ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
        )
        .UseConsoleLifetime();
  }
}