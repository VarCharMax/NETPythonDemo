using Python.Runtime;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var pathToBaseEnv = @"C:\Users\rpark\AppData\Local\Python\pythoncore-3.13-64";
      Runtime.PythonDLL = Path.Combine(pathToBaseEnv, "python313.dll");

      using (Py.GIL())
      {
        try
        {
          dynamic sys = Py.Import("sys");
          sys.path.append("Scripts");
          sys.path.append("Scripts/.venv/Lib");
          sys.path.append("Scripts/.venv/Lib/site-packages");
          dynamic exampleModule = Py.Import("rw_visual");
          dynamic result = exampleModule.create_plot();

          // Shutdown() will throw an exception because of reliance on BinaryFormatter which is obsolete.
          // Currently this is unavoidable due to how pythonnet works.
          // AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
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
