using Python.Runtime;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      //Highest Python version compatible with compatible is currently 3.13.
      var pathToBaseEnv = @"C:\Users\rpark\AppData\Local\Python\pythoncore-3.13-64";
      Runtime.PythonDLL = Path.Combine(pathToBaseEnv, "python313.dll");

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
           PythonEngine.PythonPath = PythonEngine.PythonPath + ";"+Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
           
           I couldn't get pythonnet to load the virtual environment modules using these settings, however. I suspect it's looking for the modules
           relative to the base Python installation rather than the virtual environment.
           The below code works.
          */

          dynamic sys = Py.Import("sys");
          sys.path.append("Scripts");
          sys.path.append("Scripts/.venv/Lib");
          sys.path.append("Scripts/.venv/Lib/site-packages");

          dynamic exampleModule = Py.Import("rw_visual");
          dynamic result = exampleModule.create_plot();

          // Shutdown() will throw an exception because of reliance on BinaryFormatter which is obsolete.
          // Currently this is unavoidable due to how pythonnet works. Catching the exception is the best we can do.
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
        finally
        {
         
        }
      }
    }
  }
}
