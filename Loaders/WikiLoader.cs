using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using RYCBEditorX.MySQL;
using RYCBEditorX.Crossing;

namespace RYCBEditorX.Utils;
public class WikiLoader : ICrossing
{
    private static MySqlConnection _conn;
    public static List<string> GetWiki(string name)
    {
        _conn = MySQLModule.MySQLConnection;
        List<string> content = ["", ""];
        try
        {
            var res = MySQLModule.ConnectionUtils.Select("wiki", condition: $"target='{name}'");
            content[0] = res[0]["content"].ToString();
            content[1] = res[0]["lastchanged"].ToString();
        }
        catch (Exception ex)
        {
            App.LOGGER.Error(ex);
        }
        return content;
    }

    public static List<string> GetAllTargets()
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

    public void Register()
    {
        IWikiLoader.GetWiki = GetWiki;
        IWikiLoader.GetAllTargets = GetAllTargets;
    }
}
