using System.Collections.Generic;

namespace MonitorsPanel.Core.Manager.Services.Aws
{
  public class AwsConfig
  {
    public string AmiId { get; set; }
    public string InstanceType { get; set; }
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string PlacementRegion { get; set; }
    public string KeyName { get; set; }
    public SupportedHostingTargets HostingTargets { get; set; }
    public List<string> SecurityGroupIds { get; set; } = new List<string>();
  }
}