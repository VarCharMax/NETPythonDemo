using Python.Runtime;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ".venv");
      string message;

      using PythonInitialiser pyInit = new();
      if ((message = pyInit.InitialisePy(pathToVirtualEnv)) != "")
      {
        Console.WriteLine(message);
        return;
      }

      using (Py.GIL())
      {
        try
        {
          dynamic module = Py.Import("rw_visual");
          module.create_plot(5, @"C:\tmp");
        }
        catch(PythonException pex)
        {
          Console.WriteLine(pex.Format());
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }

      // Console.ReadKey();
    }
  }
}
