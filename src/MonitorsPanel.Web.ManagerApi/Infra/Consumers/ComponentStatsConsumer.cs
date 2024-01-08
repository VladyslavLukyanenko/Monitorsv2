using System.Threading.Tasks;
using MassTransit;
using MonitorsPanel.Web.ManagerApi.App.Model;
using MonitorsPanel.Web.ManagerApi.App.Services;

namespace MonitorsPanel.Web.ManagerApi.Infra.Consumers
{
  public class ComponentStatsConsumer : IConsumer<ComponentStatsEntry>
  {
    private readonly IComponentsStateRegistry _componentsStateRegistry;

    public ComponentStatsConsumer(IComponentsStateRegistry componentsStateRegistry)
    {
      _componentsStateRegistry = componentsStateRegistry;
    }

    public Task Consume(ConsumeContext<ComponentStatsEntry> context)
    {
      _componentsStateRegistry.Update(context.Message);
      return Task.CompletedTask;
    }
  }
}