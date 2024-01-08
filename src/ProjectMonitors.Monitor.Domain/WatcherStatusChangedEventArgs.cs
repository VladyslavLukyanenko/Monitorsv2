using System.Threading;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Domain
{
  public class WatcherStatusChangedEventArgs : AsyncEventArgs
  {
    public WatcherStatusChangedEventArgs(IWatchStatus status, WatchTarget spec, ProductStatus curr,
      ProductStatus prev, CancellationToken ct)
      : base(ct)
    {
      Prev = prev;
      Curr = curr;
      Status = status;
      Spec = spec;
    }

    public ProductStatus Prev { get; }
    public ProductStatus Curr { get; }
    public IWatchStatus Status { get; }
    public WatchTarget Spec { get; }
  }
}