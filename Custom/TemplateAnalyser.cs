using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RYCBEditorX.Custom;
public class TemplateAnalyser
{
    public static Dictionary<string, string> GetCodeImplements(string filePath)
    {
        try
        {
            // 读取JSON文件内容
            var jsonContent = File.ReadAllText(filePath);

            // 解析JSON内容
            var jsonObject = JObject.Parse(jsonContent);

            // 获取"code-implements"部分
            var codeImplements = jsonObject["code-implements"] as JObject;

            if (codeImplements == null)
            {
                App.LOGGER.Error(new Exception("\"code-implements\"部分不存在"));
            }

            // 创建一个字典来存储结果
            var result = new Dictionary<string, string>();

            // 遍历"code-implements"部分的所有键值对
            foreach (var property in codeImplements.Properties())
            {
                result[property.Name] = property.Value.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            App.LOGGER.Error(ex);
            return null;
        }
    }
}
