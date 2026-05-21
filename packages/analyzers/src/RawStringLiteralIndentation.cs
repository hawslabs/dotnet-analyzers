using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

internal static class RawStringLiteralIndentation {
	public static bool TryGetClosingDelimiter(
		string textBeforeClosingParen,
		out string indent,
		out string delimiter
	) {
		indent = string.Empty;
		delimiter = string.Empty;
		var delimiterStart = 0;

		while (
			delimiterStart < textBeforeClosingParen.Length
			&& textBeforeClosingParen[delimiterStart] is ' ' or '\t'
		) {
			delimiterStart++;
		}

		var delimiterEnd = textBeforeClosingParen.Length;

		while (
			delimiterEnd > delimiterStart
			&& textBeforeClosingParen[delimiterEnd - 1] is ' ' or '\t'
		) {
			delimiterEnd--;
		}

		if (delimiterEnd - delimiterStart < 3) {
			return false;
		}

		for (var index = delimiterStart; index < delimiterEnd; index++) {
			if (textBeforeClosingParen[index] != '"') {
				return false;
			}
		}

		indent = textBeforeClosingParen.Substring(0, delimiterStart);
		delimiter = textBeforeClosingParen.Substring(delimiterStart, delimiterEnd - delimiterStart);
		return true;
	}

	public static bool TryAddContentIndentChanges(
		SourceText text,
		int firstContentLineNumber,
		int closingLineNumber,
		string currentRawStringIndent,
		string expectedRawStringIndent,
		List<TextChange> changes
	) {
		for (
			var lineNumber = firstContentLineNumber;
			lineNumber < closingLineNumber;
			lineNumber++
		) {
			var line = text.Lines[lineNumber];
			var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));

			if (lineText.Length == 0) {
				continue;
			}

			if (currentRawStringIndent.Length == 0) {
				changes.Add(new TextChange(new TextSpan(line.Start, 0), expectedRawStringIndent));
				continue;
			}

			if (!lineText.StartsWith(currentRawStringIndent, StringComparison.Ordinal)) {
				if (string.IsNullOrWhiteSpace(lineText)) {
					continue;
				}

				return false;
			}

			changes.Add(
				new TextChange(
					new TextSpan(line.Start, currentRawStringIndent.Length),
					expectedRawStringIndent
				)
			);
		}

		return true;
	}
}
