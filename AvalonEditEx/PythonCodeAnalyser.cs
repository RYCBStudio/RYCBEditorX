using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using RYCBEditorX.Utils;

namespace RYCBEditorX;
public class PythonCodeAnalyser
{
    public static Dictionary<string, List<string>> GetVariables(string pathOrContent, bool isContent = false)
    {
        if (isContent)
        {
            var content = pathOrContent;
            pathOrContent = GlobalConfig.CommonTempFilePath;
            File.WriteAllText(pathOrContent, content);
        }
        Process vg = new()
        {
            StartInfo = new()
            {
                Arguments = pathOrContent,
                FileName = App.STARTUP_PATH + "\\Tools\\var_getter.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        vg.Start();
        var _ = vg.StandardError.ReadToEnd();
        var jsonOutput = vg.StandardOutput.ReadToEnd();
        // 将JSON字符串转换为C#字典
        var variableScopes = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonOutput);

        return variableScopes;
    }

    /// <summary>
    /// 该方法用于前期过渡。可能会降低性能。
    /// </summary>
    [Obsolete("该方法即将弃用。")]
    public static List<string> ExtractVariables(Dictionary<string, List<string>> new_vars)
    {
        List<string> variables = [];
        foreach (var v in new_vars)
        {
            variables.Add(v.Key);
        }
        return variables;
    }
}

public class DocstringProcessor
{
    private static int _now;

    /// <summary>
    /// 读取指定目录下的所有JSON文件，并解析其中的docstrings。
    /// </summary>
    /// <param name="directoryPath">指定目录</param>
    public static Dictionary<string, Dictionary<string, string>> ProcessJsonFiles(string directoryPath, ref int now, ref int total)
    {
        var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
        total = files.Length;
        Dictionary<string, Dictionary<string, string>> docStrings = [];
        foreach (var file in files)
        {
            var jsonContent = File.ReadAllText(file);
            var docString = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            docStrings[file] = docString;
            now++;
        }
        return docStrings;
    }

    public static void StartExtract()
    {
        Process vg = new()
        {
            StartInfo = new()
            {
                FileName = App.STARTUP_PATH + "Tools\\func_getter.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        };
        vg.Start();
        FileSystemWatcher fs = new()
        {
            Path = @"F:\Temp\Python",
            Filter = "*.json",
            IncludeSubdirectories = true,
        };
        fs.Changed += OnCreate; ;
        fs.EnableRaisingEvents = true;
    }

    private static void OnCreate(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed)
        {
            _now++;
        }
        if (e.ChangeType == WatcherChangeTypes.Deleted)
        {
            _now--;
        }
    }
}

public class PythonPackageParser
{
    public static Dictionary<string, string> GetPackageMethodDocstrings(Dictionary<string, Dictionary<string, string>> input)
    {
        var result = new Dictionary<string, string>();

        foreach (var fileEntry in input)
        {
            // Get the absolute file path and method dictionary
            string filePath = fileEntry.Key;
            var methods = fileEntry.Value;

            // Extract the package name from the file path
            string packageName = ExtractPackageName(filePath);

            foreach (var methodEntry in methods)
            {
                string methodName = methodEntry.Key;
                string docstring = methodEntry.Value;

                // Combine package name and method name
                string key = $"{(packageName.IsNullOrEmpty() ? "" : packageName + ".")}{methodName}";

                // Add to result dictionary
                result[key] = docstring;
            }
        }

        return result;
    }

    private static string ExtractPackageName(string filePath)
    {
        // Convert the file path to a relative path with forward slashes
        string relativePath = filePath.Replace(Path.DirectorySeparatorChar, '/');
        string sitePackagesPath = "site-packages/";

        if (relativePath.Contains(sitePackagesPath))
        {
            // Extract the package name by removing the site-packages prefix
            int startIndex = relativePath.IndexOf(sitePackagesPath) + sitePackagesPath.Length;

            // Find the end index of the package name
            int endIndex = relativePath.IndexOf('/', startIndex);

            // If there's no further slash, assume the package name goes to the end of the path
            if (endIndex == -1)
            {
                endIndex = relativePath.Length;
            }

            string packageName = relativePath[startIndex..endIndex];

            // If package name is empty, default to 'root'
            return packageName.IsNullOrEmpty() ? "" : packageName;
        }
        else
        {
            string packageName = "";
            if (Directory.Exists(filePath))
            {
                packageName = Path.GetDirectoryName(filePath);
            }else if (File.Exists(filePath))
            {
                packageName = Path.GetFileNameWithoutExtension(filePath);
            }
            return packageName.IsNullOrEmpty() ? "" : packageName;
        }
    }
}