using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IProductRepository
  {
    ValueTask<ProductRef?> GetRefAsync(string targetId, CancellationToken ct);
    ValueTask CreateAsync(ProductRef productRef, CancellationToken ct);
    ValueTask<bool> AreStatusChangedAsync(string targetId, ProductStatus expectedStatus, CancellationToken ct);
    ValueTask ChangeStatusAsync(string targetId, ProductStatus status, CancellationToken ct);
  }
}