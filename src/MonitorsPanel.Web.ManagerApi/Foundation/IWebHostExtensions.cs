using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorsPanel.Web.ManagerApi.Foundation.Config;
using Polly;

namespace MonitorsPanel.Web.ManagerApi.Foundation
{
  // ReSharper disable once InconsistentNaming
  public static class IWebHostExtensions
  {
    public static async Task<IHost> MigrateDbContextAsync<TContext>(this IHost webHost,
      Func<TContext, IServiceProvider, Task> seeder)
      where TContext : DbContext
    {
      using (var scope = webHost.Services.CreateScope())
      {
        var services = scope.ServiceProvider;
        var efConfig = services.GetRequiredService<IOptions<EfCoreConfig>>();
        var logger = services.GetRequiredService<ILogger<TContext>>();
        if (!efConfig.Value.MigrateDatabaseOnStart)
        {
          logger.LogDebug($"Skipping migration database associated with context {typeof(TContext).Name}");
          return webHost;
        }

        var context = services.GetService<TContext>();

        try
        {
          logger.LogDebug("Checking database accessibility.");
          var retryPolicy = Policy.Handle<SocketException>()
            .Or<DbException>()
            .WaitAndRetryAsync(10,
              retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
              (exception, retryAttempt, executionContext) =>
              {
                logger.LogError("Failed to connect to database. Next retry in {RetryAttempt}",
                  retryAttempt);
              });

          await retryPolicy.ExecuteAsync(async () =>
          {
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            conn.Close();
          });

          logger.LogDebug(
            $"Database is accessible. Migrating database associated with context {typeof(TContext).Name}");

          var retry = Policy.Handle<SqlException>()
            .WaitAndRetryAsync(new[]
            {
              TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(8)
            });

          await retry.ExecuteAsync(async () =>
          {
            //if the sql server container is not created on run docker compose this
            //migration can't fail for network related exception. The retry options for DbContext only 
            //apply to transient exceptions.
          
            await context.Database.MigrateAsync();
            await seeder(context, services);
          });


          logger.LogDebug($"Migrated database associated with context {typeof(TContext).Name}");
        }
        catch (Exception ex)
        {
          logger.LogError(ex,
            $"An error occurred while migrating the database used on context {typeof(TContext).Name}");
        }
      }

      return webHost;
    }
  }
}