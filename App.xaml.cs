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
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Common;

namespace RYCBEditorX;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    private Splash Splash;

    internal static string STARTUP_PATH { get; private set; } = System.Windows.Forms.Application.StartupPath;

    internal static LogUtil LOGGER
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
        Splash = new Splash();
        this.Dispatcher.Invoke(Splash.Show);
        LOGGER = new(STARTUP_PATH + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
        LOGGER.Log("初始化...");
        System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException; ;
        LoadConfig();
        base.Initialize();
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

    private void LoadConfig()
    {
        LOGGER.Log("载入配置项...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        AppSettings = AppConfiguration.AppSettings;
        GlobalConfig.XshdFilePath = AppSettings.Settings["XshdFilePath"].Value;
        if (GlobalConfig.XshdFilePath.IsNullOrEmpty())
        {
            AppSettings.Settings["XshdFilePath"].Value = STARTUP_PATH+"\\Highlightings";
            AppConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            GlobalConfig.XshdFilePath = AppSettings.Settings["XshdFilePath"].Value;
        }

        LOGGER.Log("载入程序主题与颜色", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        var skin = AppSettings.Settings["Skin"].Value;
        GlobalConfig.Skin = skin;
        UpdataResourceDictionary(GlobalConfig.GetSkin(skin), 0);

        LOGGER.Log("载入编辑器配置项", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Editor.ShowLineNumber = bool.Parse(AppSettings.Settings["Editor.ShowLineNum"].Value);
        GlobalConfig.Editor.FontFamilyName = AppSettings.Settings["Editor.FontName"].Value;
        GlobalConfig.Editor.FontSize = Convert.ToInt32(AppSettings.Settings["Editor.FontSize"].Value);
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

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
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
    #endregion
}
