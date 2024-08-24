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
using RYCBEditorX.ViewModels;

namespace RYCBEditorX.Views;
/// <summary>
/// ProgressedInfoTip.xaml 的交互逻辑
/// </summary>
public partial class ProgressedInfoTip : HandyControl.Controls.Window
{
    public ProgressedInfoTip()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Init(object sender, RoutedEventArgs e)
    {
        int i = 0, n = 0;
        Task.Run(()=>DocstringProcessor.ProcessJsonFiles("F:\\Temp\\Python", ref i, ref n));
    }
}
