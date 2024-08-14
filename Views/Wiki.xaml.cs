using System.Threading.Tasks;
using System.Windows.Controls;
using HandyControl.Tools.Extension;
using Markdig;
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
        Loading.Show();
        LoadDataAsync();
        var mdDoc = Markdown.ToHtml("# header");
        Wv.Loaded += async (s, e) =>
        {
            await Wv.EnsureCoreWebView2Async().ConfigureAwait(true);
            Wv.CoreWebView2.NavigateToString(mdDoc);
            if (GlobalConfig.Skin == "dark")
            {
                await Wv.ExecuteScriptAsync(@"
                document.body.style.backgroundColor = '#1C1C1C';
                document.body.style.color = '#FFFFFF';
                ");
            }
        };
    }

    public async Task Init()
    {
        await Wv.EnsureCoreWebView2Async();
    }

    public async Task LoadDataAsync()
    {
        var filePath = @"F:\VSProj\repos\RYCBEditorX\bin\Debug\net8.0-windows\Cache\online-cache\comment[range].json";
        var comments = await CommentLoader.LoadCommentsAsync(filePath);

        // 将数据绑定到 ListBox
        CommentsListBox.ItemsSource = comments;
        Loading.Hide();
    }

    public static string GetAdvancedSearchQuery(
        string user = "", string repo = "", string created = "",
        string stars = "", string forks = "", string ext = "", string path = "",
        string filename = "", string lang = "", string license = "", string fork = "")
    {
        return GITHUB_ADVANCED_SEARCH_QUERY.FormatEx(user, repo, created, stars, forks, ext, path, filename, lang, license, fork);
    }
}
