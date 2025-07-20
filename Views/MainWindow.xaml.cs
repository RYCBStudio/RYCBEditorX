using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using HandyControl.Tools.Extension;
using RYCBEditorX.Crossings;
using RYCBEditorX.Dialogs.Views;
using RYCBEditorX.MySQL;
using RYCBEditorX.Utils;
using RYCBEditorX.Utils.Crossings;

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
        if (!GlobalConfig.NetworkAvaliable)
        {
            NetworkWarningPanel.Visibility = Visibility.Visible;
        }
#pragma warning disable CA1806 // 不要忽略方法结果
        new LightTip(this);
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
        ShowNotification(NotificationType.Info, "Test-Info", "This is a test information.");
    }

    private void MWarntest_Click(object sender, RoutedEventArgs e)
    {
        ShowNotification(NotificationType.Warn, "Test-Warn", "This is a test warning.");
    }

    private void MErrtest_Click(object sender, RoutedEventArgs e)
    {
        ShowNotification(NotificationType.Error, "Test-Error", "This is a test error.");
    }
    
    public void ShowNotification(NotificationType type, string title, string message)
{
    // 确定图标和颜色
    Brush iconBrush;
    string icon;
    string resourceKey;

    switch (type)
    {
        case NotificationType.Warn:
            resourceKey = "WarnColor";
            icon = Icons.WARN;
            break;
        case NotificationType.Error:
            resourceKey = "ErrorColor";
            icon = Icons.ERROR;
            break;
        case NotificationType.Info:
        default:
            resourceKey = "InfoColor";
            icon = Icons.INFO;
            break;
    }

    iconBrush = (Brush)LightTip.Instance.Resources[resourceKey];
    
    // 设置通知内容
    var content = $"## {title}\n{message}";
    
    // 更新UI
    LightTip.ViewModelInstance.IconBrush = iconBrush;
    LightTip.ViewModelInstance.Icon = icon;
    LightTip.ViewModelInstance.Content = content;
    
    // 添加到通知列表
    Notifications.Add(new(icon, iconBrush, content));
    NotificationsList.Items.Refresh();
    
    // 显示通知
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

    private void NotificationsList_Unselected(object sender, RoutedEventArgs e)
    {
        BtnClearSelected.Visibility = Visibility.Collapsed;
    }

    private void test_Load(object sender, RoutedEventArgs e)
    {
        new ProgressedInfoTip().Show();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Focus();
        Task.Run(new Action(() =>
        {
            if (!GlobalConfig.NetworkAvaliable) { return; }
            while (!MySQLModule.ConnectionUtils.ConnectionOpened) ;
            Dispatcher.Invoke(() => UpdateProgress.Maximum = GlobalConfig.TotalLoadedOnline - 1);
            while (GlobalConfig.CurrentLoadedOnlineIndex <= GlobalConfig.TotalLoadedOnline - 1)
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateProgress.Value = GlobalConfig.CurrentLoadedOnlineIndex;
                    UpdateRTProgress.Text = string.Format("{0}/{1}", GlobalConfig.CurrentLoadedOnlineIndex + 1, GlobalConfig.TotalLoadedOnline);
                    DownloadProgress.Text = string.Format("{0:F2}%", Math.Round((double)GlobalConfig.CurrentLoadedOnlineIndex / (GlobalConfig.TotalLoadedOnline - 1), 4) * 100);
                });
            }
            Dispatcher.Invoke(() =>
            {
                UpdateTip.Text = Application.Current.Resources["Main.Bottom.Analyzing.Suc"].ToString();
                UpdateProgress.Foreground = (Brush)Application.Current.Resources["SuccessBrush"];
                Thread.Sleep(1000); DownloadingPanel.Hide();
            });
        }));
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
        if (GlobalMsgCrossing.HasGlobalMsg) {
            Extensions.ShowTip.Invoke(GlobalMsgCrossing.GlobalMsg[0].Text, Icons.INFO);
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
