using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Crawler.Domain
{
  public interface IProductPageRepository
  {
    ValueTask ReplaceAsync(ProductPage page, CancellationToken ct = default);
    ValueTask<int> ProductsCountAsync(CancellationToken ct = default);
    ValueTask<IList<ProductPage>> GetListAsync(CancellationToken ct = default);
    ValueTask ClearAsync(CancellationToken ct = default);
    ValueTask ReplaceRangeAsync(IEnumerable<ProductPage> pages, CancellationToken ct = default);
  }
}