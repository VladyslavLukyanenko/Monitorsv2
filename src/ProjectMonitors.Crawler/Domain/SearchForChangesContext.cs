using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectMonitors.Crawler.Domain
{
  public class SearchForChangesContext
  {
    public SearchForChangesContext(int indexedProductsCount, int detectedProductsCount)
    {
      IndexedProductsCount = indexedProductsCount;
      DetectedProductsCount = detectedProductsCount;
    }

    public IDictionary<int, Task<ProductPage>> Cache { get; } = new Dictionary<int, Task<ProductPage>>();
    public int IndexedProductsCount { get; }
    public int DetectedProductsCount { get; }
  }
}