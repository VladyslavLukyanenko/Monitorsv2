using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMonitors.Crawler.Domain
{
  public class IndexChangeDescriptor : IEquatable<IndexChangeDescriptor>
  {
    public static readonly IndexChangeDescriptor NoChanges = new(Array.Empty<Product>(), Array.Empty<Product>());

    public IndexChangeDescriptor(IEnumerable<Product> addedProducts, IEnumerable<Product> removedProducts)
    {
      RemovedProducts = removedProducts.ToArray();
      AddedProducts = addedProducts.ToArray();
    }

    /* possible changes:
     - products added/removed
     - added/removed entire pages
     - product edited to represent new one
     */
    public IReadOnlyList<Product> AddedProducts { get; }
    public IReadOnlyList<Product> RemovedProducts { get; }

    public bool Equals(IndexChangeDescriptor? other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return AddedProducts.SequenceEqual(other.AddedProducts)
             && RemovedProducts.SequenceEqual(other.RemovedProducts);
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

      return Equals((IndexChangeDescriptor) obj);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(AddedProducts, RemovedProducts);
    }

    public static bool operator ==(IndexChangeDescriptor? left, IndexChangeDescriptor? right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(IndexChangeDescriptor? left, IndexChangeDescriptor? right)
    {
      return !Equals(left, right);
    }
  }
}