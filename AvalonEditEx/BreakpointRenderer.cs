using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows;
using System.Windows.Media;

namespace RYCBEditorX.AvalonEditEx;
public class BreakpointRenderer : IBackgroundRenderer
{
    public KnownLayer Layer => KnownLayer.Background;

    public Func<int, bool> IsBreakpointSet { get; set; }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        for (var i = 1; i <= textView.VisualLines.Count; i++)
        {
            if (IsBreakpointSet(i))
            {
                var visualLine = textView.VisualLines[i-1];
                var lineGeometry = new Rect(0, visualLine.VisualTop, textView.ActualWidth, visualLine.Height);

                var brush = Brushes.Red;
                var radius = 5; // 圆点的半径
                var center = new Point(lineGeometry.Left - radius, lineGeometry.Top + lineGeometry.Height / 2);

                drawingContext.DrawEllipse(brush, null, center, radius, radius);
            }
        }
    }
}