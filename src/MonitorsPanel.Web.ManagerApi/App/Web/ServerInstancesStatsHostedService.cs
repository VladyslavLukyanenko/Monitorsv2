using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitorsPanel.Core.Manager;

namespace MonitorsPanel.Web.ManagerApi.App.Web
{
  public class ServerInstancesStatsHostedService : BackgroundService
  {
    private readonly IServiceProvider _serviceProvider;

    public ServerInstancesStatsHostedService(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
      using (var scope = _serviceProvider.CreateScope())
      {
        var clients = scope.ServiceProvider.GetServices<IInfrastructureClient>()
          .ToArray();
        var logger = scope.ServiceProvider.GetService<ILogger<ServerInstancesStatsHostedService>>();
        
        logger.LogDebug("Started");
        while (!stoppingToken.IsCancellationRequested)
        {
          try
          {
            foreach (var client in clients)
            {
              await client.RefreshInstancesAsync(stoppingToken);
            }
          }
          catch (Exception e)
          {
            logger.LogError(e, "Error occurred on refreshing stats of server instances");
          }

          await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        
        logger.LogDebug("Finished");
      }
    }
  }
}