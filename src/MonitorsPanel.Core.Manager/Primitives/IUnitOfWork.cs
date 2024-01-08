using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorsPanel.Core.Manager.Primitives
{
  public interface IUnitOfWork
    : IDisposable
  {
    Task<int> SaveChangesAsync(CancellationToken token = default);

    Task<bool> SaveEntitiesAsync(CancellationToken token = default);

    Task<ITransactionScope> BeginTransactionAsync(bool autoCommit = true,
      IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken ct = default);
  }
}