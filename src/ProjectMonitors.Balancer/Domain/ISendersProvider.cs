using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DynamicData.Binding;
using ProjectMonitors.SeedWork.Bus.Rmq;

namespace ProjectMonitors.Balancer.Domain
{
  public interface ISendersProvider
  {
    IEnumerable<SenderConfig> Senders { get; }
    IObservable<int> SendersCount { get; }
  }
}