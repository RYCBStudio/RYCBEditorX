using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace RYCBEditorX.AvalonEditEx;
internal class TestTextEditorEx : TextEditor
{
    private HashSet<int> breakpoints = [];
    private BreakpointRenderer backgroundRenderer = new();

    public TestTextEditorEx()
    {
        backgroundRenderer.IsBreakpointSet = IsBreakpointSet;
        this.TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
        this.TextArea.MouseLeftButtonDown += (sender, e) =>
        {
            var position = e.GetPosition(this.TextArea);
            var line = TextArea.TextView.GetDocumentLineByVisualTop(position.Y).LineNumber;

            ToggleBreakpoint(line);
        };
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
