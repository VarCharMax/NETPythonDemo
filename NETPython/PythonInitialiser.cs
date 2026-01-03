using Microsoft.Win32;
using Python.Runtime;
using System.Diagnostics;

namespace NETPython
{
  internal class PythonInitialiser
  {
    private string pynetmaxversion = $"{PythonEngine.MaxSupportedVersion.Major}.{PythonEngine.MaxSupportedVersion.Minor}";
    private string pynetminversion = $"{PythonEngine.MinSupportedVersion.Major}.{PythonEngine.MinSupportedVersion.Minor}";

    public PythonInitialiser()
    {
      if (Runtime.PythonDLL == null)
      {
        if (OperatingSystemHelper.CheckPlatform() == OperatingSystem.Windows)
        {
          PythonInitialiserWin();
          if (Runtime.PythonDLL != null)
          {
            PythonEngine.Initialize();
          }
        }
      }
    }

    public void PythonInitialiserWin()
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

      if (output != null)
      {
        using StringReader reader = new(output);
        string? line;
        int rowCount = 0;
        // Regex re = new Regex();
        int versionCol = 0;
        List<string> pyVersions = [];
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
      }

      if (string.IsNullOrEmpty(pythonVersion) == false)
      {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey($@"Software\Python\PythonCore\{pythonVersion}\InstallPath");

        string path = key?.GetValue("")?.ToString() ?? "";

        if (string.IsNullOrEmpty(path) == false)
        {
          string pythonDll = Path.Combine(path, $"python{pythonVersion.Replace(".", "")}.dll");

          Runtime.PythonDLL = pythonDll;

          // Use this path to locate the python3.dll
          //Environment.SetEnvironmentVariable("PATH",
          //  Environment.GetEnvironmentVariable("PATH") + ";" + path);
        }
      }
    }

    public PythonInitialiser(string virtualEnvPath)
    {
      if (!string.IsNullOrEmpty(virtualEnvPath))
      {
       

        string pathToVirtualEnv = Path.Combine(virtualEnvPath, "pyvenv.cfg");

        if (File.Exists(pathToVirtualEnv) == false)
        {
          throw new FileNotFoundException($"The virtual environment configuration file was not found at {pathToVirtualEnv}");
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
          throw new Exception($"Compatible Python version between {pynetminversion} and {pynetmaxversion} either not installed or not configured");
        }

        // Set the Python home to the virtual environment path
        PythonEngine.PythonHome = virtualEnvPath;

        PythonEngine.Initialize();
      }
    }
  }
}
