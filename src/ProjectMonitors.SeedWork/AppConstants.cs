using System;
using System.Reflection;

namespace ProjectMonitors.SeedWork
{
  public static class AppConstants
  {
    static AppConstants()
    {
      var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

      CurrentAppVersion = assembly.GetName().Version!;
      AppName = Environment.GetEnvironmentVariable("MONITORS_APPNAME") ?? "<NO_NAME>";
    }
    
    public static Version CurrentAppVersion { get; }
    public static string AppName { get; }

    public static string InformationalVersion => GitVersionInformation.InformationalVersion;
  }
}