using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using RYCBEditorX.AvalonEditEx;
using RYCBEditorX.Utils;

namespace RYCBEditorX.Views;
/// <summary>
/// CodeSenseDescription.xaml 的交互逻辑
/// </summary>
public partial class CodeSenseDescription : UserControl
{
    public CodeSenseDescription(string title, string desc, string template = "")
    {
        InitializeComponent();
        txtCodeSense.Text = title;
        mdv.Markdown = desc;
        txtEditor.Text = template;

        txtEditor.ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber;
        txtEditor.FontFamily = new(GlobalConfig.Editor.FontFamilyName);
        txtEditor.FontSize = GlobalConfig.Editor.FontSize;
        if (GlobalConfig.Skin == "dark")
        {
            txtEditor.Background = (Brush)GlobalConfig.Resources["DarkBackGround"];
            txtEditor.Foreground = (Brush)GlobalConfig.Resources["LightBackGround"];
        }
        else
        {
            txtEditor.Foreground = (Brush)GlobalConfig.Resources["DarkBackGround"];
            txtEditor.Background = (Brush)GlobalConfig.Resources["LightBackGround"];
        }
        txtEditor.TextArea.TextView.LinkTextForegroundBrush = (Brush)GlobalConfig.Resources["LinkForeGround"];
        var resourceName = GlobalConfig.XshdFilePath + $"{GlobalConfig.Editor.Theme}\\python.xshd";
        using Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using System.Xml.XmlTextReader reader = new(s);
        var xshd = HighlightingLoader.LoadXshd(reader);
        txtEditor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
        if (!template.IsNullOrEmpty())
        {
            txtEditor.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
