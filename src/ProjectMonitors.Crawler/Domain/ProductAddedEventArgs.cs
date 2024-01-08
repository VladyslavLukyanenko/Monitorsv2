using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Crawler.Domain
{
  public class ProductsUpdatedEventArgs : AsyncEventArgs
  {
    public ProductsUpdatedEventArgs(IEnumerable<Product> products, IEnumerable<Product> removed,
      CancellationToken cancellationToken)
      : base(cancellationToken)
    {
      Removed = removed.ToList();
      Added = products.ToList();
    }

    public IList<Product> Added { get; }
    public IList<Product> Removed { get; }
  }
}