using System;

namespace ProjectMonitors.Crawler.Domain
{
  public readonly struct BinarySearchIndex : IEquatable<BinarySearchIndex>
  {
    public BinarySearchIndex(int min, int max)
    {
      Min = Math.Max(0, min);
      Max = Math.Max(0, max);
    }

    public int Min { get; }
    public int Max { get; }

    public int Middle => Math.Max(0, (int) Math.Ceiling((Max - Min + 1) / 2D) + Min - 1);
    public bool IsEmptyRange => Min == Max;

    // Math.Max(0, (int) Math.Ceiling((maxIdx + 1) / 2D) - 1);
    public BinarySearchIndex Backward() => new(Min, Math.Max(Min, (int) Math.Ceiling((Max + 1) / 2D) - 1));

    // minIdx = Math.Min(middleIdx + 1, indexedPages.Count - 1);
    public BinarySearchIndex Forward() => new(Math.Min(Middle + 1, Max), Max);

    public override string ToString()
    {
      return $"Min={Min} Max={Max} Middle={Middle}";
    }

    #region Generated

    public bool Equals(BinarySearchIndex other)
    {
      return Min == other.Min && Max == other.Max;
    }

    public override bool Equals(object? obj)
    {
      return obj is BinarySearchIndex other && Equals(other);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Min, Max);
    }

    public static bool operator ==(BinarySearchIndex left, BinarySearchIndex right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(BinarySearchIndex left, BinarySearchIndex right)
    {
      return !left.Equals(right);
    }

    #endregion
  }
}