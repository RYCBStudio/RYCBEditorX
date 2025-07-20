#pragma warning disable IDE0059 // 不需要赋值
using System;
using System.Configuration;
using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using RYCBEditorX.Views;
using RYCBEditorX.Utils;
using RYCBEditorX.Dialogs.Views;
using System.Windows.Media;
using Microsoft.TeamFoundation.Common;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;
using Microsoft.VisualStudio.Services.Common;
using RYCBEditorX.Crossing;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace RYCBEditorX;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{

    private Splash splash;

    public const string VERSION = "1.0.0-rc3";
    public const string MAJOR_VERSION = "1";
    public const string MINOR_VERSION = "0";
    public const string MICRO_VERSION = "0";
    public const string REVISION_NUMBER = "rc3";
    public static bool AppInitialized = false;
    public static DateTime BUILD_TIME;
    public static DispatcherTimer StartupTimer;
    public static LocalizationService LocalizationService
    {
        get; set;
    }
    public static string STARTUP_PATH { get; private set; } = System.Windows.Forms.Application.StartupPath;

    public static LogUtil LOGGER
    {
        get; private set;
    }

    internal static Configuration AppConfiguration
    {
        get; private set;
    }

    internal static AppSettingsSection AppSettings
    {
        get; private set;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var mutex = new Mutex(true, "RYCBEditorX", out var createdNew);
        if (!createdNew) {
            var current = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    Win32Helper.SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
            Shutdown();
            return;
        }
        GlobalWindows.ActivatingWindows = [];
        GlobalConfig.LocalizationString = CultureInfo.CurrentCulture.Name;
        StartupTimer = new DispatcherTimer() { Interval = new TimeSpan(hours: 0, minutes: 1, seconds: 0) };
        StartupTimer.Tick += (sender, args) =>
        {
            StartupTimer.Stop();
            if (!AppInitialized)
            {
                if (MessageBox.Show("启动时间过长。请检查网络设置或查看/向他人发送日志。\n点击“确定”退出程序。", "错误", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    App.Current.Shutdown();
                }
            }
        };
        StartupTimer.Start();
        var t = new Thread(() =>
        {
            splash = new();
            splash.ShowDialog();//不能用Show
            splash.Dispatcher.Invoke(
                () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 0));
        });
        t.SetApartmentState(ApartmentState.STA);//设置单线程
        t.Start();
        base.OnStartup(e);
    }

    protected override void Initialize()
    {
        LOGGER = new(STARTUP_PATH + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
        LOGGER.Log("初始化...");
        GlobalConfig.NetworkAvaliable = Utils.Update.CloudSourceConnectionTester.TestConnection();
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 1));
        LoadConfig();
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 2));
        LoadExceptionCaptures();
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 3));
        GlobalWindows.ActivatingWindows.Add(MainWindow);
        LoadLocalization();
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 4));
        base.Initialize();
    }

    private void LoadLocalization()
    {
        LOGGER.Log("加载本地化资源...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:本地化");
        LocalizationService = new LocalizationService(STARTUP_PATH + $"\\Languages\\{GlobalConfig.LocalizationString}.json");
        // 将本地化字符串存入资源字典
        UpdateResources();
    }

    private void UpdateResources()
    {
        foreach (var lKey in LocalizationService.LocalizationDictionary.Keys)
        {
            Resources[lKey] = LocalizationService.GetLocalizedString(lKey);
        }
    }
    private void LoadConfig()
    {
        // 载入配置项
        LOGGER.Log("载入配置项...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");

        // 配置文件路径
        var configPath = Path.Combine(STARTUP_PATH, "Profiles", "Config", "Settings.json");

        // 如果配置文件不存在，创建默认配置
        if (!File.Exists(configPath))
        {
            CreateDefaultConfig(configPath);
        }

        // 读取配置文件
        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<AppConfig>(json);

        // 初始化配置
        BUILD_TIME = DateTime.Now;
        GlobalConfig.Version = $"{MAJOR_VERSION}.{MINOR_VERSION}.{MICRO_VERSION}";
        GlobalConfig.Revision = REVISION_NUMBER;
        GlobalConfig.StartupPath = STARTUP_PATH;
        GlobalConfig.CurrentLogger = LOGGER;
        GlobalConfig.Resources = Resources;

        // 设置配置项
        GlobalConfig.XshdFilePath = Path.Combine(STARTUP_PATH, config.XshdFilePath ?? "Highlightings\\");
        GlobalConfig.LocalizationService = LocalizationService;
        GlobalConfig.PythonPath = config.PythonPath;
        GlobalConfig.MaximumFileSize = config.MaximumFileSize;
        GlobalConfig.LocalizationString = config.Language;
        GlobalConfig.AutoSaveInterval = config.AutoSave?.Interval ?? 5;
        GlobalConfig.ShouldAutoSave = config.AutoSave?.Enabled ?? true;
        GlobalConfig.AutoBackupInterval = config.AutoBackup?.Interval ?? 1;
        GlobalConfig.ShouldAutoBackup = config.AutoBackup?.Enabled ?? true;
        GlobalConfig.AutoBackupPath = Path.Combine(STARTUP_PATH, config.AutoBackup?.Path ?? "Backup\\");
        GlobalConfig.Downloading.ParallelDownload = config.Downloading?.ParallelDownload ?? true;
        GlobalConfig.Downloading.ParallelCount = config.Downloading?.ParallelCount ?? 8;
        Resources["Main"] = new FontFamily(config.Font);

        // 如果目录不存在则创建
        if (!Directory.Exists(GlobalConfig.XshdFilePath))
        {
            Directory.CreateDirectory(GlobalConfig.XshdFilePath);
        }

        if (!Directory.Exists(GlobalConfig.AutoBackupPath))
        {
            Directory.CreateDirectory(GlobalConfig.AutoBackupPath);
        }

        // 载入程序主题与颜色
        LoadSkin(config.Skin);

        // 载入编辑器配置项
        LoadEditorSettings(config.Editor);

        // 载入代码模板
        LoadCodeTemplates(Path.Combine(STARTUP_PATH, "Profiles", "Templates", "python", "builtin.json"));

        // 注册跨域访问
        RegisterCrossings();
    }

    private void CreateDefaultConfig(string configPath)
    {
        var defaultConfig = new AppConfig
        {
            Skin = "dark",
            MaximumFileSize = 307200,
            Language = "zh-CN",
            PythonPath = "python",
            AutoSave = new AutoSaveConfig { Enabled = true, Interval = 5 },
            AutoBackup = new AutoBackupConfig { Enabled = true, Interval = 1, Path = "Backup\\" },
            Font = "Microsoft YaHei UI",
            XshdFilePath = "Highlightings\\",
            Editor = new EditorConfig
            {
                Theme = "IDEA",
                ShowLineNumber = true,
                FontName = "Consolas",
                FontSize = 12
            },
            Downloading = new DownloadingConfig
            {
                ParallelDownload = true,
                ParallelCount = 8
            }
        };

        var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        File.WriteAllText(configPath, json);
    }

    // 方法定义
    private void LoadSkin(string skin)
    {
        LOGGER.Log("载入程序主题与颜色", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Skin = skin;
        UpdateResourceDictionary(GlobalConfig.GetSkin(skin), 0);
    }

    private void LoadEditorSettings(EditorConfig editorConfig)
    {
        LOGGER.Log("载入编辑器配置项", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Editor.Theme = editorConfig.Theme;
        GlobalConfig.Editor.ShowLineNumber = editorConfig.ShowLineNumber;
        GlobalConfig.Editor.FontFamilyName = editorConfig.FontName;
        GlobalConfig.Editor.FontSize = editorConfig.FontSize;
    }

    private void LoadCodeTemplates(string path)
    {
        LOGGER.Log("载入代码模板", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        var codeTemplates = Custom.TemplateAnalyser.GetCodeImplements(path);
        GlobalConfig.CodeTemplates.AddRange(codeTemplates);
    }

    private void UpdateResourceDictionary(string resourceStr, int pos)
    {
        if (pos < 0 || pos > 2)
        {
            return;
        }
        var resource = new ResourceDictionary
        {
            Source = new Uri(resourceStr)
        };
        Resources.MergedDictionaries.RemoveAt(pos);
        Resources.MergedDictionaries.Insert(pos, resource);
    }
    #region Prism Application

    internal void RegisterCrossings()
    {
        LOGGER.Log("注册跨窗口通信", module: EnumLogModule.CUSTOM, customModuleName: "初始化:通信");
        new DialogCrossing().Register();
        new UpdateCrossing().Register();
        new WikiLoader().Register();
    }

    protected override void OnInitialized()
    {
        splash.Dispatcher.Invoke(splash.Close);
        AppInitialized = true;
        base.OnInitialized();
    }

    protected override Window CreateShell()
    {
        GlobalConfig.NetworkAvaliable = Utils.Update.CloudSourceConnectionTester.TestConnection();
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 6));
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 7));
        GlobalWindows.CurrentMainWindow = Container.Resolve<MainWindow>();
        return GlobalWindows.CurrentMainWindow;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    protected override void OnExit(ExitEventArgs e)
    {
        foreach (var item in GlobalWindows.ActivatingWindows)
        {
            item?.Close();
        }
        Process.Start(new ProcessStartInfo()
        {
            Arguments = "taskkill /im RYCBEditorX.exe /f /t",
            FileName = "Cmd",
            UseShellExecute = true,
            WorkingDirectory = "C:\\Windows\\System32\\",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        });
        base.OnExit(e);
    }
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 5));
        LOGGER.Log("加载 MySQL 模块...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:模块");
        moduleCatalog.AddModule<MySQL.MySQLModule>();
        LOGGER.Log("加载 Dialog 模块...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:模块");
        moduleCatalog.AddModule<Dialogs.DialogsModule>();
        LOGGER.Log("加载 Utils 模块...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:模块");
        moduleCatalog.AddModule<UtilsModule>();
        base.ConfigureModuleCatalog(moduleCatalog);
    }
    #endregion
    #region 异常处理
    private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        if (LightTip.ViewModelInstance is not null)
        {
            LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["ErrorColor"];
            LightTip.ViewModelInstance.Icon = Icons.ERROR;
            LightTip.ViewModelInstance.Content = $"{e.Exception.GetType()}\n{e.Exception.Message}";
            Views.MainWindow.Notifications.Add(new(Icons.ERROR, (Brush)LightTip.Instance.Resources["ErrorColor"], LightTip.ViewModelInstance.Content));
            Views.MainWindow.Instance.NotificationsList.Items.Refresh();
            LightTip.Instance.Show();
            Views.MainWindow.Instance.NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
        }
        LOGGER.Error(e.Exception);
        LOGGER.Log("主线程发生异常");
        LOGGER.Log("IDE正在尝试自动解决崩溃...", module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
        try
        {
            LOGGER.Log("处理成功。", module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
            var _ex = e.Exception;
            while (_ex.InnerException is not null)
            {
                LOGGER.Error(_ex.InnerException);
                _ex = _ex.InnerException;
            }
        }
        catch (Exception ex)
        {
            LOGGER.Log("处理中发生异常！", EnumLogType.FATAL, module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
            LOGGER.Error(ex);
            LOGGER.Log("处理失败。IDE即将崩溃。", EnumLogType.FATAL, module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LOGGER.Error(e.ExceptionObject as Exception);
        LOGGER.Log("某处线程发生异常");
        LOGGER.Log("IDE正在尝试自动解决崩溃...", module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
        if (e.IsTerminating)
        {
            LOGGER.Log("处理失败。IDE即将崩溃。", EnumLogType.FATAL, module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
        }
        else
        {
            LOGGER.Log("处理成功。", module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
        }
    }
    private void LoadExceptionCaptures()
    {
        System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException; ;
    }
    #endregion

}
