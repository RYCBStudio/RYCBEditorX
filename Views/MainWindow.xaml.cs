using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using RYCBEditorX.Dialogs.ViewModels;
using RYCBEditorX.Dialogs.Views;
using RYCBEditorX.Utils;

namespace RYCBEditorX.Views;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    internal static MainWindow Instance
    {
        get; set;
    }

    internal static List<TabItemStyle> Tabs
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
        Tabs = [];
        NotificationsList.ItemsSource = Notifications;
        if (GlobalConfig.Skin == "dark")
        {
            MainGrid.Background = (Brush)Application.Current.Resources["DarkBackgroud"];
        }
        else
        {
            MainGrid.Background = (Brush)Application.Current.Resources["LightBackgroud"];
        }
        //MainTabCtrl.ItemsSource = texts;
        //FluentMessageBox.Theme = "Error";
        //new FluentMessageBox()
        //{
        //    DataContext = new FluentMessageBoxViewModel()
        //    {
        //        Title = "Test",
        //        Message = App.AppSettings.Settings["Test"].Value,
        //    },
        //}.ShowDialog();
#pragma warning disable CA1806 // 不要忽略方法结果
        new LightTip(this);
#pragma warning restore CA1806 // 不要忽略方法结果
    }

    private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (MainTabCtrl.SelectedItem is null) { return; }
        Tabs.RemoveAt(MainTabCtrl.SelectedIndex);
        MainTabCtrl.Items.Refresh();
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
        LightTip.ViewModelInstance.Icon = LightTip.INFO;
        LightTip.ViewModelInstance.Content = "Test";
        Notifications.Add(new(LightTip.INFO, (Brush)LightTip.Instance.Resources["InfoColor"], LightTip.ViewModelInstance.Content));
        NotificationsList.Items.Refresh();
        LightTip.Instance.Show();
    }

    private void MWarntest_Click(object sender, RoutedEventArgs e)
    {
        LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["WarnColor"];
        LightTip.ViewModelInstance.Icon = LightTip.WARN;
        LightTip.ViewModelInstance.Content = "Test";
        Notifications.Add(new(LightTip.WARN, (Brush)LightTip.Instance.Resources["WarnColor"], LightTip.ViewModelInstance.Content));
        NotificationsList.Items.Refresh();
        LightTip.Instance.Show();
    }

    private void MErrtest_Click(object sender, RoutedEventArgs e)
    {
        LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["ErrorColor"];
        LightTip.ViewModelInstance.Icon = LightTip.ERROR;
        LightTip.ViewModelInstance.Content = "Test";
        Notifications.Add(new(LightTip.ERROR, (Brush)LightTip.Instance.Resources["ErrorColor"], LightTip.ViewModelInstance.Content));
        NotificationsList.Items.Refresh();
        LightTip.Instance.Show();
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
