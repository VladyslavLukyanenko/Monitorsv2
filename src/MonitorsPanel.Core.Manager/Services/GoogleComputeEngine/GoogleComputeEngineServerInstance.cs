using System;

namespace MonitorsPanel.Core.Manager.Services.GoogleComputeEngine
{
  public class GoogleComputeEngineServerInstance : ServerInstance
  {
    public GoogleComputeEngineServerInstance(string id, SupportedHostingTargets supportedHostingTargets, string zone,
      string instanceType, DateTimeOffset launchTime)
      : base(id, supportedHostingTargets)
    {
      Zone = zone;
      InstanceType = instanceType;
      LaunchTime = launchTime;
    }

    public string Zone { get; private set; }
    public string InstanceType { get; private set; }
    public DateTimeOffset LaunchTime { get; private set; }
    public override string ProviderName => "GoogleComputeEngine";
  }
}