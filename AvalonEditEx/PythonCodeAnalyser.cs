using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Scripting.Hosting;
using Microsoft.SqlServer.Management.Assessment;
using Newtonsoft.Json;

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
