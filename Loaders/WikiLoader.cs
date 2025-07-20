using MySql.Data.MySqlClient;
using RYCBEditorX.Crossing;
using RYCBEditorX.MySQL;
using RYCBEditorX.Utils;
using System.Collections.Generic;
using System;
using RYCBEditorX;

public class WikiLoader : ICrossing
{
    private static MySqlConnection _conn;

    public static List<string> GetWiki(string name)
    {
        try
        {
            // 尝试从数据库获取
            _conn = MySQLModule.MySQLConnection;
            var res = MySQLModule.ConnectionUtils.Select("wiki", condition: $"target='{name}'");
            var content = new List<string>
            {
                res[0]["content"].ToString(),
                res[0]["lastchanged"].ToString()
            };
            return content;
        }
        catch (Exception ex)
        {
            App.LOGGER.Error(ex);
            // 从缓存获取
            var cachedData = WikiCache.LoadFromCache();
            if (cachedData.TryGetValue(name, out var cachedContent))
            {
                return cachedContent;
            }
            return ["", ""];
        }
    }

    public static List<string> GetAllTargets()
    {
        try
        {
            _conn = MySQLModule.MySQLConnection;
            var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM wiki";
            var res = cmd.ExecuteReader();
            var targets = new List<string>();
            while (res.Read())
            {
                targets.Add(res.GetString("target"));
            }
            return targets;
        }
        catch (Exception ex)
        {
            App.LOGGER.Error(ex);
            // 从缓存获取所有键
            var cachedData = WikiCache.LoadFromCache();
            return new List<string>(cachedData.Keys);
        }
    }

    public void Register()
    {
        IWikiLoader.GetWiki = GetWiki;
        IWikiLoader.GetAllTargets = GetAllTargets;
    }
}