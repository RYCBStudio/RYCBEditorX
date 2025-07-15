using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Media;
using RYCBEditorX.Views;
using RYCBEditorX.Utils;

namespace RYCBEditorX;
public class CompletionData(string text, CompletionDataType type, string templateReference = "", string desc = "") : ICompletionData
{
    public ImageSource Image => null;

    public string Text
    {
        get;
    } = text;

    public string TemplateReference
    {
        get;
    } = templateReference;

    public CompletionDataType Type
    {
        get; set;
    } = type;

    public object Content => new CodeSenseSelection(Text, GetCandidatesIcon(Type));

    public object Description => new CodeSenseDescription(Text, desc.IsNullOrEmpty() ? ((CodeSenseSelection)Content).Candidates : desc, templateReference);

    /// <inheritdoc />
    public double Priority
    {
        get;
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        if (completionSegment == null)
        {
            return;
        }

        // 获取用户输入的起始位置和结束位置
        var startOffset = completionSegment.Offset;
        var endOffset = completionSegment.EndOffset;

        // 获取要替换的内容
        string replacementText;
        if (Type == CompletionDataType.CodeTemplate)
        {
            replacementText = TemplateReference;
        }
        else
        {
            replacementText = Text;
        }

        // 执行替换操作
        // 确保替换范围准确，覆盖用户输入的内容
        textArea.Document.Replace(startOffset - 1, endOffset - startOffset + 1, replacementText);
    }

    private static string GetCandidatesIcon(CompletionDataType icon)
    {
        return icon switch
        {
            CompletionDataType.Keyword => Icons.KEYWORD,
            CompletionDataType.Builtin => Icons.BUILTIN,
            CompletionDataType.Function => Icons.FUNCTION,
            CompletionDataType.Magic => Icons.MAGIC,
            CompletionDataType.Variable => Icons.VARIABLE,
            CompletionDataType.CodeTemplate => Icons.CT,
            _ => "",
        };
    }
}
