using System.Collections.Generic;
using System.Linq;
using MonitorsPanel.Core.Manager.Primitives;

namespace MonitorsPanel.Core.Manager
{
  public class ImageInfo : Entity<string>
  {
    public const string MonitorPublicSlugParamName = "MONITOR_PUBLIC_SLUG";
    public const string RunningStateValue = "running";

    private List<string> _requiredSpawnParameters = new List<string>();

    private ImageInfo()
    {
    }

    public ImageInfo(string id, string name, string slug, ImageType imageType,
      IEnumerable<string> requiredSpawnParameters, string username = null, string password = null)
      : base(id.Trim())
    {
      Name = name.Trim();
      Slug = slug.Trim();
      Username = username?.Trim();
      Password = password?.Trim();
      ImageType = imageType;
      _requiredSpawnParameters = requiredSpawnParameters.Select(o => o.Trim()).ToList();
    }

    public string Name { get; private set; }
    public string Slug { get; private set; }

    public string Username { get; private set; }
    public string Password { get; private set; }
    public ImageType ImageType { get; private set; }

    public IReadOnlyList<string> RequiredSpawnParameters => _requiredSpawnParameters.AsReadOnly();
  }
}