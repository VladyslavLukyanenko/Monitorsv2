using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using MonitorsPanel.Web.ManagerApi.Foundation;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace MonitorsPanel.Web.ManagerApi
{
  public class Program
  {
    static Program()
    {
      var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

      var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddBaseAppSettingsFile()
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{environmentName}.json", true)
        .AddJsonFile("appsettings.local.json", true)
        .AddEnvironmentVariables();

      var args = Environment.GetCommandLineArgs();

      if (args.Any())
      {
        builder.AddCommandLine(args);
      }

      IdentityModelEventSource.ShowPII = true;
      Configuration = builder.Build();
    }

    private static IConfigurationRoot Configuration { get; }

    public static async Task Main(string[] args)
    {
      var logger = LogManager.GetCurrentClassLogger();
      try
      {
        logger.Info("Starting application");

        var webHost = CreateHostBuilder(args).Build();

        await MigrateDatabaseIfAllowedAsync(webHost);
        await webHost.RunAsync();
      }
      catch (Exception e)
      {
        logger.Fatal(e, "Host terminated unexpectedly");
        throw;
      }
      finally
      {
        LogManager.Flush();
        LogManager.Shutdown();
      }
    }

    private static async Task MigrateDatabaseIfAllowedAsync(IHost webHost)
    {
      await webHost.MigrateDbContextAsync<DbContext>(async (context, services) =>
      {
        // IEnumerable<IDataSeeder> seeders = services.GetServices<IDataSeeder>().OrderBy(_ => _.Order);
        // using (await context.Database.BeginTransactionAsync())
        // {
        //   foreach (IDataSeeder seeder in seeders)
        //   {
        //     await seeder.SeedAsync();
        //   }
        //
        //   context.Database.CommitTransaction();
        // }
      });
    }


    private static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(c => c.AddConfiguration(Configuration))
        .ConfigureAppConfiguration(c => c.AddConfiguration(Configuration))
        .ConfigureWebHostDefaults(configure =>
        {
          configure
            .UseNLog()
            .UseStartup<Startup>();
        })
        .ConfigureLogging(logging =>
        {
          logging.ClearProviders();
          logging.AddNLog();
        })
        .UseConsoleLifetime(options => { });
    }
  }
}