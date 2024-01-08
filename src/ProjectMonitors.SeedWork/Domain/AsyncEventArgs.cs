using System;
using System.Threading;

namespace ProjectMonitors.SeedWork.Domain
{
  public abstract class AsyncEventArgs : EventArgs
  {
    protected AsyncEventArgs(CancellationToken cancellationToken)
    {
      CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }
  }
}