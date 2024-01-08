using System;

namespace MonitorsPanel.Core.Manager.Services.Aws
{
  public class AwsServerInstance : ServerInstance
  {
    public AwsServerInstance(string id, SupportedHostingTargets supportedHostingTargets, string availabilityZone,
      string keyName, string instanceType)
      : base(id, supportedHostingTargets)
    {
      AvailabilityZone = availabilityZone;
      KeyName = keyName;
      InstanceType = instanceType;
    }

    public string AvailabilityZone { get; private set; }
    public string KeyName { get; private set; }
    public string InstanceType { get; private set; }

    public DateTimeOffset LaunchTime { get; set; }
    public override string ProviderName => "AWS";
  }
}