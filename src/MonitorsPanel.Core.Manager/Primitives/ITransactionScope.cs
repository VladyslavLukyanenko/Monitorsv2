using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorsPanel.Core.Manager.Primitives
{
  public interface ITransactionScope
    : IDisposable
  {
    Task RollbackAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
  }
}