using Microsoft.Win32;
using Python.Runtime;
using System.Diagnostics;

namespace NETPython
{
  public class PythonInitialiser(bool useThreads = false) : IDisposable
  {
    private readonly string pynetmaxversion = $"{PythonEngine.MaxSupportedVersion.Major}.{PythonEngine.MaxSupportedVersion.Minor}";
    private readonly string pynetminversion = $"{PythonEngine.MinSupportedVersion.Major}.{PythonEngine.MinSupportedVersion.Minor}";
    private readonly OperatingSystem os = OperatingSystemHelper.CheckPlatform();
    private string? _virtualEnvPath;
    private bool disposedValue;
    private static readonly char[] separator = ['='];

    public string InitialisePy(string? virtualEnvPath = null, string? subFolder = "Scripts")
    {
      if (PythonEngine.IsInitialized)
      {
        return "";
      }

      string message;

      if (!string.IsNullOrEmpty(virtualEnvPath))
      {
        _virtualEnvPath = virtualEnvPath;
        message = InitialiseVirtual(subFolder);
      }
      else
      {
        message = InitialiseStandard();
      }

      if (!string.IsNullOrEmpty(message))
      {
        return message;
      }

      if (PythonEngine.IsInitialized == false)
      {
        message = "Python engine failed to initialize.";
      }

      return message;
    }

    private string InitializePythonEngine()
    {
      if (Runtime.PythonDLL != null)
      {
        try
        {
          if (!PythonEngine.IsInitialized)
          {
            PythonEngine.Initialize();
            if (useThreads == true)
            {
              PythonEngine.BeginAllowThreads();
            }
          }
        }
        catch (TypeInitializationException tie)
        {
          if (tie.InnerException is DllNotFoundException)
          {
            return "The specified Python DLL was not found. Please ensure that the correct version of Python is installed and configured.";
          }
          else
          {
            return tie.InnerException?.Message ?? tie.Message;
          }
        }
        catch (Exception ex)
        {
          return ex.Message;
        }
      }

      return "";
    }

    private string InitialiseStandard()
    {
      string message = "";

      switch (os)
      {
        case OperatingSystem.Windows:
          message = InitialiseWin();
          break;
        case OperatingSystem.Linux:
          // Linux specific initialisation can go here
          throw new PlatformNotSupportedException("Linux operating system is not yet supported.");
          break;
        case OperatingSystem.MacOS:
          throw new PlatformNotSupportedException("Mac OS operating system is not yet supported.");
          break;
        case OperatingSystem.Unknown:
          // throw new PlatformNotSupportedException("The operating system is not supported.");
          return "The operating system is not supported.";
      }

      return message;
    }

    private string InitialiseWin()
    {
      string? pythonVersion = null;

      Process pProcess = new();
      pProcess.StartInfo.CreateNoWindow = true;
      pProcess.StartInfo.FileName = "py";
      pProcess.StartInfo.Arguments = "list";
      pProcess.StartInfo.UseShellExecute = false;
      pProcess.StartInfo.RedirectStandardOutput = true;
      pProcess.Start();
      string output = pProcess.StandardOutput.ReadToEnd();
      pProcess.WaitForExit();

      if (output == null)
      {
        return "No result from py Command.";
      }

      int rowCount = 0;
      int versionCol = 0;
      List<string> pyVersions = [];

      using StringReader reader = new(output);
      string? line;
      while ((line = reader.ReadLine()) != null)
      {
        if (rowCount == 0)
        {
          versionCol = line.IndexOf("Version");
        }
        else
        {
          if (versionCol > 0)
          {
            string strPyVersion = line.Substring(versionCol, 4); //Read up to whitespace.
            pyVersions.Add(strPyVersion);
          }
        }

        rowCount++;
      }

      pythonVersion = pyVersions.Where(p => p.CompareTo(pynetmaxversion) <= 0).Max();

      if (string.IsNullOrEmpty(pythonVersion) == true)
      {
        return $"Compatible Python version between {pynetminversion} and {pynetmaxversion} either not installed or not configured";
      }

#pragma warning disable CA1416 // Validate platform compatibility
      RegistryKey? key = Registry.CurrentUser.OpenSubKey($@"Software\Python\PythonCore\{pythonVersion}\InstallPath");
      string path = key?.GetValue("")?.ToString() ?? "";
#pragma warning restore CA1416
      if (string.IsNullOrEmpty(path) == true)
      {
        return $"Python {pythonVersion} install path not found in registry.";
      }

      string pythonDll = Path.Combine(path, $"python{pythonVersion.Replace(".", "")}.dll");

      Runtime.PythonDLL = pythonDll;

      // Use this path to locate the python3.dll
      //Environment.SetEnvironmentVariable("PATH",
      //  Environment.GetEnvironmentVariable("PATH") + ";" + path);

      return "";
    }

    private string DeactivateVirtualEnvironment()
    {
      var envVars = ExecuteBatchAndCaptureEnvVars(Path.Combine(_virtualEnvPath!, "Scripts", "deactivate.bat"));

      foreach (var kvp in envVars)
      {
        Environment.SetEnvironmentVariable(kvp.Key, kvp.Value, EnvironmentVariableTarget.Process);
        // Console.WriteLine($"{kvp.Key} = {kvp.Value}");
      }

      return String.Empty;
    }

    private string EnsureVirtualEnvironment(string dllSystemPath, out string strDllDest)
    {
      string strDll = dllSystemPath[(dllSystemPath.LastIndexOf('\\') + 1)..];
      strDllDest = Path.Combine(_virtualEnvPath!, "Scripts", strDll);
    
      if (File.Exists(strDllDest) == false)
      {
        return $"Python dll not found at {strDllDest}.";
      }

      var envVars = ExecuteBatchAndCaptureEnvVars(Path.Combine(_virtualEnvPath!, "Scripts", "activate.bat"));

      if (envVars.ContainsKey("Path") == false)
      {
        return "PATH env variable not found.";
      }

      var paths = envVars["Path"].Split(';');

      var venvPathIndex = Array.FindIndex(paths, el => el.Contains(".venv"));

      if (venvPathIndex == -1)
      {
        return "PATH env missing Python setting.";
      }

      paths[venvPathIndex] = @$"{_virtualEnvPath}\Scripts";

      envVars["Path"] = String.Join(';', paths);

      foreach (var kvp in envVars)
      {
        // Set the captured variables in the current process if needed
        Environment.SetEnvironmentVariable(kvp.Key, kvp.Value, EnvironmentVariableTarget.Process);
        // Console.WriteLine($"{kvp.Key} = {kvp.Value}");
      }

      return "";
    }

    private static Dictionary<string, string> ExecuteBatchAndCaptureEnvVars(string batchFilePath)
    {
      var envVars = new Dictionary<string, string>();

      var startInfo = new ProcessStartInfo
      {
        FileName = "cmd.exe",
        Arguments = $"/c \"{batchFilePath}\"", // /c runs the command and then terminates
        RedirectStandardOutput = true,
        UseShellExecute = false, // Required for redirection
        CreateNoWindow = true    // Hides the console window
      };

      using (var process = Process.Start(startInfo))
      {
        if (process == null) return envVars;
        bool captureMode = false;

        // Synchronously read the standard output
        using (StreamReader reader = process.StandardOutput)
        {
          string line;
          while ((line = reader.ReadLine()!) != null)
          {
            if (captureMode && line.Contains("---ENV-VARS-START---"))
            {
              captureMode = true;
              continue;
            }

            if (line.Contains('='))
            {
              var parts = line.Split(separator, 2);
              if (parts.Length == 2)
              {
                envVars[parts[0]] = parts[1];
              }
            }
          }
        }

        process.WaitForExit();
      }

      return envVars;
    }

    private string InitialiseVirtual(string? scriptsFolder)
    {
      if (!string.IsNullOrEmpty(_virtualEnvPath))
      {
        string pathToVirtualEnv = Path.Combine(_virtualEnvPath, "pyvenv.cfg");

        if (File.Exists(pathToVirtualEnv) == false)
        {
          return $"The virtual environment configuration file was not found at {pathToVirtualEnv}";
        }

        //Get Python configuration from pyvenv.cfg
        Dictionary<string, string> config = File.ReadLines(pathToVirtualEnv)
            .Select(line => line.Trim().Split('='))
            .Where(arr => arr.Length == 2)
            .Select(arr => (Key: arr[0].Trim(), Value: arr[1].Trim()))
            .GroupBy(x => x.Key)
            .ToDictionary(keyGroup => keyGroup.Key, keyGroup => keyGroup.First().Value);

        // Extract major and minor version (e.g., "3.11" from "3.11.4"),
        // remove the dot for dll naming.
        // virtualenv manager has different keys and values to venv.
        string? versionKey = config.Keys.FirstOrDefault(k => k.StartsWith("version"));

        if (string.IsNullOrEmpty(versionKey))
        {
          return "Python version not found in config.";
        }
          
        int versionBuild = config[versionKey].IndexOf('.');

        string pyVersion = config[versionKey][..config[versionKey].IndexOf('.', versionBuild + 1)];

        if (String.Compare(pynetminversion, pyVersion, StringComparison.OrdinalIgnoreCase) < 0
          || String.Compare(pyVersion, pynetmaxversion, StringComparison.OrdinalIgnoreCase) > 0)
        {
          // throw new Exception($"Compatible Python version between {pynetminversion} and {pynetmaxversion} either not installed or not configured");
          return $"Compatible Python version between {pynetminversion} and {pynetmaxversion} either not installed or not configured";
        }

        string pythonSystemDll = string.Empty;
        string pythonVeDll = "";
        string macosShim = "";

        switch (os)
        {
          case OperatingSystem.Windows:
            pythonSystemDll = Path.Combine(config["home"], $"python{pyVersion.Replace(".", "")}.dll");
            break;
          case OperatingSystem.MacOS:
            pythonVeDll = config["executable"];
            // On macOS, we need to use a shim to load the Python environment modules.
            // This is because the folder structure is different from Windows.
            macosShim = $"python{pyVersion[..'.']}/";
            break;
          case OperatingSystem.Linux:

            break;
            case OperatingSystem.Unknown:
              return "Unknown operating system";
        }

        if (string.IsNullOrEmpty(pythonSystemDll) == true)
        {
          return "Unable to locate Python Dll in Virtual Environment.";
        }

        string message = EnsureVirtualEnvironment(pythonSystemDll, out pythonVeDll);

        if (string.IsNullOrEmpty(message) == false)
        {
          return message;
        }

        Runtime.PythonDLL = pythonVeDll;

        message = InitializePythonEngine();

        if (!string.IsNullOrEmpty(message))
        {
          return message;
        }

        using (Py.GIL())
        {
          dynamic sys = Py.Import("sys");
          sys.path.append(scriptsFolder);
          sys.path.append($"{scriptsFolder}/.venv/Lib");
          sys.path.append($"{scriptsFolder}/.venv/Lib/{macosShim}site-packages");
        }
      }

      return "";
    }

    private void ShutdownPy()
    {
      if (PythonEngine.IsInitialized)
      {
        try
        {
          // AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true); // No longer works in .NET 8+
          using (Py.GIL())
          PythonEngine.Shutdown();
          // AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false); // Causes a corrupted memory exxception.
        }
        catch (PlatformNotSupportedException)
        {
          // Ignore the exception as the shutdown likely proceeded enough
        }
        catch (PythonException)
        {

        }
        finally
        {
          if (String.IsNullOrEmpty(_virtualEnvPath) != false)
          {
            DeactivateVirtualEnvironment();
          }
        }
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          ShutdownPy();
        }

        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PythonInitialiser()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
