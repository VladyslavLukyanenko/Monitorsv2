using System;

namespace ProjectMonitors.Crawler.Domain
{
  public class Product : IEquatable<Product>
  {
    public int GlobalIndex { get; set; }
    public int PageIdx { get; init; }
    public string Id { get; init; }
    public DateTimeOffset ChangedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Title { get; init; }
    public decimal Price { get; init; }
    public string ProductUrl { get; init; }
    public string ImageUrl { get; init; }


    public bool Equals(Product? other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return Id == other.Id
             && Title == other.Title
             && Price == other.Price
             && ProductUrl == other.ProductUrl
             && ImageUrl == other.ImageUrl;
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

      return Equals((Product) obj);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Id, Title, Price, ProductUrl, ImageUrl);
    }

    public static bool operator ==(Product? left, Product? right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Product? left, Product? right)
    {
      return !Equals(left, right);
    }
  }
}