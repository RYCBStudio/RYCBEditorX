using System;
using System.Windows;
using RYCBEditorX.Utils;

namespace RYCBEditorX.Views;
/// <summary>
/// Splash.xaml 的交互逻辑
/// </summary>
public partial class Splash : Window
{

    public Splash()
    {
        InitializeComponent();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        GlobalWindows.ActivatingWindows.Remove(this);
    }
}
