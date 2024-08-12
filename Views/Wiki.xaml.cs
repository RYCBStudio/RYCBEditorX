using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RYCBEditorX.Utils;

namespace RYCBEditorX.Views;
/// <summary>
/// Wiki.xaml 的交互逻辑
/// </summary>
public partial class Wiki : UserControl
{

    public const string GITHUB_ADVANCED_SEARCH_QUERY =
        "user:{0} repo:{1} created:{2} stars:{3} forks:{4} " +
        "path:{5} path:{6} path:**/{7} language:{8} " +
        "license:{9} fork:{10}";

    public Wiki()
    {
        InitializeComponent();
    }

    public static string GetAdvancedSearchQuery(
        string user = "", string repo = "", string created = "",
        string stars = "", string forks = "", string ext = "", string path = "",
        string filename = "", string lang = "", string license = "", string fork = "")
    {
        return GITHUB_ADVANCED_SEARCH_QUERY.FormatEx(user, repo, created, stars, forks, ext, path, filename, lang, license, fork);
    }
}
