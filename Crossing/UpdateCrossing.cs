using System;
using RYCBEditorX.Utils;
using RYCBEditorX.Views;
using RYCBEditorX.Utils.Update;
using static RYCBEditorX.Utils.Extensions;

namespace RYCBEditorX.Crossing;
public class UpdateCrossing : ICrossing
{
    public static void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
    {
        MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.DownloadSpeed.Text = ProcessFileSize((long)e.BytesPerSecondSpeed) + "/s");
        var r = ProcessFileSize(e.ReceivedBytesSize);
        var t = ProcessFileSize(e.TotalBytesToReceive);
        MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.UpdateRTProgress.Text = $" {r} / {t}");
        MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgress.Value = (int)e.ReceivedBytesSize);
        MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.DownloadProgress.Text = (Math.Round((double)MainWindow.Instance.UpdateProgress.Value / MainWindow.Instance.UpdateProgress.Maximum, 4) * 100).ToString());
    }

    public static void Downloader_DownloadTestFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error is not null) { App.LOGGER.Error(e.Error, "测试文件下载失败!", EnumLogType.WARN, EnumLogPort.SERVER); }
        App.LOGGER.Log("测试文件下载完成", EnumLogType.INFO, EnumLogPort.CLIENT, EnumLogModule.NET);
        UpdateUtils.UpdateCheckOK = true;
    }

    public static void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error is not null) { App.LOGGER.Error(e.Error, "文件下载失败!", EnumLogType.WARN, EnumLogPort.SERVER); }
        App.LOGGER.Log("文件下载完成", EnumLogType.INFO, EnumLogPort.CLIENT, EnumLogModule.NET);
    }

    public static void Downloader_DownloadStarted(object sender, Downloader.DownloadStartedEventArgs e)
    {
        App.LOGGER.Log("开始下载文件", EnumLogType.INFO, EnumLogPort.CLIENT, EnumLogModule.NET);
        App.LOGGER.Log("文件名：" + e.FileName, EnumLogType.INFO, EnumLogPort.SERVER, EnumLogModule.NET);
        App.LOGGER.Log("文件大小：" + ProcessFileSize(e.TotalBytesToReceive), EnumLogType.INFO, EnumLogPort.SERVER, EnumLogModule.NET);
        MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgress.Maximum = (int)e.TotalBytesToReceive);
    }

    public void Register()
    {
        UpdateUtils.DownloadProgressChanged = Downloader_DownloadProgressChanged;
        UpdateUtils.DownloadCompleted = Downloader_DownloadFileCompleted;
        UpdateUtils.DownloadStarted = Downloader_DownloadStarted;
    }
}
