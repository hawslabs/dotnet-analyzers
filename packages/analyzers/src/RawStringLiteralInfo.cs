using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HawsLabs.Analyzers;

internal readonly struct RawStringLiteralInfo {
	private RawStringLiteralInfo(
		int openingDelimiterStart,
		int openingDelimiterLength,
		int closingDelimiterStart,
		string delimiter
	) {
		OpeningDelimiterStart = openingDelimiterStart;
		OpeningDelimiterLength = openingDelimiterLength;
		ClosingDelimiterStart = closingDelimiterStart;
		Delimiter = delimiter;
	}

	public int OpeningDelimiterStart { get; }

	public int OpeningDelimiterLength { get; }

	public int ClosingDelimiterStart { get; }

	public string Delimiter { get; }

	public static bool TryCreate(ExpressionSyntax expression, out RawStringLiteralInfo rawStringLiteral) {
		rawStringLiteral = default;
		var expressionText = expression.ToString();
		var dollarCount = 0;

		while (dollarCount < expressionText.Length && expressionText[dollarCount] == '$') {
			dollarCount++;
		}

		var quoteCount = 0;

		while (
			dollarCount + quoteCount < expressionText.Length
			&& expressionText[dollarCount + quoteCount] == '"'
		) {
			quoteCount++;
		}

		var delimiter = new string('"', quoteCount);

		if (!IsMultilineRawStringExpressionText(expressionText, delimiter)) {
			return false;
		}

		rawStringLiteral = new RawStringLiteralInfo(
			expression.SpanStart,
			dollarCount + quoteCount,
			expression.Span.End - quoteCount,
			delimiter
		);

		return true;
	}

	public static bool TryCreate(SyntaxToken token, out RawStringLiteralInfo rawStringLiteral) {
		rawStringLiteral = default;
		var tokenText = token.Text;
		var delimiterLength = 0;

		while (delimiterLength < tokenText.Length && tokenText[delimiterLength] == '"') {
			delimiterLength++;
		}

		var delimiter = new string('"', delimiterLength);

		if (!IsMultilineRawStringText(tokenText, delimiter)) {
			return false;
		}

		rawStringLiteral = new RawStringLiteralInfo(
			token.SpanStart,
			delimiterLength,
			token.Span.End - delimiterLength,
			delimiter
		);

		return true;
	}

	private static bool IsMultilineRawStringText(string text, string delimiter) {
		return delimiter.Length >= 3
			&& text.StartsWith(delimiter, StringComparison.Ordinal)
			&& text.EndsWith(delimiter, StringComparison.Ordinal)
			&& text.Any(static character => character is '\r' or '\n');
	}

	private static bool IsMultilineRawStringExpressionText(string text, string delimiter) {
		return delimiter.Length >= 3
			&& text.EndsWith(delimiter, StringComparison.Ordinal)
			&& text.Any(static character => character is '\r' or '\n');
	}
}
