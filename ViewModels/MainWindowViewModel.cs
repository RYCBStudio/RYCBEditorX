using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.TeamFoundation.Common;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using RYCBEditorX.AvalonEditEx;
using RYCBEditorX.Views;

namespace RYCBEditorX.ViewModels;
public class MainWindowViewModel : BindableBase
{
    private string _title = "Prism Application";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

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

    public MainWindowViewModel()
    {
        NewFileCmd = new DelegateCommand(NewFile);
        NewProjectCmd = new DelegateCommand(NewProj);
        OpenFileCmd = new DelegateCommand(OpenFile);
        SaveFileCmd = new DelegateCommand(SaveFile);
        SaveAsCmd = new DelegateCommand(SaveAsFile);
        CloseTabCmd = new DelegateCommand(CloseTab);
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
        dock.Children.Add(new TextBlock() { Text = System.IO.Path.GetFileName(filename) });
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
        testTextEditorEx.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(MainWindow.Instance.BtnSelectHLProfile.Content.ToString().Trim());
        if (load & (!loadPath.IsNullOrEmpty()))
        {
            testTextEditorEx.Load(loadPath);
        }
        return tabItem;
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
