using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;
using System.Windows;
using RYCBEditorX.AvalonEditEx;

namespace RYCBEditorX.ControlEx
{
    public class PythonEditor:TextEditor
    {
        public static readonly DependencyProperty TextEditorCodeProperty =
            DependencyProperty.Register("TextEditorCode", typeof(object), typeof(PythonEditor), new PropertyMetadata(null, (d,e) =>
            {
                if (d is PythonEditor editor)
                {
                    editor.Text = e.NewValue?.ToString() ?? string.Empty;
                }
            }));

        public object TextEditorCode
        {
            get => GetValue(TextEditorCodeProperty);
            set => SetValue(TextEditorCodeProperty, value);
        }

        public PythonEditor()
        {
            this.ShowLineNumbers = GlobalConfig.Editor.ShowLineNumber;
            this.FontFamily = new(GlobalConfig.Editor.FontFamilyName);
            this.FontSize = GlobalConfig.Editor.FontSize;
            if (GlobalConfig.Skin == "dark")
            {
                this.Foreground = (Brush)Application.Current.Resources["LightBackGround"];
            }
            else
            {
                this.Foreground = (Brush)Application.Current.Resources["DarkBackGround"];
            }
            this.TextArea.TextView.LinkTextForegroundBrush = (Brush)GlobalConfig.Resources["LinkForeGround"];
            var resourceName = GlobalConfig.XshdFilePath + $"{GlobalConfig.Editor.Theme}\\python.xshd";
            using Stream s = new FileStream(resourceName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using System.Xml.XmlTextReader reader = new(s);
            var xshd = HighlightingLoader.LoadXshd(reader);
            this.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
            this.Loaded += (s, e) =>
            {
                this.Text = TextEditorCode.ToString();
            };
        }
    }
}
