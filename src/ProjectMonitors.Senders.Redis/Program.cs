using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ProjectMonitors.SeedWork;

namespace ProjectMonitors.Senders.Redis
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
        .ConfigureServices((hostContext, services) => { new Startup().ConfigureServices(services, hostContext); });
  }
}