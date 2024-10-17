using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Tools.Extension;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using RYCBEditorX.Views;

namespace RYCBEditorX.AvalonEditEx;
public class TextEditorEx : TextEditor
{
    private readonly HashSet<int> breakpoints = [];
    private readonly BreakpointRenderer backgroundRenderer = new();
    private bool _ctrl, _alt, _shift;

    public Action CustomCmd;

    public TextEditorEx()
    {
        backgroundRenderer.IsBreakpointSet = IsBreakpointSet;
        TextChanged += OnTextChanged;
        PreviewKeyDown += TextEditorEx_PreviewKeyDown;
        KeyDown += TextEditorEx_KeyDown;
        KeyUp += TextEditorEx_KeyUp;
        TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
        TextArea.MouseLeftButtonDown += (sender, e) =>
        {
            var position = e.GetPosition(TextArea);
            var line = TextArea.TextView.GetDocumentLineByVisualTop(position.Y).LineNumber;

            ToggleBreakpoint(line);
        };
        TextArea.TextView.SetValue(Typography.StandardLigaturesProperty, false);
        TextArea.SetValue(Typography.StandardLigaturesProperty, false);
        Options.HighlightCurrentLine = true;
        TextArea.TextView.VisualLinesChanged += (s, e) =>
        {
            TextArea.TextView.CurrentLineBackground?.SetValue(OpacityProperty, .5f);
        };
        //this.TextArea.OverstrikeMode = false;
        SetValue(Typography.StandardLigaturesProperty, false);
    }

    private void TextEditorEx_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _ctrl = false;
        }
        else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            _alt = false;
        }
        else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _shift = false;
        }
    }

    private void TextEditorEx_KeyDown(object sender, KeyEventArgs e)
    {
        if (_ctrl & e.Key == Key.W) {
            CustomCmd.Invoke();
            Wiki.Instance.Header.Text = SelectedText;
            Wiki.Instance.LoadData();
        }
    }

    private void TextEditorEx_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _ctrl = true;
        }
        else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            _alt = true;
        }
        else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _shift = true;
        }
    }

    public void OnTextChanged(object sender, EventArgs e)
    {
        MainWindow.Instance.FileSavingPanel.Show();
        MainWindow.Instance.FileSavingTip.Text = System.Windows.Application.Current.Resources["Main.Bottom.FileSavingTip.Waiting"].ToString();
    }

    public void ToggleBreakpoint(int line)
    {
        if (breakpoints.Contains(line))
        {
            breakpoints.Remove(line);
        }
        else
        {
            breakpoints.Add(line);
        }

        // 刷新渲染层
        TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    private bool IsBreakpointSet(int line)
    {
        return breakpoints.Contains(line);
    }
}
