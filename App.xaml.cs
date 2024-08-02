﻿using System;
using System.Configuration;
using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using RYCBEditorX.Views;
using RYCBEditorX.Utils;

namespace RYCBEditorX;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    internal static readonly string STARTUP_PATH = System.Windows.Forms.Application.StartupPath;

    internal static Utils.LogUtil LOGGER
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
        LOGGER = new(STARTUP_PATH + "\\Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
        LOGGER.Log("初始化...");
        System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        App.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException; ;
        LoadConfig();
        base.Initialize();
    }

    private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
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

    private static void LoadConfig()
    {
        LOGGER.Log("载入配置项...", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        AppSettings = AppConfiguration.AppSettings;

        LOGGER.Log("载入编辑器配置项", module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
        GlobalConfig.Editor.ShowLineNumber = bool.Parse(AppSettings.Settings["Editor.ShowLineNum"].Value);
        GlobalConfig.Editor.FontFamilyName = AppSettings.Settings["Editor.FontName"].Value;
        GlobalConfig.Editor.FontSize = Convert.ToInt32(AppSettings.Settings["Editor.FontSize"].Value);
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
        moduleCatalog.AddModule<Dialogs.DialogsModule>();
        moduleCatalog.AddModule<Utils.UtilsModule>();
        base.ConfigureModuleCatalog(moduleCatalog);
    }
}
