using System.Threading.Tasks;
using System.Windows;

namespace RYCBEditorX.Views;
/// <summary>
/// ProgressedInfoTip.xaml 的交互逻辑
/// </summary>
public partial class ProgressedInfoTip : HandyControl.Controls.Window
{
    private ViewModels.ProgressedTipViewModel _vm;

    public ProgressedInfoTip()
    {
        InitializeComponent();
        _vm = new ViewModels.ProgressedTipViewModel();
        DataContext = _vm;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Init(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(Change);
    }

    private async void Change()
    {
        int n = 0, t = 0;
        GlobalConfig.LocalPackages =  await Task.Run(()=>DocstringProcessor.ProcessJsonFiles("F:\\Temp\\Python", ref n, ref t));
        Dispatcher.Invoke(()=> { ProgBar.Value = n; ProgBar.Maximum = t; });
        GlobalConfig.LocalDocs = PythonPackageParser.GetPackageMethodDocstrings(GlobalConfig.LocalPackages);
        Title.Text = "完成";
    }
}
