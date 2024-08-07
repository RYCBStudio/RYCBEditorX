using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.TeamFoundation.Common;
using Prism.Commands;
using Prism.Mvvm;
using RYCBEditorX.AvalonEditEx;
using RYCBEditorX.Views;
using System;
using ICSharpCode.AvalonEdit;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RYCBEditorX.Dialogs.Views;
using ICSharpCode.AvalonEdit.Search;
using RYCBEditorX.Dialogs.ViewModels;
using RYCBEditorX.Utils;
using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;
using Markdig;
using System.Collections.Generic;

namespace RYCBEditorX.ViewModels;
public partial class MainWindowViewModel : BindableBase
{
    #region 变量
    private Process _runnerProc;
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
    public DelegateCommand CutCmd
    {
        get; private set;
    }
    public DelegateCommand CopyCmd
    {
        get; private set;
    }
    public DelegateCommand PasteCmd
    {
        get; private set;
    }
    public DelegateCommand UndoCmd
    {
        get; private set;
    }
    public DelegateCommand RedoCmd
    {
        get; private set;
    }
    public DelegateCommand CommentLinesCmd
    {
        get; private set;
    }
    public DelegateCommand UncommentLinesCmd
    {
        get; private set;
    }
    public DelegateCommand GotoLineCmd
    {
        get; private set;
    }
    public DelegateCommand AboutCmd
    {
        get; set;
    }
    public DelegateCommand RunCmd
    {
        get; set;
    }
    public DelegateCommand DebugCmd
    {
        get; set;
    }
    public DelegateCommand StopCmd
    {
        get; set;
    }
    public DelegateCommand ConfigRunProfilesCmd
    {
        get; set;
    }
    public DelegateCommand StepInCmd
    {
        get; set;
    }
    public DelegateCommand StepPassCmd
    {
        get; set;
    }
    public DelegateCommand StepOutCmd
    {
        get; set;
    }
    public DelegateCommand PythonPkgMgmtCmd
    {
        get; set;
    }
    #endregion
    public MainWindowViewModel()
    {
        NewFileCmd = new DelegateCommand(NewFile);
        NewProjectCmd = new DelegateCommand(NewProj);
        OpenFileCmd = new DelegateCommand(OpenFile);
        SaveFileCmd = new DelegateCommand(SaveFile);
        SaveAsCmd = new DelegateCommand(SaveAsFile);
        CloseTabCmd = new DelegateCommand(CloseTab);
        CutCmd = new DelegateCommand(Cut);
        CopyCmd = new DelegateCommand(Copy);
        PasteCmd = new DelegateCommand(Paste);
        CommentLinesCmd = new DelegateCommand(CommentLines);
        UncommentLinesCmd = new DelegateCommand(UncommentLines);
        UndoCmd = new DelegateCommand(Undo);
        RedoCmd = new DelegateCommand(Redo);
        AboutCmd = new DelegateCommand(About);
        RunCmd = new DelegateCommand(Run);
        DebugCmd = new DelegateCommand(Debug);
        StopCmd = new DelegateCommand(Stop);
        PythonPkgMgmtCmd = new DelegateCommand(PythonPkgMgmt);
        GotoLineCmd = new DelegateCommand(GotoLine);
        ConfigRunProfilesCmd = new DelegateCommand(ConfigureRunProfiles);
    }

    internal void ConfigureRunProfiles()
    {
        List<ProfileInfo> profiles = [];
        List<string> profile_paths = [];
        foreach (var item in Directory.EnumerateFiles(App.STARTUP_PATH + "\\Profiles\\Runners"))
        {
            MainWindow.Instance.RunProfilesComboBox.Items.Add(Path.GetFileNameWithoutExtension(item));
            profiles.Add(new()
            {
                Name = Path.GetFileNameWithoutExtension(item)
            });
            profile_paths.Add(item);
        }
        new RunnerProfileConfig(profiles, profile_paths).ShowDialog();
    }

    internal void Debug()
    {
        MainWindow.Instance.DebugPanel.Visibility = Visibility.Visible;
    }

    internal void PythonPkgMgmt()
    {
        new PythonPackageManager().Show();
    }

    internal void Run()
    {
        string tmpFileName;
        if (MainWindow.Instance.MainTabCtrl.SelectedItem is not null)
        {
            if (!((TabItem)MainWindow.Instance.MainTabCtrl.SelectedItem).ToolTip.ToString().IsNullOrEmpty())
            {
                tmpFileName = ((TabItem)MainWindow.Instance.MainTabCtrl.SelectedItem).ToolTip.ToString();
            }
            else
            {
                tmpFileName = App.STARTUP_PATH + "\\Cache\\" + DateTime.Now.Ticks + ".py";
                File.WriteAllText(tmpFileName, GetCurrentTextEditor()?.Text);
            }
        }
        else
        {
            return;
        }
        MainWindow.Instance.RunPanel.Visibility = Visibility.Visible;
        Process runner = new()
        {
            StartInfo = new()
            {
                Arguments = tmpFileName,
                FileName = App.STARTUP_PATH + "\\Tools\\Runner.exe",
                UseShellExecute = false,
            }
        }; 
        Process runner_proc_protecter = new()
        {
            StartInfo = new()
            {
                Arguments = tmpFileName,
                FileName = App.STARTUP_PATH + "\\Tools\\Runner.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            }
        };
        runner.Start();
        runner_proc_protecter.Start();
        //SetForegroundWindow(runner.Handle);
        SetWindowPos(runner.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        _runnerProc = runner;
        MainWindow.Instance.BottomTabCtrl.SelectedIndex = 1;
        MainWindow.Instance.ConsoleHost.Text = runner_proc_protecter.StandardError.ReadToEnd();
    }

    internal void Stop()
    {
        Process p = new()
        {
            StartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            }
        };
        p.Start();

        p.StandardInput.WriteLine("taskkill /pid " + _runnerProc.Id + " /f /t" + "&exit");
        p.StandardInput.AutoFlush = true;
        p.WaitForExit();
        p.Close();
    }

    internal void About()
    {
        new AboutWindow()
        {
            DataContext =
            new AboutViewModel()
            {
                BuildTime = "Build Time: " + DateTime.Now.ToString("yyyy-MM-dd"),
                Version = "Version: " + App.VERSION
            }
        }.Show();
    }

    internal void Cut()
    {
        GetCurrentTextEditor()?.Cut();
    }
    internal void Copy()
    {
        GetCurrentTextEditor()?.Copy();
    }
    internal void Paste()
    {
        GetCurrentTextEditor()?.Paste();
    }
    internal void Undo()
    {
        GetCurrentTextEditor()?.Undo();
    }
    internal void Redo()
    {
        GetCurrentTextEditor()?.Redo();
    }

    internal void GotoLine()
    {
        var t = GetCurrentTextEditor();
        if (t is null)
        {
            return;
        }
        if (new GotoLineWindow(t.LineCount).ShowDialog() == true)
        {
            t.ScrollToLine(GotoLineWindow.Line);
            t.TextArea.Caret.Position = new TextViewPosition(GotoLineWindow.Line, 0);
            //    double vertOffset = (GetCurrentTextEditor().TextArea.TextView.DefaultLineHeight) * GotoLineWindow.Line;
            //    GetCurrentTextEditor().ScrollToVerticalOffset(vertOffset);
        }
    }

    internal void CommentLines()
    {
        var selectedTextEditor = GetCurrentTextEditor();
        if (selectedTextEditor is null)
        {
            return;
        }
        var selectedLines = selectedTextEditor.SelectedText;
        string commentedLines;
        commentedLines = selectedLines
            .Insert(0, "\"\"\"");
        commentedLines += "\"\"\"";
        if (selectedLines.IsNullOrEmpty()) { return; }
        selectedLines = selectedLines.Replace(selectedLines, "{RYCB Editor Temporary Replace String}");
        selectedLines = selectedLines.Replace("{RYCB Editor Temporary Replace String}", commentedLines);
        selectedTextEditor.SelectedText = selectedLines;
    }

    internal void UncommentLines()
    {
        var selectedTextEditor = GetCurrentTextEditor();
        if (selectedTextEditor is null)
        {
            return;
        }
        var selectedLines = selectedTextEditor.SelectedText;
        if (selectedLines.StartsWith("\"\"\"") || selectedLines.StartsWith("#"))
        {
            var commentedLines = selectedLines
                .Replace("\"\"\"", "").Replace("#", "");
            if (selectedLines.IsNullOrEmpty()) { return; }
            selectedTextEditor.Text = selectedTextEditor.Text.Replace(selectedLines, "{RYCB Editor Temporary Replace String}");
            selectedTextEditor.Text = selectedTextEditor.Text.Replace("{RYCB Editor Temporary Replace String}", commentedLines);
        }
    }

    internal void NewFile()
    {
        var ofd = new Dialogs.Views.FileCreator();
        if (ofd.ShowDialog() == true)
        {
            foreach (TabItem item in MainWindow.Instance.MainTabCtrl.Items)
            {
                if (((TextBlock)((DockPanel)item.Header).Children[0]).Text.ToString() == ofd.FileName)
                {
                    MainWindow.Instance.MainTabCtrl.SelectedIndex = MainWindow.Instance.MainTabCtrl.Items.IndexOf(item);
                    return;
                }
            }
            //tabItem.Style = (Style)MainWindow.Instance.Resources["ClosableTab"];
            //MainWindow.Tabs.Add(new(ofd.FileName, tabItem));
            var index = MainWindow.Instance.MainTabCtrl.Items.Add(Init(ofd.FileName));
            //MainWindow.Instance.MainTabCtrl.ItemsSource = MainWindow.Tabs;
            MainWindow.Instance.MainTabCtrl.Items.Refresh();
            MainWindow.Instance.MainTabCtrl.SelectedIndex = index;
        }
    }

    private TextEditor GetCurrentTextEditor()
    {
        if (MainWindow.Instance.MainTabCtrl.SelectedItem is null) { return null; }
        return (MainWindow.Instance.MainTabCtrl.SelectedItem as TabItem).Tag as TextEditor;
    }

    private TabItem Init(string filename = "", bool load = false, string loadPath = "")
    {
        var dock = new DockPanel();
        dock.Children.Add(new TextBlock()
        {
            Text = Path.GetFileName(filename),
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
        if (Path.GetExtension(filename).Contains("md"))
        {
            var grid = new Grid()
            {
                ColumnDefinitions =
                {
                    new(){Width=new(3, GridUnitType.Star)},
                    new(){Width=new(3, GridUnitType.Star)},
                    new(){Width=new(4, GridUnitType.Star)},
                }
            };
            grid.ShowGridLines = true;
            var testTextEditorExMd = new TestTextEditorEx()
            {
                ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
                FontFamily = new(GlobalConfig.Editor.FontFamilyName),
                FontSize = GlobalConfig.Editor.FontSize,
            };
            grid.Children.Add(testTextEditorExMd);
            testTextEditorExMd.Load(filename);
            var testTextEditorExHTML = new TestTextEditorEx()
            {
                ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
                FontFamily = new(GlobalConfig.Editor.FontFamilyName),
                FontSize = GlobalConfig.Editor.FontSize,
            };
            grid.Children.Add(testTextEditorExHTML);
            testTextEditorExHTML.SetValue(Grid.ColumnProperty, 1);
            var resourceName = GlobalConfig.XshdFilePath + $"\\{GlobalConfig.Editor.Theme}\\HTML.xshd";
            using (Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                using System.Xml.XmlTextReader reader = new(s);
                var xshd = HighlightingLoader.LoadXshd(reader);
                testTextEditorExHTML.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
            }
            WebView2 webView = new();
            TabItem tabItem = new TabItem();
            grid.Children.Add(webView);
            webView.SetValue(Grid.ColumnProperty, 2);
            tabItem = new TabItem
            {
                ToolTip = filename,
                Header = dock,
                Uid = filename,
                Content = grid,
                Tag = testTextEditorExMd
            };
            var mdDoc = Markdown.ToHtml(testTextEditorExMd.Text);
            testTextEditorExHTML.Text = mdDoc;
            webView.Loaded += async (s, e) =>
            {
                await webView.EnsureCoreWebView2Async().ConfigureAwait(true);
                webView.CoreWebView2.NavigateToString(mdDoc);
                if (GlobalConfig.Skin == "dark")
                {
                    await webView.ExecuteScriptAsync(@"
                document.body.style.backgroundColor = '#1C1C1C';
                document.body.style.color = '#FFFFFF';
                ");
                }
            };
            return tabItem;
        }
        else
        {
            var tmp = new FileInfo(filename);
            var fileSize = tmp.Length;
            tmp = null;
            var testTextEditorEx = new TestTextEditorEx()
            {
                ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
                FontFamily = new(GlobalConfig.Editor.FontFamilyName),
                FontSize = GlobalConfig.Editor.FontSize,
            };
            if (GlobalConfig.Skin == "dark")
            {
                testTextEditorEx.Background = (Brush)Application.Current.Resources["DarkBackGround"];
                testTextEditorEx.Foreground = (Brush)Application.Current.Resources["LightBackGround"];
            }
            else
            {
                testTextEditorEx.Foreground = (Brush)Application.Current.Resources["DarkBackGround"];
                testTextEditorEx.Background = (Brush)Application.Current.Resources["LightBackGround"];
            }
            testTextEditorEx.TextArea.TextView.LinkTextForegroundBrush = (Brush)Application.Current.Resources["LinkForeGround"];
            var tabItem = new TabItem
            {
                ToolTip = filename,
                Header = dock,
                Uid = filename,
                Content = testTextEditorEx
            };
            if (Path.GetExtension(filename) != "dll" || !(fileSize <= GlobalConfig.MaximumFileSize))
            {
                var resourceName = GlobalConfig.XshdFilePath + $"{GlobalConfig.Editor.Theme}\\{GetLanguage(filename)}.xshd";
#pragma warning disable IDE0063 // 使用简单的 "using" 语句
                using (Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (System.Xml.XmlTextReader reader = new(s))
                    {

                        var xshd = HighlightingLoader.LoadXshd(reader);
                        testTextEditorEx.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                    }
                }
            }
            if (load & (!loadPath.IsNullOrEmpty()))
            {
                testTextEditorEx.Load(loadPath);
            }
            tabItem.Tag = testTextEditorEx;
            var _searchPanel = new TextEditorSearchPanel
            {
                DataContext = new SearchPanelViewModel
                {
                    Editor = testTextEditorEx
                }
            };
            _searchPanel.Show();
            return tabItem;
        }
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
        else if (SuffixName.Contains(".pyx"))
        {
            return "Cython";
        }
        else if (SuffixName.Contains(".py") | SuffixName.Contains(".pyw") | SuffixName.Contains(".pyi"))
        {
            return "Python";
        }
        else if (SuffixName.Contains(".xml") | SuffixName.Contains(".xshd") | SuffixName.Contains(".xaml") | SuffixName.Contains(".config"))
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
        new ProjectCreator().ShowDialog();
    }

    internal void OpenFile()
    {
        var ofd = new System.Windows.Forms.OpenFileDialog() { Filter = "|*.py||*.*" };
        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            foreach (TabItem item in MainWindow.Instance.MainTabCtrl.Items)
            {
                if (((TextBlock)((DockPanel)item.Header).Children[0]).Text.ToString() == ofd.FileName)
                {
                    MainWindow.Instance.MainTabCtrl.SelectedIndex = MainWindow.Instance.MainTabCtrl.Items.IndexOf(item);
                    return;
                }
            }
            var tab = Init(ofd.FileName, true, ofd.FileName);
            var index = MainWindow.Instance.MainTabCtrl.Items.Add(tab);
            //MainWindow.Instance.MainTabCtrl.ItemsSource = MainWindow.Tabs;
            MainWindow.Instance.MainTabCtrl.Items.Refresh();
            MainWindow.Instance.MainTabCtrl.SelectedIndex = index;
            if (Path.GetExtension(ofd.FileName).Contains(".py"))
            {
                Json2TreeViewParser.LoadJsonIntoTreeView(MainWindow.Instance.FileStructTree, new FileStructAnalyzer(ofd.FileName).Analyze());
            }
        }
    }

    internal void SaveFile()
    {
        if (MainWindow.Instance.MainTabCtrl.SelectedItem is null)
        {
            return;
        }
        MainWindow.Instance.FileSavingPanel.Visibility = Visibility.Visible;
        if (!((TabItem)MainWindow.Instance.MainTabCtrl.SelectedItem).ToolTip.ToString().IsNullOrEmpty())
        {
            try
            {
                GetCurrentTextEditor().Save(((TabItem)MainWindow.Instance.MainTabCtrl.SelectedItem).ToolTip.ToString());
                MainWindow.Instance.FileSavingTip.Text = Application.Current.Resources["Main.Bottom.FileSavingTip.Success"].ToString();
            }
            catch (Exception ex)
            {
                App.LOGGER.Error(ex);
                MainWindow.Instance.FileSavingTip.Text = Application.Current.Resources["Main.Bottom.FileSavingTip.Fail"].ToString();
            }
        }
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

    [DllImport("USER32.DLL")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public const uint SWP_NOMOVE = 0x2;
    public const uint SWP_NOSIZE = 0x1;
    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public static readonly IntPtr HWND_NOTOPMOST = new(-2);
}
