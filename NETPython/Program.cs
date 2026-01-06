using Python.Runtime;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ".venv");
      string message;

      if ((message = new PythonInitialiser()
        .InitialisePy(pathToVirtualEnv, "Scripts")) != "")
      {
        Console.WriteLine(message);
        return;
      }

      using (Py.GIL())
      {
          dynamic module = Py.Import("rw_visual");
          module.create_plot(5);
      }

      PythonInitialiser.ShutdownPy();
    }
  }
}
