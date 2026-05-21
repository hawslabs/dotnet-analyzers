using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HangingListClosingParenCodeFixProvider))]
[Shared]
public sealed class HangingListClosingParenCodeFixProvider : CodeFixProvider {
	private const string Title = "Format hanging-list closing parenthesis";

	public override ImmutableArray<string> FixableDiagnosticIds =>
		ImmutableArray.Create(HangingListClosingParenAnalyzer.DiagnosticId);

	public override FixAllProvider GetFixAllProvider() =>
		WellKnownFixAllProviders.BatchFixer;

	public override Task RegisterCodeFixesAsync(CodeFixContext context) {
		var diagnostic = context.Diagnostics[0];

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				cancellationToken => FixAsync(context.Document, diagnostic, cancellationToken),
				equivalenceKey: Title
			),
			diagnostic
		);

		return Task.CompletedTask;
	}

	private static async Task<Document> FixAsync(
		Document document,
		Diagnostic diagnostic,
		CancellationToken cancellationToken
	) {
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		if (root is null) {
			return document;
		}

		var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
		var targetToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

		if (TryFixRawStringLiteralIndentation(text, targetToken, out var fixedRawStringLiteralText)) {
			return document.WithText(fixedRawStringLiteralText);
		}

		var closeParen = targetToken;
		if (!closeParen.IsKind(SyntaxKind.CloseParenToken)) {
			return document;
		}

		var openParen = closeParen.Parent switch {
			ArgumentListSyntax argumentList => argumentList.OpenParenToken,
			ParameterListSyntax parameterList => parameterList.OpenParenToken,
			WhileStatementSyntax whileStatement => whileStatement.OpenParenToken,
			_ => default,
		};

		if (openParen.IsMissing) {
			return document;
		}

		var openLine = text.Lines.GetLineFromPosition(openParen.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var expectedIndent = GetLineIndentation(text, openLine);
		var actualPrefixSpan = TextSpan.FromBounds(closeLine.Start, closeParen.SpanStart);
		var actualPrefix = text.ToString(actualPrefixSpan);
		var lineBreak = GetLineBreak(text, closeLine);

		if (TryFixExpressionBodyArrowLine(text, closeParen, out var fixedExpressionBodyText)) {
			return document.WithText(fixedExpressionBodyText);
		}

		if (TryFixBaseListLine(text, closeParen, out var fixedBaseListText)) {
			return document.WithText(fixedBaseListText);
		}

		if (
			TryFixRawStringLiteralClosingLine(
				text,
				closeParen,
				closeLine,
				actualPrefixSpan,
				actualPrefix,
				expectedIndent,
				lineBreak,
				out var fixedRawStringText
			)
		) {
			return document.WithText(fixedRawStringText);
		}

		if (
			TryFixRawStringLiteralBeforeClosingLine(
				text,
				closeParen,
				actualPrefixSpan,
				actualPrefix,
				expectedIndent,
				out fixedRawStringText
			)
		) {
			return document.WithText(fixedRawStringText);
		}

		if (actualPrefix.All(static character => character is ' ' or '\t')) {
			return document.WithText(text.Replace(actualPrefixSpan, expectedIndent));
		}

		var fixedText = text.Replace(
			new TextSpan(closeParen.SpanStart, 0),
			lineBreak + expectedIndent
		);

		return document.WithText(fixedText);
	}

	private static bool TryFixExpressionBodyArrowLine(
		SourceText text,
		SyntaxToken closeParen,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			closeParen.Parent is not ParameterListSyntax parameterList
			|| !TryGetExpressionBody(parameterList, out var expressionBody)
			|| expressionBody.ArrowToken.IsMissing
		) {
			return false;
		}

		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var arrowLine = text.Lines.GetLineFromPosition(expressionBody.ArrowToken.SpanStart);

		if (closeLine.LineNumber == arrowLine.LineNumber) {
			return false;
		}

		var gapSpan = TextSpan.FromBounds(closeParen.Span.End, expressionBody.ArrowToken.SpanStart);
		var gapText = text.ToString(gapSpan);

		if (!gapText.All(static character => char.IsWhiteSpace(character))) {
			return false;
		}

		fixedText = text.Replace(gapSpan, " ");
		return true;
	}

	private static bool TryFixBaseListLine(
		SourceText text,
		SyntaxToken closeParen,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			closeParen.Parent is not ParameterListSyntax parameterList
			|| !TryGetBaseList(parameterList, out var baseList)
			|| baseList.ColonToken.IsMissing
		) {
			return false;
		}

		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var colonLine = text.Lines.GetLineFromPosition(baseList.ColonToken.SpanStart);

		if (closeLine.LineNumber == colonLine.LineNumber) {
			return false;
		}

		var gapSpan = TextSpan.FromBounds(closeParen.Span.End, baseList.ColonToken.SpanStart);
		var gapText = text.ToString(gapSpan);

		if (!gapText.All(static character => char.IsWhiteSpace(character))) {
			return false;
		}

		fixedText = text.Replace(gapSpan, " ");
		return true;
	}

	private static bool TryFixRawStringLiteralClosingLine(
		SourceText text,
		SyntaxToken closeParen,
		TextLine closeLine,
		TextSpan actualPrefixSpan,
		string actualPrefix,
		string expectedIndent,
		string lineBreak,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			!TryGetRawStringClosingDelimiter(
				actualPrefix,
				out var currentRawStringIndent,
				out var rawStringDelimiter
			)
		) {
			return false;
		}

		var rawStringToken = closeParen.GetPreviousToken();

		if (!IsMultilineRawStringLiteral(rawStringToken, rawStringDelimiter)) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawStringToken.SpanStart);

		if (openingLine.LineNumber == closeLine.LineNumber) {
			return false;
		}

		var expectedRawStringIndent = GetLineIndentation(text, openingLine);
		var changes = new List<TextChange> {
			new(
				actualPrefixSpan,
				expectedRawStringIndent + rawStringDelimiter + lineBreak + expectedIndent
			),
		};

		if (
			expectedRawStringIndent != currentRawStringIndent
			&& !TryAddRawStringContentIndentChanges(
				text,
				openingLine.LineNumber + 1,
				closeLine.LineNumber,
				currentRawStringIndent,
				expectedRawStringIndent,
				changes
			)
		) {
			return false;
		}

		fixedText = text.WithChanges(changes.OrderBy(static change => change.Span.Start));
		return true;
	}

	private static bool TryFixRawStringLiteralBeforeClosingLine(
		SourceText text,
		SyntaxToken closeParen,
		TextSpan actualPrefixSpan,
		string actualPrefix,
		string expectedIndent,
		out SourceText fixedText
	) {
		fixedText = text;

		if (!actualPrefix.All(static character => character is ' ' or '\t')) {
			return false;
		}

		var rawStringToken = closeParen.GetPreviousToken();

		if (!TryGetMultilineRawStringDelimiter(rawStringToken, out var rawStringDelimiter)) {
			return false;
		}

		var closingDelimiterStart = rawStringToken.Span.End - rawStringDelimiter.Length;

		if (closingDelimiterStart < rawStringToken.SpanStart) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawStringToken.SpanStart);
		var rawStringClosingLine = text.Lines.GetLineFromPosition(closingDelimiterStart);
		var closeParenLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);

		if (
			openingLine.LineNumber == rawStringClosingLine.LineNumber
			|| rawStringClosingLine.LineNumber >= closeParenLine.LineNumber
		) {
			return false;
		}

		var expectedRawStringIndent = GetLineIndentation(text, openingLine);
		var currentRawStringIndent = GetLineIndentation(text, rawStringClosingLine);
		var changes = new List<TextChange>();

		if (expectedRawStringIndent != currentRawStringIndent) {
			if (
				!TryAddRawStringContentIndentChanges(
					text,
					openingLine.LineNumber + 1,
					rawStringClosingLine.LineNumber,
					currentRawStringIndent,
					expectedRawStringIndent,
					changes
				)
			) {
				return false;
			}

			changes.Add(
				new TextChange(
					TextSpan.FromBounds(rawStringClosingLine.Start, closingDelimiterStart),
					expectedRawStringIndent
				)
			);
		}

		if (actualPrefix != expectedIndent) {
			changes.Add(new TextChange(actualPrefixSpan, expectedIndent));
		}

		if (changes.Count == 0) {
			return false;
		}

		fixedText = text.WithChanges(changes.OrderBy(static change => change.Span.Start));
		return true;
	}

	private static bool TryFixRawStringLiteralIndentation(
		SourceText text,
		SyntaxToken rawStringToken,
		out SourceText fixedText
	) {
		fixedText = text;

		if (!TryGetMultilineRawStringDelimiter(rawStringToken, out var rawStringDelimiter)) {
			return false;
		}

		var closingDelimiterStart = rawStringToken.Span.End - rawStringDelimiter.Length;

		if (closingDelimiterStart < rawStringToken.SpanStart) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawStringToken.SpanStart);
		var closingLine = text.Lines.GetLineFromPosition(closingDelimiterStart);

		if (openingLine.LineNumber == closingLine.LineNumber) {
			return false;
		}

		var expectedRawStringIndent = GetLineIndentation(text, openingLine);
		var currentRawStringIndent = GetLineIndentation(text, closingLine);

		if (expectedRawStringIndent == currentRawStringIndent) {
			return false;
		}

		var changes = new List<TextChange>();

		if (
			!TryAddRawStringContentIndentChanges(
				text,
				openingLine.LineNumber + 1,
				closingLine.LineNumber,
				currentRawStringIndent,
				expectedRawStringIndent,
				changes
			)
		) {
			return false;
		}

		changes.Add(
			new TextChange(
				TextSpan.FromBounds(closingLine.Start, closingDelimiterStart),
				expectedRawStringIndent
			)
		);

		fixedText = text.WithChanges(changes.OrderBy(static change => change.Span.Start));
		return true;
	}

	private static bool TryAddRawStringContentIndentChanges(
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

	private static bool TryGetRawStringClosingDelimiter(
		string actualPrefix,
		out string indent,
		out string delimiter
	) {
		indent = string.Empty;
		delimiter = string.Empty;

		var delimiterStart = 0;

		while (
			delimiterStart < actualPrefix.Length
			&& actualPrefix[delimiterStart] is ' ' or '\t'
		) {
			delimiterStart++;
		}

		var delimiterEnd = actualPrefix.Length;

		while (
			delimiterEnd > delimiterStart
			&& actualPrefix[delimiterEnd - 1] is ' ' or '\t'
		) {
			delimiterEnd--;
		}

		if (delimiterEnd - delimiterStart < 3) {
			return false;
		}

		for (var index = delimiterStart; index < delimiterEnd; index++) {
			if (actualPrefix[index] != '"') {
				return false;
			}
		}

		indent = actualPrefix.Substring(0, delimiterStart);
		delimiter = actualPrefix.Substring(delimiterStart, delimiterEnd - delimiterStart);
		return true;
	}

	private static bool TryGetMultilineRawStringDelimiter(
		SyntaxToken token,
		out string delimiter
	) {
		delimiter = string.Empty;

		var tokenText = token.Text;
		var delimiterLength = 0;

		while (delimiterLength < tokenText.Length && tokenText[delimiterLength] == '"') {
			delimiterLength++;
		}

		if (delimiterLength < 3) {
			return false;
		}

		delimiter = tokenText.Substring(0, delimiterLength);
		return IsMultilineRawStringLiteral(token, delimiter);
	}

	private static bool IsMultilineRawStringLiteral(
		SyntaxToken token,
		string delimiter
	) {
		var tokenText = token.Text;

		return tokenText.StartsWith(delimiter, StringComparison.Ordinal)
			&& tokenText.EndsWith(delimiter, StringComparison.Ordinal)
			&& tokenText.Any(static character => character is '\r' or '\n');
	}

	private static string GetLineIndentation(SourceText text, TextLine line) {
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

	private static string GetLineBreak(SourceText text) {
		foreach (var line in text.Lines) {
			if (line.EndIncludingLineBreak > line.End) {
				return text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
			}
		}

		return "\r\n";
	}

	private static string GetLineBreak(SourceText text, TextLine preferredLine) {
		if (preferredLine.EndIncludingLineBreak > preferredLine.End) {
			return text.ToString(TextSpan.FromBounds(preferredLine.End, preferredLine.EndIncludingLineBreak));
		}

		return GetLineBreak(text);
	}

	private static bool TryGetExpressionBody(
		ParameterListSyntax node,
		out ArrowExpressionClauseSyntax expressionBody
	) {
		expressionBody = node.Parent switch {
			ConstructorDeclarationSyntax { ExpressionBody: { } value } => value,
			ConversionOperatorDeclarationSyntax { ExpressionBody: { } value } => value,
			LocalFunctionStatementSyntax { ExpressionBody: { } value } => value,
			MethodDeclarationSyntax { ExpressionBody: { } value } => value,
			OperatorDeclarationSyntax { ExpressionBody: { } value } => value,
			_ => null!,
		};

		return expressionBody is not null;
	}

	private static bool TryGetBaseList(
		ParameterListSyntax node,
		out BaseListSyntax baseList
	) {
		baseList = node.Parent switch {
			TypeDeclarationSyntax { BaseList: { } value } => value,
			_ => null!,
		};

		return baseList is not null;
	}
}
