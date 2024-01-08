namespace MonitorsPanel.Web.ManagerApi.Foundation.Config
{
  public class IdentityServerConfig
  {
    public string ValidAudience { get; set; }
    public string ValidIssuer { get; set; }
    public string AuthorityUrl { get; set; }
    
    public bool RequireHttpsMetadata { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateLifetime { get; set; }
  }
}