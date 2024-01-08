using System;

namespace ProjectMonitors.SeedWork.Bus.Rmq
{
  public class SenderConfig : IEquatable<SenderConfig>
  {
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string RoutingKey { get; init; } = null!;

    public bool IsOutdated(TimeSpan lifetime) => CreatedAt + lifetime < DateTimeOffset.UtcNow;
    
    public bool Equals(SenderConfig? other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return RoutingKey == other.RoutingKey;
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

      return Equals((SenderConfig) obj);
    }

    public override int GetHashCode()
    {
      return RoutingKey.GetHashCode();
    }

    public static bool operator ==(SenderConfig? left, SenderConfig? right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(SenderConfig? left, SenderConfig? right)
    {
      return !Equals(left, right);
    }
  }
}