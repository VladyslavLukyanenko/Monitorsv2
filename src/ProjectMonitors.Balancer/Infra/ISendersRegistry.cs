using ProjectMonitors.Balancer.Domain;
using ProjectMonitors.SeedWork.Bus.Rmq;

namespace ProjectMonitors.Balancer.Infra
{
  public interface ISendersRegistry : ISendersProvider
  {
    void AddOrUpdate(SenderConfig cfg);
    void ClearOutdated();
  }
}