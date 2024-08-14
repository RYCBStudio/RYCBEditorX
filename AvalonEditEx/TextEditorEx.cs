using System;
using System.Collections.Generic;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using RYCBEditorX.Views;

namespace RYCBEditorX.AvalonEditEx;
internal class TextEditorEx : TextEditor
{
    private HashSet<int> breakpoints = [];
    private BreakpointRenderer backgroundRenderer = new();


    public TextEditorEx()
    {
        backgroundRenderer.IsBreakpointSet = IsBreakpointSet;
        this.TextChanged += OnTextChanged;
        this.TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
        this.TextArea.MouseLeftButtonDown += (sender, e) =>
        {
            var position = e.GetPosition(this.TextArea);
            var line = TextArea.TextView.GetDocumentLineByVisualTop(position.Y).LineNumber;

            ToggleBreakpoint(line);
        };
    }

    public void OnTextChanged(object sender, EventArgs e)
    {
        MainWindow.Instance.FileSavingTip.Text = App.Current.Resources["Main.Bottom.FileSavingTip.Waiting"].ToString();
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
