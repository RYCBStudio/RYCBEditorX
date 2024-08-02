using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

    private void FontSizeChange()
    {
        var titlesize = ((SystemParameters.PrimaryScreenWidth / 12) / 3 * 2) / 5;
        Application.Current.Resources.Remove("TitleFontSize");
        Application.Current.Resources.Add("TitleFontSize", titlesize);
        var tabsize = ((SystemParameters.PrimaryScreenWidth / 12) / 3 * 2) / 5 * 0.9;
        Application.Current.Resources.Remove("TabFontSize");
        Application.Current.Resources.Add("TabFontSize", tabsize);
        var gridsize = ((SystemParameters.PrimaryScreenWidth / 12) / 3 * 2) / 5 * 0.8;
        Application.Current.Resources.Remove("GridFontSize");
        Application.Current.Resources.Add("GridFontSize", gridsize);
        var controlsize = ((SystemParameters.PrimaryScreenWidth / 12) / 3 * 2) / 5 * 0.7;
        Application.Current.Resources.Remove("ControlFontSize");
        Application.Current.Resources.Add("ControlFontSize", controlsize);
    }

    private void Window_Loaded(object sender, DependencyPropertyChangedEventArgs e)
    {
        FontSizeChange();
    }
}
