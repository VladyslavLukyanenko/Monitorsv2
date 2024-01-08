using System.Collections.Generic;
using System.Linq;

namespace ProjectMonitors.Crawler.Domain
{
  public class ChangeDetectorCursor
  {
    public ChangeDetectorCursor(BinarySearchIndex index, IEnumerable<ProductPage> indexedPages, int changesCount)
    {
      Index = index;
      ChangesCount = changesCount;
      IndexedPages = indexedPages.ToList();
      UpdateIndexedProducts();
    }

    public void UpdateChangesCount(int delta) => ChangesCount += delta;

    public int ChangesCount { get; private set; }
    public ProductPage FreshPage { get; set; } = null!;
    public ProductPage? IndexedPage { get; set; }
    public IList<ProductPage> IndexedPages { get; set; }

    public IList<Product> AllIndexedProducts { get; private set; } = null!;
    public BinarySearchIndex Index { get; private set; }

    public void Backward() => Index = Index.Backward();
    public void Forward() => Index = Index.Forward();

    public void UpdateIndexedProducts()
    {
      AllIndexedProducts = IndexedPages.SelectMany(_ => _.Products)
        .OrderBy(_ => _.GlobalIndex)
        .ToList();
    }

    public override string ToString()
    {
      return $"Index={Index} Fresh={FreshPage?.PageIdx} Indexed={IndexedPage?.PageIdx} Changes={ChangesCount}";
    }
  }
}