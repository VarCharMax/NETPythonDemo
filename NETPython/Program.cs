using Python.Runtime;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ".venv");

      string message;
      if ((message = PythonInitialiser.Initialise(pathToVirtualEnv)) != "")
      {
        Console.WriteLine(message);
        return;
      }

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
          // sys.path.append($"Scripts/.venv/Lib/{macosShim}site-packages");

          dynamic module = Py.Import("rw_visual");
          module.create_plot(50);

          // Shutdown() will throw an exception because of reliance on BinaryFormatter
          // which is no longer supported on Core 9+.
          // Currently this is unavoidable due to how pythonnet works. Catching the
          // exception is the best we can do.
          // AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true); // No longer works in .NET 8+
          PythonEngine.Shutdown();
          // AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false); // Causes a corrupted memory exxception.
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
