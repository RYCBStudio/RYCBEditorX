using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using RYCBEditorX.Crossings;
using RYCBEditorX.Dialogs.Views;
using RYCBEditorX.MySQL;
using RYCBEditorX.Utils;

namespace RYCBEditorX.Views;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    internal static bool IsCodeRunning = false;
    public static DispatcherTimer EWResizer = new(), autoSaveTimer, autoBackupTimer;
    public static MainWindow Instance
    {
        get; set;
    }

    public static List<NotificationTemplate> Notifications
    {
        get; internal set;
    } = [];

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        NotificationsList.ItemsSource = Notifications;
        if (GlobalConfig.Skin == "dark")
        {
            MainGrid.Background = (Brush)Application.Current.Resources["DarkBackGround"];
        }
        else
        {
            MainGrid.Background = (Brush)Application.Current.Resources["LightBackGround"];
        }
        EWResizer.Tick += ResizeEmbeddedWindow;  //绑定事件
        EWResizer.Interval = TimeSpan.FromSeconds(0.1);
        //MainTabCtrl.ItemsSource = texts;
        //FluentMessageBox.Theme = "Error";
        //new FluentMessageBox()
        //{
        //    DataContext = new FluentMessageBoxViewModel()
        //    {
        //        Title = "Test",
        //        Message = App.AppSettings.Settings["Test"].OrderBy_Value,
        //    },
        //}.ShowDialog();
#pragma warning disable CA1806 // 不要忽略方法结果
        new LightTip(this);
#pragma warning restore CA1806 // 不要忽略方法结果
        foreach (var item in GlobalConfig.CurrentProfiles)
        {
            RunProfilesComboBox.Items.Add(item.Name);
        }
    }

    private void ResizeEmbeddedWindow(object sender, EventArgs e)
    {
        if (SetWindow.intPtr != IntPtr.Zero)
        {
            var t = new Thread(SetWindow.ResizeWindow);
            t.Start();  //开线程刷新第三方窗体大小
            Thread.Sleep(50); //略加延时
            EWResizer.Stop();  //停止定时器
        }
    }

    private void BtnSelectHLProfile_Click(object sender, RoutedEventArgs e)
    {
        BottomTabCtrl.SelectedIndex = 2;
    }

    private void ThrowEx_Click(object sender, RoutedEventArgs e)
    {
        throw new System.Exception(string.Format("sender [{0}] has thrown an exception. Yahoo~", sender));
    }

    private void MInfotest_Click(object sender, RoutedEventArgs e)
    {
        LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["InfoColor"];
        LightTip.ViewModelInstance.Icon = Icons.INFO;
        LightTip.ViewModelInstance.Content = "Test";
        Notifications.Add(new(Icons.INFO, (Brush)LightTip.Instance.Resources["InfoColor"], LightTip.ViewModelInstance.Content));
        NotificationsList.Items.Refresh();
        GlobalWindows.ActivatingWindows.Add(LightTip.Instance);
        LightTip.Instance.Show();
        NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
    }

    private void MWarntest_Click(object sender, RoutedEventArgs e)
    {
        LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["WarnColor"];
        LightTip.ViewModelInstance.Icon = Icons.WARN;
        LightTip.ViewModelInstance.Content = "Test";
        Notifications.Add(new(Icons.WARN, (Brush)LightTip.Instance.Resources["WarnColor"], LightTip.ViewModelInstance.Content));
        NotificationsList.Items.Refresh();
        GlobalWindows.ActivatingWindows.Add(LightTip.Instance);
        LightTip.Instance.Show();
        NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
    }

    private void MErrtest_Click(object sender, RoutedEventArgs e)
    {
        LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["ErrorColor"];
        LightTip.ViewModelInstance.Icon = Icons.ERROR;
        LightTip.ViewModelInstance.Content = "Test";
        Notifications.Add(new(Icons.ERROR, (Brush)LightTip.Instance.Resources["ErrorColor"], LightTip.ViewModelInstance.Content));
        NotificationsList.Items.Refresh();
        GlobalWindows.ActivatingWindows.Add(LightTip.Instance);
        LightTip.Instance.Show();
        NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        LightTip.Instance.Close();
    }

    private void BtnClearSelected_Click(object sender, RoutedEventArgs e)
    {
        if (NotificationsList.SelectedIndex < 0) { return; }
        Notifications.RemoveAt(NotificationsList.SelectedIndex);
        NotificationsList.Items.Refresh();
    }

    private void BtnClearAll_Click(object sender, RoutedEventArgs e)
    {
        Notifications.Clear();
        NotificationsList.Items.Refresh();
    }

    private void NotificationsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        BtnClearSelected.Visibility = Visibility.Visible;
    }

    private void NotificationDockPanel_LostFocus(object sender, RoutedEventArgs e)
    {
        BtnClearSelected.Visibility = Visibility.Collapsed;
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void NotificationToggleButton_Click(object sender, RoutedEventArgs e)
    {
        NotificationBadge.BadgeMargin = new(0, 1, -100, 0);
    }

    private void FileSavingTip_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (FileSavingTip.Text == Application.Current.Resources["Main.Bottom.FileSavingTip.Success"].ToString())
        {
            FileSavingIcon.Foreground = (Brush)Application.Current.Resources["SuccessBrush"];
            FileSavingIcon.Text = "\xe860";
        }
        else if (FileSavingTip.Text == Application.Current.Resources["Main.Bottom.FileSavingTip.Waiting"].ToString())
        {
            FileSavingIcon.Foreground = (Brush)Application.Current.Resources["WarningBrush"];
            FileSavingIcon.Text = "\xe63e";
        }
        else
        {
            FileSavingIcon.Foreground = (Brush)Application.Current.Resources["DangerBrush"];
            FileSavingIcon.Text = "\xe685";
        }
    }

    private void RunProfilesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        GlobalConfig.CurrentRunProfile = GlobalConfig.CurrentProfiles[RunProfilesComboBox.SelectedIndex - 1];
    }

    private void Login(object sender, RoutedEventArgs e)
    {
        var oldUserName = User.Content;
        var lw = new LoginWindow();
        if (lw.ShowDialog() == true)
        {
            User.Content = lw.UsrName;
            if (lw.UsrName.IsNullOrEmpty())
            {
                User.Content = oldUserName;
            }
        }
        else
        {
            User.Content = Application.Current.Resources["Main.User.Unlogin"];
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        EWResizer.Start();
    }

    private void test_Load(object sender, RoutedEventArgs e)
    {
        new ProgressedInfoTip().Show();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Focus();
        if (UpdateInfoCrossing.HasSV)
        {
            Extensions.ShowTip.Invoke(Application.Current.Resources["Update.HasSV"].ToString().Format(GlobalConfig.Version), Icons.ERROR);
        }
        else if (UpdateInfoCrossing.EOL)
        {
            Extensions.ShowTip.Invoke(Application.Current.Resources["Update.EOL"].ToString().Format(GlobalConfig.Version), Icons.WARN);
        }
        else if (UpdateInfoCrossing.ComingToEOL)
        {
            Extensions.ShowTip.Invoke(Application.Current.Resources["Update.AboutToStopSupport"].ToString().Format(GlobalConfig.Version), Icons.INFO);
        }
        else if (UpdateInfoCrossing.HasNew)
        {
            Extensions.ShowTip.Invoke(Application.Current.Resources["Update.HasNew"].ToString().Format(UpdateInfoCrossing.NewVersion), Icons.INFO);
        }
    }
}

public class TabItemStyle(string text, object content)
{
    public string Title
    {
        get; set;
    } = text;
    public object Content
    {
        get; set;
    } = content;
}

public class NotificationTemplate(string icon, Brush iconbrush, string content)
{
    public string Icon
    {
        get; set;
    } = icon;

    public Brush IconBrush
    {
        get; set;
    } = iconbrush;

    public string Content
    {
        get; set;
    } = content;
}
