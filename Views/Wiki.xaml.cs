using System.Windows;
using System.Windows.Controls;
using HandyControl.Tools.Extension;
using RYCBEditorX.Utils;
using RYCBEditorX.MySQL;
using System.Windows.Controls.Primitives;
using System;

namespace RYCBEditorX.Views;
/// <summary>
/// Wiki.xaml 的交互逻辑
/// </summary>
public partial class Wiki : UserControl
{
    public static Wiki Instance
    {
        get; private set;
    }

    public const string GITHUB_ADVANCED_SEARCH_QUERY =
        "user:{0} repo:{1} created:{2} stars:{3} forks:{4} " +
        "path:{5} path:{6} path:**/{7} language:{8} " +
        "license:{9} fork:{10}";

    private MySQL.CommentLoader _loader;

    public Wiki()
    {
        InitializeComponent();
        Loading.Show();
        Instance = this;
    }

    public void LoadData()
    {
        //var filePath = @"F:\VSProj\repos\RYCBEditorX\bin\Debug\net8.0-windows\Cache\online-cache\comment[range].json";
        if (Header.Text.IsNullOrEmpty())
        {
            _loader = new MySQL.CommentLoader(MySQLModule.ConnectionUtils);
        }
        else
        {
            _loader = new MySQL.CommentLoader(MySQLModule.ConnectionUtils, $"target='{Header.Text.Replace("'", "\uffff")}'");
        }
        var comments = _loader.LoadCommentsAsync();
        // 将数据绑定到 ListBox
        CommentsListBox.ItemsSource = comments;
        Loading.Hide();
        Main.SetValue(Grid.RowSpanProperty, 2);
        Main.SetValue(Grid.RowProperty, 0);
        LoadBtn.Hide();
    }

    public static string GetAdvancedSearchQuery(
        string user = "", string repo = "", string created = "",
        string stars = "", string forks = "", string ext = "", string path = "",
        string filename = "", string lang = "", string license = "", string fork = "")
    {
        return GITHUB_ADVANCED_SEARCH_QUERY.Format(user, repo, created, stars, forks, ext, path, filename, lang, license, fork);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    //private void UpdateLikes(object sender, RoutedEventArgs e)
    //{
    //    if (sender is ToggleButton button)
    //    {
    //        if (button.Tag is Comment comment)
    //        {
    //            if (button.IsChecked.Value)
    //            {
    //                // 点赞
    //                button.Content = "\xe62a";
    //                _loader.UpdateLikes(comment.Uid, ++comment.Likes);
    //            }
    //            else
    //            {
    //                // 取消点赞
    //                button.Content = "\xe64f";
    //                _loader.UpdateLikes(comment.Uid, --comment.Likes);
    //            }

    //            LoadData();
    //        }
    //    }
    //}

    private void CheckedLikes(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton button)
        {
            if (button.Tag is Comment comment)
            {
                if (button.IsChecked.Value)
                {
                    // 点赞
                    button.Content = "\xe62a";
                    _loader.UpdateLikes(comment.Uid, ++comment.Likes);
                }
                else
                {
                    // 取消点赞
                    button.Content = "\xe64f";
                    _loader.UpdateLikes(comment.Uid, --comment.Likes);
                }

                LoadData();
            }
        }
    }

    private void NewCommentBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (NewCommentUser.Text.IsNullOrEmpty())
        {
            NewCommentUser.Text = "User-" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:FFFFFFF").ComputeMd5()[0..4];
        }
        if (!NewCommentBox.Text.IsNullOrEmpty())
        {
            SendComment.IsEnabled = true;
        }
        else
        {
            SendComment.IsEnabled = false;
        }
    }

    private void SendComment_Click(object sender, RoutedEventArgs e)
    {
        if (!NewCommentBox.Text.IsNullOrEmpty())
        {
            _loader.AddComment(NewCommentBox.Text, NewCommentUser.Text, Header.Text);
            LoadData();
            NewCommentBox.Text = "";
        }
    }

    private void NewCommentBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter & SendComment.IsEnabled)
        {
            SendComment_Click(sender, e);
        }
    }

    private void Header_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Utils.Language.Python.Keywords.Contains(Header.Text))
        {
            HeaderIcon.Text = Icons.KEYWORD;
        }
        else if (Utils.Language.Python.MagicMethods.Contains(Header.Text))
        {
            HeaderIcon.Text = Icons.MAGIC;
        }
        else if (Utils.Language.Python.BuiltIns.Contains(Header.Text))
        {
            HeaderIcon.Text = Icons.BUILTIN;
        }
        else
        {
            HeaderIcon.Text = Icons.VARIABLE;
        }
        LoadData();
    }

    private void DeleteComment(object sender, RoutedEventArgs e)
    {
        _loader.DeleteComment((((Button)sender).Tag as Comment).Uid);
        LoadData();
    }

    private void Init(object sender, RoutedEventArgs e)
    {
        
    }
}
