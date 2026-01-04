using Microsoft.Win32;
using Python.Runtime;
using System.Diagnostics;

namespace NETPython
{
  public class PythonInitialiser
  {
    private static readonly string pynetmaxversion = $"{PythonEngine.MaxSupportedVersion.Major}.{PythonEngine.MaxSupportedVersion.Minor}";
    private static readonly string pynetminversion = $"{PythonEngine.MinSupportedVersion.Major}.{PythonEngine.MinSupportedVersion.Minor}";
    private static readonly OperatingSystem os = OperatingSystemHelper.CheckPlatform();

    public static string Initialise(string? virtualEnvPath = null, string? subFolder = null)
    {
      if (PythonEngine.IsInitialized)
      {
        return "";
      }

      if (!string.IsNullOrEmpty(virtualEnvPath))
      {
        string message = InitialiseVirtual(virtualEnvPath, subFolder);
        if (!string.IsNullOrEmpty(message))
        {
          return message;
        }
      }
      else
      {
        switch (os)
        {
          case OperatingSystem.Windows:
            PythonInitialiserWin();
            break;
          case OperatingSystem.Linux:
            // Linux specific initialisation can go here
            break;
          case OperatingSystem.MacOS:
            // MacOS specific initialisation can go here
            break;
          case OperatingSystem.Unknown:
            // throw new PlatformNotSupportedException("The operating system is not supported.");
            return "The operating system is not supported.";
        }
      }

      if (Runtime.PythonDLL != null)
      {
        try
        {
          PythonEngine.Initialize();
        }
        catch (TypeInitializationException tie)
        {
          if (tie.InnerException is DllNotFoundException)
          {
            // Handle the DllNotFoundException specifically
            // throw new Exception("The specified Python DLL was not found. Please ensure that the correct version of Python is installed and configured.", tie);
            return "The specified Python DLL was not found. Please ensure that the correct version of Python is installed and configured.";
        }
          else
          {
            // throw; // Rethrow if it's a different exception
          }
        }
        catch (Exception ex)
        {
          return ex.Message;
        }
      }

      if (PythonEngine.IsInitialized == false)
      {
        return "Python engine failed to initialize.";
      }

      return "";
    }

    private static string PythonInitialiserWin()
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
#pragma warning restore CA1416 // Validate platform compatibility
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

    private static string InitialiseVirtual(string virtualEnvPath, string? subFolder = null)
    {
      if (!string.IsNullOrEmpty(virtualEnvPath))
      {
        string pathToVirtualEnv = Path.Combine(virtualEnvPath, "pyvenv.cfg");

        if (File.Exists(pathToVirtualEnv) == false)
        {
          // throw new FileNotFoundException($"The virtual environment configuration file was not found at {pathToVirtualEnv}");
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
        string pyVersion = config["version"][..config["version"].LastIndexOf('.')];

        if (String.Compare(pynetminversion, pyVersion, StringComparison.OrdinalIgnoreCase) < 0
          || String.Compare(pyVersion, pynetmaxversion, StringComparison.OrdinalIgnoreCase) > 0)
        {
          // throw new Exception($"Compatible Python version between {pynetminversion} and {pynetmaxversion} either not installed or not configured");
          return $"Compatible Python version between {pynetminversion} and {pynetmaxversion} either not installed or not configured";
        }

        string pythonDll = "";
        string macosShim = "";

        switch (os)
        {
          case OperatingSystem.Windows:
            pythonDll = Path.Combine(config["home"], $"python{pyVersion}.dll");
            break;
          case OperatingSystem.MacOS:
            pythonDll = config["executable"];
            // On macOS, we need to use a shim to load the Python environment modules.
            // This is because the folder structure is different from Windows.
            macosShim = $"python{pyVersion[..'.']}/";
            break;
          case OperatingSystem.Linux:

            break;
            case OperatingSystem.Unknown:
              return "Unknown operating system";
        }

        Runtime.PythonDLL = pythonDll;

        // Set the Python home to the virtual environment path
        PythonEngine.PythonHome = virtualEnvPath;
      }

      return "";
    }
  }
}
