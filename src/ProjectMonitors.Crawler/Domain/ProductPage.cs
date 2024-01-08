using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMonitors.Crawler.Domain
{
  public class ProductPage : IEquatable<ProductPage>
  {
    private readonly List<Product> _products = new();
    public Uri Url { get; init; } = null!;

    public IReadOnlyList<Product> Products
    {
      get => _products;
      init => _products = value.ToList();
    }

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public int PageIdx { get; init; }
    public int ItemsPerPage { get; init; }

    public static IList<ProductPage> InsertArranged(IEnumerable<Product> currentProducts, IEnumerable<Product> toInsert,
      int itemsPerPage, Func<int, int, Uri> urlFactory)
    {
      var listToInsert = toInsert as Product[] ?? toInsert.ToArray();
      if (!listToInsert.Any())
      {
        throw new InvalidOperationException("No products to insert");
      }

      var products = currentProducts.OrderBy(_ => _.GlobalIndex).ToList();
      foreach (var product in listToInsert.OrderBy(_ => _.GlobalIndex))
      {
        var insertAfter = products.FindLastIndex(_ => _.GlobalIndex < product.GlobalIndex);
        for (int ix = insertAfter + 1; ix < products.Count; ix++)
        {
          products[ix].GlobalIndex++;
        }

        products.Insert(insertAfter + 1, product);
      }

      return Paginate(products, itemsPerPage, urlFactory);
    }

    public static IList<ProductPage> RemoveArranged(IEnumerable<Product> currentProducts, IEnumerable<Product> removed,
      int itemsPerPage, Func<int, int, Uri> urlFactory)
    {
      var removedList = removed as Product[] ?? removed.ToArray();
      if (!removedList.Any())
      {
        throw new InvalidOperationException("No products to remove");
      }

      var products = currentProducts.Except(removedList).OrderBy(_ => _.GlobalIndex).ToList();
      RecalculateIndex(products);

      return Paginate(products, itemsPerPage, urlFactory);
    }

    private static void RecalculateIndex(IEnumerable<Product> products)
    {
      int ix = 0;
      foreach (var product in products)
      {
        product.GlobalIndex = ix++;
      }
    }

    #region Generated

    public bool Equals(ProductPage? other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return _products.SequenceEqual(other._products)
             && Url.Equals(other.Url)
             && PageIdx == other.PageIdx
             && ItemsPerPage == other.ItemsPerPage;
    }

    public override bool Equals(object? obj)
    {
      if (ReferenceEquals(null, obj))
      {
        return false;
      }

      if (ReferenceEquals(this, obj))
      {
        return true;
      }

      if (obj.GetType() != this.GetType())
      {
        return false;
      }

      return Equals((ProductPage) obj);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(_products, Url, PageIdx, ItemsPerPage);
    }

    public static bool operator ==(ProductPage? left, ProductPage? right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(ProductPage? left, ProductPage? right)
    {
      return !Equals(left, right);
    }

    #endregion

    private static IList<ProductPage> Paginate(IReadOnlyCollection<Product> products, int itemsPerPage,
      Func<int, int, Uri> urlFactory)
    {
      var nextPagesCount = (int) Math.Ceiling(products.Count / (double) itemsPerPage);
      var output = new List<ProductPage>(nextPagesCount);
      for (var ix = 0; ix < nextPagesCount; ix++)
      {
        var page = new ProductPage
        {
          Url = urlFactory(ix, itemsPerPage),
          PageIdx = ix,
          ItemsPerPage = itemsPerPage
        };

        output.Add(page);
        page._products.AddRange(products.Skip(ix * itemsPerPage).Take(itemsPerPage));
      }

      return output;
    }
  }
}