using System.Diagnostics;
using System.Windows.Controls;

namespace RYCBEditorX.ControlEx;

public class ConsoleHost : UserControl
{
    private readonly Process _consoleProcess;
    private readonly System.IO.StreamReader _consoleReader;
    private readonly System.IO.StreamWriter _consoleWriter;

    public ConsoleHost()
    {
        // 创建控制台进程
        _consoleProcess = new Process();
        _consoleProcess.StartInfo.FileName = "cmd.exe";
        _consoleProcess.StartInfo.UseShellExecute = false;
        _consoleProcess.StartInfo.RedirectStandardInput = true;
        _consoleProcess.StartInfo.RedirectStandardOutput = true;
        _consoleProcess.StartInfo.RedirectStandardError = true;
        _consoleProcess.StartInfo.CreateNoWindow = true;
        _consoleProcess.Start();

        // 获取控制台的输入输出
        _consoleReader = _consoleProcess.StandardOutput;
        _consoleWriter = _consoleProcess.StandardInput;

        // 创建 TextBox 用于显示控制台输出
        var consoleTextBox = new TextBox
        {
            IsReadOnly = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        consoleTextBox.TextChanged += ConsoleTextBox_TextChanged;

        // 将 TextBox 添加到 UserControl 中
        this.Content = consoleTextBox;
    }

    private void ConsoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // 读取控制台输出并更新到 TextBox 中
        var output = _consoleReader.ReadToEnd();
        ((TextBox)sender).Text = output;
    }

    public void SendInput(string input)
    {
        // 向控制台发送输入
        _consoleWriter.WriteLine(input);
        _consoleWriter.Flush();
    }
}