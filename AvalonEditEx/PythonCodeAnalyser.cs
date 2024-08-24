using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using RYCBEditorX.Utils;
using RYCBEditorX.ViewModels;

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
    /// <summary>
    /// 读取指定目录下的所有JSON文件，并解析其中的docstrings。
    /// </summary>
    /// <param name="directoryPath">指定目录</param>
    public static List<Dictionary<string, string>> ProcessJsonFiles(string directoryPath, ref int now, ref int total)
    {
        var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
        total = files.Length;
        List<Dictionary<string, string>> docStrings = [];
        foreach (var file in files)
        {
            var jsonContent = File.ReadAllText(file);
            var docString = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            docStrings.Add(docString);
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

