#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable IDE0051 // 添加只读修饰符
#pragma warning disable IDE0063 // 使用简单的 "using" 语句
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
using RYCBEditorX.Dialogs.ViewModels;
using RYCBEditorX.Utils;
using Microsoft.Web.WebView2.Wpf;
using Markdig;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Linq;
using Sunny.UI;

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
    public DelegateCommand LACCmd
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
        LACCmd = new DelegateCommand(OpenLAC);

        if (GlobalConfig.ShouldAutoSave)
        {
            MainWindow.autoSaveTimer = new System.Windows.Forms.Timer
            {
                Interval = GlobalConfig.AutoSaveInterval,
            };
            MainWindow.autoSaveTimer.Tick += SaveFile;
        }
        if (GlobalConfig.ShouldAutoBackup)
        {
            MainWindow.autoBackupTimer = new System.Windows.Forms.Timer
            {
                Interval = GlobalConfig.AutoBackupInterval,
            };
            MainWindow.autoBackupTimer.Tick += SaveFile;
        }
    }

    internal void OpenLAC()
    {
        new LicensesAndCopyright().ShowDialog();
    }

    internal void ConfigureRunProfiles()
    {
        // 创建一个空列表用于存储ProfileInfo对象
        List<ProfileInfo> profiles = [];
        // 创建一个空列表用于存储profile路径
        List<string> profile_paths = [];
        // 遍历Profiles\Runners文件夹中的文件
        foreach (var item in Directory.EnumerateFiles(App.STARTUP_PATH + @"\Profiles\Runners"))
        {
            // 将文件名添加到RunProfilesComboBox中
            MainWindow.Instance.RunProfilesComboBox.Items.Add(Path.GetFileNameWithoutExtension(item));
            // 将文件名作为Name属性添加到ProfileInfo对象中
            profiles.Add(new()
            {
                Name = Path.GetFileNameWithoutExtension(item)
            });
            // 将文件路径添加到profile_paths中
            profile_paths.Add(item);
        }
        // 创建一个RunnerProfileConfig对象，传入profiles和profile_paths
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
        try
        {
            GetCurrentTextEditor().Save(tmpFileName);
            MainWindow.Instance.FileSavingPanel.Visibility = Visibility.Visible;
            MainWindow.Instance.FileSavingTip.Text = Application.Current.Resources["Main.Bottom.FileSavingTip.Success"].ToString();
        }
        catch (Exception ex)
        {
            App.LOGGER.Error(ex);
            MainWindow.Instance.FileSavingPanel.Visibility = Visibility.Visible;
            MainWindow.Instance.FileSavingTip.Text = Application.Current.Resources["Main.Bottom.FileSavingTip.Fail"].ToString();
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
                Arguments = tmpFileName + " -nowait",
                FileName = App.STARTUP_PATH + "\\Tools\\Runner.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };
        App.LOGGER.LogDebug("Starting runner...");
        runner.Start();
        App.LOGGER.LogDebug("Judging runner...");
        if (!GetCurrentTextEditor().Text.Contains("import turtle"))
        {
            runner_proc_protecter.Start();
            MainWindow.Instance.ConsoleHost.Text = runner_proc_protecter.StandardError.ReadToEnd();
        }
        //SetForegroundWindow(runner.Handle);
        _runnerProc = runner;
        MainWindow.Instance.BottomTabCtrl.SelectedIndex = 1;
        App.LOGGER.LogDebug("Setting runner...");
        SetWindowPos(runner.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }

    internal void Stop()
    {
        if (_runnerProc is null)
        {
            return;
        }

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
            FontFamily = (FontFamily)MainWindow.Instance.Resources["Iconfont"],
            Content = "\xe614",
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            Background = new SolidColorBrush(Colors.Transparent),
            Margin = new Thickness(5, 0, 0, 0),
        });
        TextEditorEx textEditorEx;
        if (Path.GetExtension(filename).Contains("md"))
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new(){Width=new(3, GridUnitType.Star)},
                    new(){Width=new(3, GridUnitType.Star)},
                    new(){Width=new(4, GridUnitType.Star)},
                },
                ShowGridLines = true
            };
            textEditorEx = new TextEditorEx()
            {
                ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
                FontFamily = new(GlobalConfig.Editor.FontFamilyName),
                FontSize = GlobalConfig.Editor.FontSize,
            };
            if (GlobalConfig.Skin == "dark")
            {
                textEditorEx.Background = (Brush)Application.Current.Resources["DarkBackGround"];
                textEditorEx.Foreground = (Brush)Application.Current.Resources["LightBackGround"];
            }
            else
            {
                textEditorEx.Foreground = (Brush)Application.Current.Resources["DarkBackGround"];
                textEditorEx.Background = (Brush)Application.Current.Resources["LightBackGround"];
            }
            textEditorEx.TextArea.TextView.LinkTextForegroundBrush = (Brush)Application.Current.Resources["LinkForeGround"];
            grid.Children.Add(textEditorEx);
            textEditorEx.Load(filename);
            var testTextEditorExHTML = new TextEditorEx()
            {
                ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
                FontFamily = new(GlobalConfig.Editor.FontFamilyName),
                FontSize = GlobalConfig.Editor.FontSize,
            };
            grid.Children.Add(testTextEditorExHTML);
            testTextEditorExHTML.SetValue(Grid.ColumnProperty, 1);
            if (GlobalConfig.Skin == "dark")
            {
                testTextEditorExHTML.Background = (Brush)Application.Current.Resources["DarkBackGround"];
                testTextEditorExHTML.Foreground = (Brush)Application.Current.Resources["LightBackGround"];
            }
            else
            {
                testTextEditorExHTML.Foreground = (Brush)Application.Current.Resources["DarkBackGround"];
                testTextEditorExHTML.Background = (Brush)Application.Current.Resources["LightBackGround"];
            }
            testTextEditorExHTML.TextArea.TextView.LinkTextForegroundBrush = (Brush)Application.Current.Resources["LinkForeGround"];
            var resourceName = GlobalConfig.XshdFilePath + $"\\{GlobalConfig.Editor.Theme}\\HTML.xshd";
            using (Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                using System.Xml.XmlTextReader reader = new(s);
                var xshd = HighlightingLoader.LoadXshd(reader);
                testTextEditorExHTML.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
            }
            WebView2 webView = new();
            var tabItem = new TabItem();
            grid.Children.Add(webView);
            webView.SetValue(Grid.ColumnProperty, 2);
            tabItem = new TabItem
            {
                ToolTip = filename,
                Header = dock,
                Uid = filename,
                Content = grid,
                Tag = textEditorEx
            };
            var mdDoc = Markdown.ToHtml(textEditorEx.Text);
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
            var fileSize = 0L;
            try
            {
                var tmp = new FileInfo(filename);
                fileSize = tmp.Length;
                tmp = null;
            }
            catch (FileNotFoundException)
            {
                fileSize = 0;
            }
            textEditorEx = new TextEditorEx()
            {
                ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber,
                FontFamily = new(GlobalConfig.Editor.FontFamilyName),
                FontSize = GlobalConfig.Editor.FontSize,
            };
            if (GlobalConfig.Skin == "dark")
            {
                textEditorEx.Background = (Brush)Application.Current.Resources["DarkBackGround"];
                textEditorEx.Foreground = (Brush)Application.Current.Resources["LightBackGround"];
            }
            else
            {
                textEditorEx.Foreground = (Brush)Application.Current.Resources["DarkBackGround"];
                textEditorEx.Background = (Brush)Application.Current.Resources["LightBackGround"];
            }
            textEditorEx.TextArea.TextEntered += CodeTip;
            textEditorEx.Options.CutCopyWholeLine = true;
            textEditorEx.Options.InheritWordWrapIndentation = true;
            textEditorEx.TextArea.TextView.LinkTextForegroundBrush = (Brush)Application.Current.Resources["LinkForeGround"];
            var cm = new ContextMenu();
            foreach (MenuItem item in MainWindow.Instance.EditMenu.Items)
            {
                cm.Items.Add(new MenuItem()
                {
                    Style = item.Style,
                    InputGestureText = item.InputGestureText,
                    Header = item.Header,
                    Command = item.Command,
                    FontFamily = item.FontFamily,
                    FontSize = item.FontSize,
                    Icon = item.Icon,
                });
            }
            textEditorEx.ContextMenu = cm;
            var tabItem = new TabItem
            {
                ToolTip = filename,
                Header = dock,
                Uid = filename,
                Content = textEditorEx
            };
            if (Path.GetExtension(filename) != "dll" || !(fileSize <= GlobalConfig.MaximumFileSize))
            {
                var resourceName = GlobalConfig.XshdFilePath + $"{GlobalConfig.Editor.Theme}\\{GetLanguage(filename)}.xshd";
                using (Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (System.Xml.XmlTextReader reader = new(s))
                    {

                        var xshd = HighlightingLoader.LoadXshd(reader);
                        textEditorEx.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                    }
                }
            }
            if (load & (!loadPath.IsNullOrEmpty()))
            {
                textEditorEx.Load(loadPath);
            }
            tabItem.Tag = textEditorEx;
            var _searchPanel = new TextEditorSearchPanel
            {
                DataContext = new SearchPanelViewModel
                {
                    Editor = textEditorEx
                }
            };
            _searchPanel.Show();
            textEditorEx.CustomCmd = () => MainWindow.Instance.WikiDrawer.IsOpen = true;
            return tabItem;
        }
    }

    private string _currentInput = "";
    private string _lastCodeHash;
    private Dictionary<string, List<string>> _cachedVariables;
    private bool _inhert = false, _brackets_auto_closed = false, _qmark_auto_closed = false;
    private CompletionWindow _completionWindow;

    private void CodeTip(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        if (_completionWindow is null || (_completionWindow.Visibility != Visibility.Visible))
        {
            _completionWindow = new(GetCurrentTextEditor().TextArea)
            {
                Background = MainWindow.Instance.MainMenu.Background,
                Foreground = MainWindow.Instance.MainMenu.Foreground,
                CloseWhenCaretAtBeginning = true
            };
        }
        try
        {
            _completionWindow.Show();
        }
        catch (InvalidOperationException)
        {
            _completionWindow = new(GetCurrentTextEditor().TextArea)
            {
                Background = MainWindow.Instance.MainMenu.Background,
                Foreground = MainWindow.Instance.MainMenu.Foreground,
                CloseWhenCaretAtBeginning = true
            };
        }
        finally
        {
            _completionWindow.Hide();
        }
        if (e.Text == ":")
        {
            _inhert = true;
        }
        else if (new List<string> { "(", "[", "{", "\"", "'" }.Contains(e.Text))
        {
            if (new List<string> { "\"", "'" }.Contains(e.Text))
            {
                SymbolsAutoCompletion(e.Text, true);
            }
            else
            {
                SymbolsAutoCompletion(e.Text);
            }
        }
        if (e.Text == "\n" || e.Text == " " || e.Text.IsNumber() || !e.Text.IsAllChar())
        {
            if (_inhert & e.Text == "\n")
            {
                InsertTextAtCursor(GetCurrentTextEditor(), "\t");
                _inhert = false;
            }
            else if (e.Text == "\n")
            {
                var t = GetCurrentTextEditor();
                var caretOffset = t.CaretOffset;
                // 获取光标前的字符
                if (caretOffset > 0 && t.Document.GetText(caretOffset - 1, 1) == ":")
                {
                    // 插入换行符
                    t.Document.Insert(caretOffset, "\n");

                    // 获取当前行的缩进
                    var lineNumber = t.Document.GetLineByOffset(caretOffset).LineNumber;
                    var indent = GetLineIndentation(t, lineNumber);

                    // 插入缩进
                    t.Document.Insert(t.CaretOffset, indent);

                    // 更新光标位置
                    t.CaretOffset += indent.Length;

                    // 处理事件已处理
                    e.Handled = true;
                }
            }
            else if ((e.Text == ")" || e.Text == "]" || e.Text == "}" || (e.Text == "'" & _qmark_auto_closed) || (e.Text == "\"" & _qmark_auto_closed)) & _brackets_auto_closed)
            {
                var t = GetCurrentTextEditor();
                var co = t.CaretOffset;
                t.Text = t.Text.Remove(t.CaretOffset, 1);
                t.CaretOffset = co;
                _brackets_auto_closed = false;
                _qmark_auto_closed = false;
            }
            EndCompletion();
            return;
        }

        var completionDatas = _completionWindow.CompletionList.CompletionData;
        completionDatas.Clear();

        // 保存当前输入
        _currentInput += e.Text;
        if (Language.Python.Keywords.Contains(_currentInput) || Language.Python.MagicMethods.Contains(_currentInput) || Language.Python.BuiltIns.Contains(_currentInput))
        {
            EndCompletion();
            return;
        }

        App.LOGGER.LogDebug("Start Code Sense");

        App.LOGGER.LogDebug("Start Code Sense - Keyword");
        var _keywords = AddCompletions(Language.Python.Keywords);

        App.LOGGER.LogDebug("Start Code Sense - Magic");
        var _magics = AddCompletions(Language.Python.MagicMethods);

        App.LOGGER.LogDebug("Start Code Sense - Bulitin");
        var _builtins = AddCompletions(Language.Python.BuiltIns);

        App.LOGGER.LogDebug("Start Code Sense - Template");
        var _templates = AddCompletions(GlobalConfig.CodeTemplates.Keys);

        var currentCode = GetCurrentTextEditor().Text.RemoveRight(_currentInput.Length);
        var currentHash = currentCode.ComputeHash();

        //App.LOGGER.LogDebug("Start Code Sense - Variable");
        //if (currentHash == _lastCodeHash)
        //{
        //    // 使用缓存的结果
        //    var allVars = _cachedVariables;
        //}
        //else
        //{
        //    // 在后台线程中分析代码
        //    var allVars = PythonCodeAnalyser.GetVariables(currentCode, true);
        //    _cachedVariables = allVars;
        //    _lastCodeHash = currentHash;
        //}
        //var _variables = PythonCodeAnalyser.ExtractVariables(_cachedVariables)
        //    .Where((s) => { return s.StartsWith(_currentInput); }).ToList();
        //_variables.AddRange(PythonCodeAnalyser.ExtractVariables(_cachedVariables)
        //    .Where((s) => { return s.Contains(_currentInput); }));
        //_variables.RemoveDuplicates();
        IEnumerable<string> _variables = [];
        App.LOGGER.LogDebug("Start Code Sense - Add");
        // 检查当前输入是否为关键字、变量或方法,并添加到对应的集合中
        foreach (var item in _keywords)
        {
            completionDatas.Add(new CompletionData(item, CompletionDataType.Keyword));
        }
        foreach (var item in _magics)
        {
            completionDatas.Add(new CompletionData(item, CompletionDataType.Magic));
        }
        foreach (var item in _builtins)
        {
            completionDatas.Add(new CompletionData(item, CompletionDataType.Builtin));
        }
        foreach (var item in _variables)
        {
            completionDatas.Add(new CompletionData(item, CompletionDataType.Variable));
        }
        foreach (var item in _templates)
        {
            completionDatas.Add(new CompletionData(item, CompletionDataType.CodeTemplate, GlobalConfig.CodeTemplates[item]));
        }

        if (completionDatas.Count == 0)
        {
            EndCompletion();
            return;
        }

        completionDatas = completionDatas.RemoveDuplicates();
        App.LOGGER.LogDebug("End Code Sense");
        _completionWindow.Show();

        _completionWindow.Closed += (o, args) =>
        {
            _completionWindow = null;
            // 当用户输入完整的关键字、变量或方法,或使用了代码补全时,重置保存的输入
            _currentInput = "";
        };
    }

    public List<string> AddCompletions(IEnumerable<string> data)
    {
        var _data = data.Where((s) => { return s.StartsWith(_currentInput); }).ToList();
        _data.AddRange(data.Where((s) => { return s.Contains(_currentInput); }));
        _data = [.. _data.RemoveDuplicates()];
        return _data;
    }

    public void EndCompletion()
    {
        _currentInput = "";
        _completionWindow?.Close();
    }

    public void SymbolsAutoCompletion(string symbol, bool isQMark = false)
    {
        List<string> SupportedSymbols = ["(", "[", "{", "\"", "'"];
        List<string> SupportedCompletionSymbols = [")", "]", "}", "\"", "'"];
        if (SupportedSymbols.Contains(symbol))
        {
            var t = GetCurrentTextEditor();
            InsertTextAtCursor(t, SupportedCompletionSymbols[SupportedSymbols.IndexOf(symbol)]);
            t.CaretOffset--;
            _brackets_auto_closed = true;
            _qmark_auto_closed = isQMark;
        }
        else
        {
            App.LOGGER.Error(new NotSupportedException("符号[{0}]不支持自动补全。".FormatEx(symbol)));
        }
    }

    public string GetLineIndentation(TextEditor t, int lineNumber)
    {
        // 获取前一行的缩进
        if (lineNumber > 1)
        {
            var previousLine = t.Document.GetLineByNumber(lineNumber - 1);
            var previousLineText = t.Document.GetText(previousLine.Offset, previousLine.Length);

            // 假设使用空格作为缩进
            var indentLength = 0;
            foreach (var c in previousLineText)
            {
                if (c == ' ')
                {
                    indentLength++;
                }
                else
                {
                    break;
                }
            }
            return new string(' ', indentLength);
        }
        return "";
    }

    private void InsertTextAtCursor(TextEditor textEditor, string textToInsert)
    {
        // 获取光标位置
        var caretOffset = textEditor.CaretOffset;

        // 在光标处插入文本
        textEditor.Document.Insert(caretOffset, textToInsert);

        // 更新光标位置，使其位于插入的文本后面
        textEditor.CaretOffset = caretOffset + textToInsert.Length;
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
            MainWindow.autoSaveTimer?.Start();
            MainWindow.autoBackupTimer?.Start();
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
        else
        {
            SaveAsFile();
        }
    }

    internal void SaveFile(object sender, EventArgs e)
    {
        if (MainWindow.Instance.MainTabCtrl.SelectedItem is null)
        {
            return;
        }
        if (sender == MainWindow.autoSaveTimer)
        {
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
        else if (sender == MainWindow.autoBackupTimer)
        {
            if (!((TabItem)MainWindow.Instance.MainTabCtrl.SelectedItem).ToolTip.ToString().IsNullOrEmpty())
            {
                try
                {
                    var file = ((TabItem)MainWindow.Instance.MainTabCtrl.SelectedItem).ToolTip.ToString();
                    GetCurrentTextEditor().Save(GlobalConfig.AutoBackupPath + Path.GetFileNameWithoutExtension(file) + ".bak" + Path.GetExtension(file).Replace("\"", ""));
                }
                catch (Exception ex)
                {
                    App.LOGGER.Error(ex);
                }
            }
        }
    }

    internal void SaveAsFile()
    {
        var sfd = new System.Windows.Forms.SaveFileDialog();
        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            MainWindow.Instance.FileSavingPanel.Visibility = Visibility.Visible;
            try
            {
                GetCurrentTextEditor().Save(sfd.FileName);
                MainWindow.Instance.FileSavingTip.Text = Application.Current.Resources["Main.Bottom.FileSavingTip.Success"].ToString();
            }
            catch (Exception ex)
            {
                App.LOGGER.Error(ex);
                MainWindow.Instance.FileSavingTip.Text = Application.Current.Resources["Main.Bottom.FileSavingTip.Fail"].ToString();
            }
        }
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
