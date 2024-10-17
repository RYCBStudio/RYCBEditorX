using System;
using System.Windows.Media;
using RYCBEditorX.Dialogs.Views;
using RYCBEditorX.Utils;

namespace RYCBEditorX.Crossing;
public class DialogCrossing : ICrossing
{

    /// <summary>
    /// 显示信息
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="type">接受三种值: <see cref="Icons.ERROR"/>, <see cref="Icons.WARN"/>, <see cref="Icons.INFO"/>.</param>
    public static bool ShowTip(string content, string type = Icons.ERROR)
    {
        if (type != Icons.INFO && type != Icons.WARN && type != Icons.ERROR)
        {
            App.LOGGER.Error(new ArgumentException("type不为指定类型。"));
            return false;
        }
        switch (type)
        {
            case Icons.INFO:
                LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["InfoColor"];
                break;
            case Icons.WARN:
                LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["WarnColor"];
                break;
            case Icons.ERROR:
                LightTip.ViewModelInstance.IconBrush = (Brush)LightTip.Instance.Resources["ErrorColor"];
                break;
        }
        LightTip.ViewModelInstance.Icon = type;
        LightTip.ViewModelInstance.Content = content;
        Views.MainWindow.Notifications.Add(new(type, LightTip.ViewModelInstance.IconBrush, LightTip.ViewModelInstance.Content));
        Views.MainWindow.Instance.NotificationsList.Items.Refresh();
        try
        {
            LightTip.Instance.Show();
        }
        catch
        {
            return false;
        }
        Views.MainWindow.Instance.NotificationBadge.BadgeMargin = new(0, 1, 1, 0);
        return true;
    }

    public void Register()
    {
        Utils.Extensions.ShowTip = ShowTip;
    }
}
