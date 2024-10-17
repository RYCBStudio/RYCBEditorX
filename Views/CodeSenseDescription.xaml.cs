using System.IO;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;

namespace RYCBEditorX.Views;
/// <summary>
/// CodeSenseDescription.xaml 的交互逻辑
/// </summary>
public partial class CodeSenseDescription : UserControl
{
    public CodeSenseDescription(string title, string desc)
    {
        InitializeComponent();
        txtCodeSense.Text = title;
        txtEditor.Text = desc;
        var resourceName = GlobalConfig.XshdFilePath + $"{GlobalConfig.Editor.Theme}\\python.xshd";
        using Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using System.Xml.XmlTextReader reader = new(s);
        var xshd = HighlightingLoader.LoadXshd(reader);
        txtEditor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
    }
}
