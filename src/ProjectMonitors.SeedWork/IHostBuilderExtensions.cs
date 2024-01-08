using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Serilog;

namespace ProjectMonitors.SeedWork
{
  // ReSharper disable once InconsistentNaming
  public static class IHostBuilderExtensions
  {
    public static IHostBuilder UseDefaultConsoleAppConfig(this IHostBuilder builder)
    {
      Console.WriteLine($"Starting {AppConstants.AppName}@{AppConstants.InformationalVersion}");
      return builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
          var env = _.HostingEnvironment.EnvironmentName;
          configurationBuilder.AddEnvironmentVariables("MONITORS_")
            .AddJsonFile("serilogsettings.json", false, true)
            .AddInMemoryCollection(new Dictionary<string, string>
            {
              {"Serilog:WriteTo:2:Args:indexFormat", AppConstants.AppName + "-{0:yyyyMMdd}"},
            })
            .AddJsonFile($"serilogsettings.{env}.json", true, true);
        })
        .UseSerilog((context, _, logCfg) =>
          logCfg.ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
        )
        .UseConsoleLifetime();
    }

    public static async ValueTask BootstrapWithErrorHandlingAsync(this IHostBuilder builder,
      Func<IServiceProvider, ValueTask>? bootstrapper = null)
    {
      var host = builder.Build();

      var activitySource = host.Services.GetRequiredService<ActivitySource>();
      var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
      var logger = loggerFactory.CreateLogger("Bootstrap");
      var tracer = host.Services.GetRequiredService<TracerProvider>();
      IApmAgent? apmAgent = null;
      try
      {
        using (activitySource.StartActivity("bootstrap"))
        {
          if (bootstrapper != null)
          {
            await bootstrapper(host.Services);
          }

          await host.StartAsync().ConfigureAwait(false);
          apmAgent = host.Services.GetRequiredService<IApmAgent>();
        }

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
        if (apmAgent != null)
        {
          await apmAgent.Flush().ConfigureAwait(false);
        }
      }
    }
  }
}