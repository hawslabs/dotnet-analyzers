using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

internal static class SourceTextExtensions {
	public static string GetLineIndentation(this SourceText text, TextLine line) {
		var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));
		var index = 0;

		while (
			index < lineText.Length
			&& (lineText[index] == ' ' || lineText[index] == '\t')
		) {
			index++;
		}

		return lineText.Substring(0, index);
	}

	public static string GetLineBreak(this SourceText text) {
		foreach (var line in text.Lines) {
			if (line.EndIncludingLineBreak > line.End) {
				return text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
			}
		}

		return "\r\n";
	}

	public static string GetLineBreak(this SourceText text, TextLine preferredLine) {
		if (preferredLine.EndIncludingLineBreak > preferredLine.End) {
			return text.ToString(TextSpan.FromBounds(preferredLine.End, preferredLine.EndIncludingLineBreak));
		}

		return text.GetLineBreak();
	}

	public static bool IsWhiteSpace(this SourceText text, TextSpan span) {
		return text.ToString(span).All(static character => char.IsWhiteSpace(character));
	}
}
