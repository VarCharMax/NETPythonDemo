using Python.Runtime;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace NETPython
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string pathToVirtualEnv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ".venv");
      string message;

      Option<string> outputpathOption = new("--output", "-o")
      {
        Description = "An option whose argument is parsed as a string",
        DefaultValueFactory = parseResult => @"C:\tmp",
      };

      Option<int> countOption = new("--delay", "-d")
      {
        Description = "An option whose argument is parsed as an int",
        DefaultValueFactory = parseResult => 5,
      };

      RootCommand rootCommand = new();
        
      rootCommand.Options.Add(outputpathOption);
      rootCommand.Options.Add(countOption);

      ParseResult parseResult = rootCommand.Parse(args);
      if (parseResult.Errors.Count == 0)
      {
        string outputPath = parseResult.GetValue(outputpathOption)!;
        int numCount = parseResult.GetValue(countOption)!;

        if (Path.Exists(outputPath) == false)
        {
          return;        
        }

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
            module.create_plot(numCount, outputPath);
          }
          catch (PythonException pex)
          {
            Console.WriteLine(pex.Format());
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
        }

        return;
      }

      foreach (ParseError parseError in parseResult.Errors)
      {
        Console.Error.WriteLine(parseError.Message);
      }
      return;

      

     

      // Console.ReadKey();
    }
  }
}
