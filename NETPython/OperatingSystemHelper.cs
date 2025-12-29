using System.Runtime.InteropServices;

namespace NETPython
{
  public static class OperatingSystemHelper
  {
    public static void CheckPlatform()
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        Console.WriteLine("Running on Windows!");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        Console.WriteLine("Running on Linux!");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        Console.WriteLine("Running on macOS!");
      }
      else
      {
        Console.WriteLine("Unknown operating system.");
      }
    }
  }
}
