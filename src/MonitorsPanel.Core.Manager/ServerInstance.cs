using System;
using System.Collections.Generic;

namespace MonitorsPanel.Core.Manager
{
  public abstract class ServerInstance
  {
    private string _publicDnsName;

    protected ServerInstance(string id, SupportedHostingTargets supportedHostingTargets)
    {
      Id = id;
      SupportedHostingTargets = supportedHostingTargets;
    }

    public string Id { get; private set; }

    public string PublicDnsName
    {
      get => _publicDnsName;
      set
      {
        DockerRemoteApiUrl = !string.IsNullOrEmpty(value)
          ? new Uri("http://" + value + ":2375")
          : null;

        _publicDnsName = value;
      }
    }

    public Uri DockerRemoteApiUrl { get; private set; }
    public SupportedHostingTargets SupportedHostingTargets { get; private set; }
    
    public bool IsAvailable { get; private set; }
    public DateTimeOffset LastChecked { get; private set; }
    public abstract string ProviderName { get; }
    
    
    public ServerInstanceStatus Status { get; set; }
    public Dictionary<string, string> AdditionalStats { get; set; }
    
    public List<ImageRuntimeInfo> Images { get; private set; } = new List<ImageRuntimeInfo>();
    public bool IsRunning => Status == ServerInstanceStatus.Running;
    public bool IsStopped => Status == ServerInstanceStatus.Stopped;
    public bool IsIdle => IsRunning && Images.Count == 0;

    public void Merge(IEnumerable<ImageRuntimeInfo> infos)
    {
      Images.Clear();
      Images.AddRange(infos);
    }
    
    public void Checked(bool isAvailable)
    {
      LastChecked = DateTimeOffset.Now;
      IsAvailable = IsRunning && isAvailable;
    }
  }
}