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
using RYCBEditorX.ViewModels;

namespace RYCBEditorX;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    private Splash Splash;

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

    protected override void Initialize()
    {
        SplashViewModel svm = new();
        Splash = new Splash();
        LOGGER = new(STARTUP_PATH + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
        LOGGER.Log("初始化...");
        this.Dispatcher.Invoke(Splash.Show);
        LoadExceptionCaptures();
        GlobalWindows.ActivatingWindows = [];
        GlobalWindows.ActivatingWindows.Add(Splash);
        GlobalWindows.ActivatingWindows.Add(MainWindow);
        LoadConfig();
        LoadLocalization();
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
        foreach (var item in Directory.EnumerateFiles(STARTUP_PATH+"\\Profiles\\Runners"))
        {
            var icbfp = new ICBFileProcessor(item);
            GlobalConfig.RunProfile rp = new()
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
        LOGGER.Log("载入配置项...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        AppSettings = AppConfiguration.AppSettings;

        BUILD_TIME = DateTime.Now;
        GlobalConfig.XshdFilePath = AppSettings.Settings["XshdFilePath"].Value;
        GlobalConfig.LocalizationService = LocalizationService;
        GlobalConfig.MaximumFileSize = int.Parse(AppSettings.Settings["MaximumFileSize"].Value);
        GlobalConfig.StartupPath = STARTUP_PATH;
        GlobalConfig.CurrentLogger = LOGGER;
        GlobalConfig.Resources = Resources;
        GlobalConfig.LocalizationString = AppSettings.Settings["Language"].Value;

        RefreshItems();

        if (GlobalConfig.XshdFilePath.IsNullOrEmpty())
        {
            AppSettings.Settings["XshdFilePath"].Value = STARTUP_PATH + "\\Highlightings";
            AppConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            GlobalConfig.XshdFilePath = AppSettings.Settings["XshdFilePath"].Value;
        }

        LOGGER.Log("载入程序主题与颜色", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        var skin = AppSettings.Settings["Skin"].Value;
        GlobalConfig.Skin = skin;
        UpdataResourceDictionary(GlobalConfig.GetSkin(skin), 0);

        LOGGER.Log("载入编辑器配置项", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Editor.Theme = AppSettings.Settings["Editor.Theme"].Value;
        GlobalConfig.Editor.ShowLineNumber = bool.Parse(AppSettings.Settings["Editor.ShowLineNum"].Value);
        GlobalConfig.Editor.FontFamilyName = AppSettings.Settings["Editor.FontName"].Value;
        GlobalConfig.Editor.FontSize = Convert.ToInt32(AppSettings.Settings["Editor.FontSize"].Value);
    }

    private void UpdataResourceDictionary(string resourceStr, int pos)
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
    protected override void OnInitialized()
    {
        Splash.Close();
        base.OnInitialized();
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    protected override void OnExit(ExitEventArgs e)
    {
        foreach (var item in GlobalWindows.ActivatingWindows)
        {
            item.Close();
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
        LightTip.ViewModelInstance.Icon = LightTip.ERROR;
        LightTip.ViewModelInstance.Content = $"{e.Exception.GetType()}\n{e.Exception.Message}";
        Views.MainWindow.Notifications.Add(new(LightTip.ERROR, (Brush)LightTip.Instance.Resources["ErrorColor"], LightTip.ViewModelInstance.Content));
        Views.MainWindow.Instance.NotificationsList.Items.Refresh();
        LightTip.Instance.Show();
        Views.MainWindow.Instance.NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
        LOGGER.Error(e.Exception);
        LOGGER.Log("主线程发生异常");
        LOGGER.Log("IDE正在尝试自动解决崩溃...", module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
        try
        {
            LOGGER.Log("处理成功。", module: EnumLogModule.CUSTOM, customModuleName: "异常处理");
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
