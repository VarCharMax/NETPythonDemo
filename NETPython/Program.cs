using Python.Runtime;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      //Highest Python version compatible with pythonNet is currently 3.13.
      const string pynetversion = "3.13";
      string pythonDll = "";

      string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "Scripts", ".venv", "pyvenv.cfg");

      //Get Python configuration from pyvenv.cfg
      Dictionary<string, string> config = File.ReadLines(pathToVirtualEnv)
          .Select(line => line.Trim().Split('='))
          .Where(arr => arr.Length == 2)
          .Select(arr => (Key: arr[0].Trim(), Value: arr[1].Trim()))
          .GroupBy(x => x.Key)
          .ToDictionary(keyGroup => keyGroup.Key, keyGroup => keyGroup.First().Value);

      // Extract major and minor version (e.g., "3.11" from "3.11.4"),
      // remove the dot for dll naming.
      string pyVersion = config["version"][..config["version"]
        .LastIndexOf('.')].Replace(".", "");

      if (pyVersion != pynetversion.Replace(".", ""))
      {
        throw new Exception($"Compatible Python version {pynetversion} either not installed or not configured");
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        pythonDll = Path.Combine(config["home"], $"python{pyVersion}.dll");
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
        ExceptionDispatchInfo
          .Capture(new PlatformNotSupportedException("Unsupported OS platform"))
          .Throw();
      }

      Runtime.PythonDLL = pythonDll;

      PythonEngine.Initialize();

      using (Py.GIL())
      {
        try
        {
          /* Note: I found numerous forum posts saying that the correct way to support Python in .NET is with these kinds of settings:

           string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ".venv");
           path = string.IsNullOrEmpty(path) ? pathToVirtualEnv : pathToVirtualEnv + ";" + path;
           Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
           Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
           Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);
           PythonEngine.Initialize();
           PythonEngine.PythonHome = pathToVirtualEnv;
           PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);

           I couldn't get pythonnet to load the virtual environment modules using these settings, however. I suspect it's looking for the modules
           relative to the base Python installation rather than the virtual environment.
           The below code works.
          */

          dynamic sys = Py.Import("sys");
          sys.path.append("Scripts");
          sys.path.append("Scripts/.venv/Lib");
          sys.path.append("Scripts/.venv/Lib/site-packages");

          dynamic module = Py.Import("rw_visual");
          module.create_plot();

          // Shutdown() will throw an exception because of reliance on BinaryFormatter
          // which is no longer supported on Core 9+.
          // Currently this is unavoidable due to how pythonnet works. Catching the
          // exception is the best we can do.
          PythonEngine.Shutdown();
        }
        catch (PlatformNotSupportedException)
        {
          // Ignore the exception as the shutdown likely proceeded enough
        }
        catch (PythonException ex)
        {
          Console.WriteLine($"Python error: {ex.Message}");
        }
      }
    }
  }
}
