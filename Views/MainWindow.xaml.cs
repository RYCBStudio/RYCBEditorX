using System.Collections.Generic;
using System.Configuration;
using System.Windows;
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

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        Tabs = [];
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
