namespace MonitorsPanel.Core.Manager.Services.GoogleComputeEngine
{
  public class GoogleComputeEngineConfig
  {
    public string SourceInstanceTemplate { get; set; }
    public string Zone { get; set; }
    public string ProjectId { get; set; }
    public string CredentialsPath { get; set; }
    public string ApplicationName { get; set; }
    public string MachineType { get; set; }

    public SupportedHostingTargets HostingTargets { get; set; }
  }
}