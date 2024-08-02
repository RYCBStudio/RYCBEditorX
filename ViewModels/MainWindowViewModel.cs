using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.TeamFoundation.Common;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using RYCBEditorX.AvalonEditEx;
using RYCBEditorX.Views;
using Microsoft.SqlServer.Management;
using System;

namespace RYCBEditorX.ViewModels;
public class MainWindowViewModel : BindableBase
{
    public DelegateCommand NewFileCmd
    {
        get; private set;
    }
    public DelegateCommand NewProjectCmd
    {
        get; private set;
    }
    public DelegateCommand OpenFileCmd
    {
        get; private set;
    }
    public DelegateCommand SaveFileCmd
    {
        get; private set;
    }
    public DelegateCommand SaveAsCmd
    {
        get; private set;
    }
    public DelegateCommand CloseTabCmd
    {
        get; private set;
    }
    public DelegateCommand EditingCmd
    {
        get; private set;
    }

    public MainWindowViewModel()
    {
        NewFileCmd = new DelegateCommand(NewFile);
        NewProjectCmd = new DelegateCommand(NewProj);
        OpenFileCmd = new DelegateCommand(OpenFile);
        SaveFileCmd = new DelegateCommand(SaveFile);
        SaveAsCmd = new DelegateCommand(SaveAsFile);
        CloseTabCmd = new DelegateCommand(CloseTab);
        EditingCmd = new DelegateCommand(Editing);
    }

    internal void Editing()
    {
        //TODO: 分MenuItem添加Command
    }

    internal void NewFile()
    {
        var ofd = new Dialogs.Views.FileCreator();
        if (ofd.ShowDialog() == true)
        {
            //tabItem.Style = (Style)MainWindow.Instance.Resources["ClosableTab"];
            //MainWindow.Tabs.Add(new(ofd.FileName, tabItem));
            MainWindow.Instance.MainTabCtrl.Items.Add(Init(ofd.FileName));
            //MainWindow.Instance.MainTabCtrl.ItemsSource = MainWindow.Tabs;
            MainWindow.Instance.MainTabCtrl.Items.Refresh();
        }
    }

    private TabItem Init(string filename = "", bool load = false, string loadPath = "")
    {
        var dock = new DockPanel();
        var testTextEditorEx = new TestTextEditorEx()
        {
            ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
            FontFamily = new(GlobalConfig.Editor.FontFamilyName),
            FontSize = GlobalConfig.Editor.FontSize,
        };
        if (GlobalConfig.Skin == "dark")
        {
            testTextEditorEx.Background = (Brush)Application.Current.Resources["DarkBackgroud"];
            testTextEditorEx.Foreground = (Brush)Application.Current.Resources["LightBackgroud"];
        }
        else
        {
            testTextEditorEx.Foreground = (Brush)Application.Current.Resources["DarkBackgroud"];
            testTextEditorEx.Background = (Brush)Application.Current.Resources["LightBackgroud"];
        }
        dock.Children.Add(new TextBlock()
        {
            Text = System.IO.Path.GetFileName(filename),
            VerticalAlignment = VerticalAlignment.Center,
        });
        dock.Children.Add(new Button()
        {
            Command = CloseTabCmd,
            FontFamily = (FontFamily)MainWindow.Instance.Resources["iconfont"],
            Content = "\xe614",
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            Background = new SolidColorBrush(Colors.Transparent),
            Margin = new Thickness(5, 0, 0, 0),
        });
        var tabItem = new TabItem
        {
            ToolTip = filename,
            Header = dock,
            Uid = filename,
            Content = testTextEditorEx
        };
        var resourceName = GlobalConfig.XshdFilePath + $"\\{GetLanguage(filename)}.xshd";
        using (Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        {
            using System.Xml.XmlTextReader reader = new(s);
            var xshd = HighlightingLoader.LoadXshd(reader);
            testTextEditorEx.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
        }
        if (load & (!loadPath.IsNullOrEmpty()))
        {
            testTextEditorEx.Load(loadPath);
        }
        return tabItem;
    }
    /// <summary>
    /// 识别文件语言
    /// </summary>
    /// <param name="SuffixName">文件后缀名</param>
    /// <param name="log">是否记录于日志中</param>
    /// <returns>语言类型</returns>
    public static string GetLanguage(string SuffixName, bool log = true)
    {
        SuffixName = Path.GetExtension(SuffixName);
        if (log)
        {
            App.LOGGER.Log($"已获取文件名: {SuffixName}");
        }
        if (SuffixName.Contains(".cs"))
        {
            return "C-Sharp";
        }
        else if (SuffixName.Contains(".pycn") | SuffixName.Contains(".pyCN"))
        {
            return "Py-CN";
        }
        else if (SuffixName.Contains(".py") | SuffixName.Contains(".pyw") | SuffixName.Contains(".pyi"))
        {
            return "Python";
        }
        else if (SuffixName.Contains(".xml") | SuffixName.Contains(".xshd"))
        {
            return "XML";
        }
        else
        {
            return "PlainText";
        }

    }

    internal void NewProj()
    {
        new Dialogs.Views.ProjectCreator().ShowDialog();
    }

    internal void OpenFile()
    {
        var ofd = new OpenFileDialog() { Filter = "|*.py||*.*" };
        if (ofd.ShowDialog() == true)
        {
            MainWindow.Instance.MainTabCtrl.Items.Add(Init(ofd.FileName, true, ofd.FileName));
            //MainWindow.Instance.MainTabCtrl.ItemsSource = MainWindow.Tabs;
            MainWindow.Instance.MainTabCtrl.Items.Refresh();
        }
    }

    internal void SaveFile()
    {

    }

    internal void SaveAsFile()
    {

    }

    internal void CloseTab()
    {
        var index = MainWindow.Instance.MainTabCtrl.SelectedIndex;
        MainWindow.Instance.MainTabCtrl.Items.RemoveAt(index);
        MainWindow.Instance.MainTabCtrl.Items.Refresh();
        MainWindow.Instance.MainTabCtrl.SelectedIndex = index - 1;
    }
}
