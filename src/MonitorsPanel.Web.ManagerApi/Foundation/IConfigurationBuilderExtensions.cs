using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace MonitorsPanel.Web.ManagerApi.Foundation
{
  // ReSharper disable once InconsistentNaming
  public static class IConfigurationBuilderExtensions
  {
    public static IConfigurationBuilder AddBaseAppSettingsFile(this IConfigurationBuilder builder)
    {
      var location = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
      return builder.AddJsonFile(Path.Combine(location!, "appsettings.base.json"), false, true);
    }
  }
}