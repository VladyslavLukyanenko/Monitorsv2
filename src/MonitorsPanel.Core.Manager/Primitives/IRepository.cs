using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorsPanel.Core.Manager.Primitives
{
  public interface IRepository<T, in TKey>
    where T : class, IEntity<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
  {
    Task<T> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<IList<T>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default);
    Task<bool> ExistsAsync(TKey key, CancellationToken token = default);
    Task<IList<T>> ListAllAsync(CancellationToken token = default);
  }
}