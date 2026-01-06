using Python.Runtime;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ".venv");
      string message;

      using PythonInitialiser pythonInitialiser = new();
      if ((message = pythonInitialiser
        .InitialisePy(pathToVirtualEnv, "Scripts")) != "")
      {
        Console.WriteLine(message);
        return;
      }

      using (Py.GIL())
      {
        try
        {
          dynamic module = Py.Import("r_visual");
          module.create_plot(5);
        }
        catch(PythonException pex)
        {
          Console.WriteLine(pex.Format());
          //Console.ReadKey();
        }
          
      }
    }
  }
}
