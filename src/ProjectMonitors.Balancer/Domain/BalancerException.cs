using System;

namespace ProjectMonitors.Balancer.Domain
{
  public class BalancerException : Exception
  {
    protected BalancerException(string message)
      : base(message)
    {
    }
  }
}