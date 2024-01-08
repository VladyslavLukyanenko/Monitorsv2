using System.Threading.Tasks;

namespace ProjectMonitors.SeedWork.Domain
{
  public delegate ValueTask AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs args)
    where TEventArgs : AsyncEventArgs;
}