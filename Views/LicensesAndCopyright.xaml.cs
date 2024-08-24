using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RYCBEditorX.Views;
/// <summary>
/// LicensesAndCopyright.xaml 的交互逻辑
/// </summary>
public partial class LicensesAndCopyright : HandyControl.Controls.Window
{
    public LicensesAndCopyright()
    {
        InitializeComponent();
        MyListBox.Dispatcher.BeginInvoke(
                new Action(ApplyAnimation),
                System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void ApplyAnimation()
    {
        var storyboard = (Storyboard)FindResource("ItemAppearStoryboard");
        var index = 0;

        foreach (var item in MyListBox.Items)
        {
            var listBoxItem = (ListBoxItem)MyListBox.ItemContainerGenerator.ContainerFromIndex(index);

            if (listBoxItem != null)
            {
                // 延迟动画的开始时间
                var animation = storyboard.Clone();
                animation.BeginTime = TimeSpan.FromSeconds(index * 0.025); // 每个项间隔0.025秒
                Storyboard.SetTarget(animation, listBoxItem);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                animation.Begin();
            }
            index++;
        }
    }

    private void OpenNuget(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = ((FrameworkElement)sender).Tag.ToString(),
            UseShellExecute = true
        });
    }
}
