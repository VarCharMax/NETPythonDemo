using System.Runtime.InteropServices;

namespace NETPython
{
  public enum OperatingSystem
  {
    Windows,
    Linux,
    MacOS,
    Unknown
  }

  public static class OperatingSystemHelper
  {
    public static OperatingSystem CheckPlatform()
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        return OperatingSystem.Windows;
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        return OperatingSystem.Linux;
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        return OperatingSystem.MacOS;
      }
      else
      {
        return OperatingSystem.Unknown;
      }
    }
  }
}
