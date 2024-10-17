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
using System.Collections.Specialized;
using RYCBEditorX.MySQL;
using System.Collections.Generic;

namespace RYCBEditorX;
#pragma warning disable IDE0059 // 不需要赋值
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{

    private Splash splash;

    public const string VERSION = "1.0.0-rc1";
    public const string MAJOR_VERSION = "1";
    public const string MINOR_VERSION = "0";
    public const string MICRO_VERSION = "0";
    public const string REVISION_NUMBER = "rc1";
    public static DateTime BUILD_TIME;
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
        GlobalWindows.ActivatingWindows = [];
        GlobalConfig.LocalizationString = CultureInfo.CurrentCulture.Name;
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

    private void RefreshItems()
    {
        GlobalConfig.CurrentProfiles = [];
        foreach (var item in Directory.EnumerateFiles(STARTUP_PATH + "\\Profiles\\Runners"))
        {
            var icbfp = new ICBFileProcessor(item);
            RunProfile rp = new()
            {
                Name = icbfp.GetInfo(ICBFileProcessor.InfoType.name),
                ScriptPath = icbfp.GetInfo(ICBFileProcessor.InfoType.script),
                InterpreterArgs = icbfp.GetInfo(ICBFileProcessor.InfoType.itpr_args),
                Interpreter = icbfp.GetInfo(ICBFileProcessor.InfoType.itpr),
                ScriptArgs = icbfp.GetInfo(ICBFileProcessor.InfoType.script_args),
                UseBPSR = bool.Parse(icbfp.GetInfo(ICBFileProcessor.InfoType.use_bpsr)),
            };
            icbfp = null;
            GlobalConfig.CurrentProfiles.Add(rp);
        }
    }

    private void LoadConfig()
    {
        // 载入配置项
        LOGGER.Log("载入配置项...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        AppSettings = AppConfiguration.AppSettings;

        // 初始化配置
        BUILD_TIME = DateTime.Now;
        GlobalConfig.Version = $"{MAJOR_VERSION}.{MINOR_VERSION}.{MICRO_VERSION}";
        GlobalConfig.Revision = REVISION_NUMBER;
        GlobalConfig.StartupPath = STARTUP_PATH;
        GlobalConfig.CurrentLogger = LOGGER;
        GlobalConfig.Resources = Resources;

        // 读取并设置配置项
        var settings = AppSettings.Settings;
        GlobalConfig.XshdFilePath = settings["XshdFilePath"].Value;
        GlobalConfig.LocalizationService = LocalizationService;
        GlobalConfig.PythonPath = settings["PythonPath"].Value;
        GlobalConfig.MaximumFileSize = int.Parse(settings["MaximumFileSize"].Value);
        GlobalConfig.LocalizationString = settings["Language"].Value;
        GlobalConfig.AutoBackupInterval = int.Parse(settings["AutoBackupInterval"].Value);
        GlobalConfig.ShouldAutoBackup = bool.Parse(settings["AutoBackup"].Value);
        GlobalConfig.AutoSaveInterval = int.Parse(settings["AutoSaveInterval"].Value);
        GlobalConfig.ShouldAutoSave = bool.Parse(settings["AutoSave"].Value);
        GlobalConfig.AutoBackupPath = settings["AutoBackupPath"].Value;
        GlobalConfig.Downloading.ParallelDownload = bool.Parse(settings["Downloading.ParallelDownload"].Value);
        GlobalConfig.Downloading.ParallelCount = int.Parse(settings["Downloading.ParallelCount"].Value);
        Resources["Main"] = new FontFamily(settings["Font"].Value);

        RefreshItems();

        // 如果 XshdFilePath 为空，设置默认值并保存
        if (GlobalConfig.XshdFilePath.IsNullOrEmpty())
        {
            settings["XshdFilePath"].Value = Path.Combine(STARTUP_PATH, "Highlightings");
            AppConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            GlobalConfig.XshdFilePath = settings["XshdFilePath"].Value;
        }

        // 载入程序主题与颜色
        LoadSkin(settings["Skin"].Value);

        // 载入编辑器配置项
        LoadEditorSettings(settings);

        // 载入代码模板
        LoadCodeTemplates(Path.Combine(STARTUP_PATH, "Profiles", "Templates", "python", "builtin.json"));

        // 注册跨域访问
        RegisterCrossings();
    }

    // 方法定义
    private void LoadSkin(string skin)
    {
        LOGGER.Log("载入程序主题与颜色", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Skin = skin;
        UpdateResourceDictionary(GlobalConfig.GetSkin(skin), 0);
    }

    private void LoadEditorSettings(KeyValueConfigurationCollection settings)
    {
        LOGGER.Log("载入编辑器配置项", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Editor.Theme = settings["Editor.Theme"].Value;
        GlobalConfig.Editor.ShowLineNumber = bool.Parse(settings["Editor.ShowLineNum"].Value);
        GlobalConfig.Editor.FontFamilyName = settings["Editor.FontName"].Value;
        GlobalConfig.Editor.FontSize = int.Parse(settings["Editor.FontSize"].Value);
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
        base.OnInitialized();
    }

    protected override Window CreateShell()
    {
        splash.Dispatcher.Invoke(
            () => splash.LoadingTip.Text = GetLoadingTip(GlobalConfig.LocalizationString, 6));
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
        LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["ErrorColor"];
        LightTip.ViewModelInstance.Icon = Icons.ERROR;
        LightTip.ViewModelInstance.Content = $"{e.Exception.GetType()}\n{e.Exception.Message}";
        Views.MainWindow.Notifications.Add(new(Icons.ERROR, (Brush)LightTip.Instance.Resources["ErrorColor"], LightTip.ViewModelInstance.Content));
        Views.MainWindow.Instance.NotificationsList.Items.Refresh();
        LightTip.Instance.Show();
        Views.MainWindow.Instance.NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
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
