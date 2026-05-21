using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnRawStringLiteralCodeFixProvider))]
[Shared]
public sealed class ReturnRawStringLiteralCodeFixProvider : CodeFixProvider {
	private const string Title = "Format multiline raw string literal";

	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ReturnRawStringLiteralAnalyzer.DiagnosticId);

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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

		var targetToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
		var rawStringExpression = targetToken.Parent?
			.AncestorsAndSelf()
			.OfType<ExpressionSyntax>()
			.FirstOrDefault(static expression => TryGetMultilineRawStringExpression(expression, out _));

		if (rawStringExpression is null) {
			return document;
		}

		if (!TryGetMultilineRawStringExpression(rawStringExpression, out var rawString)) {
			return document;
		}

		var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
		var openingLine = text.Lines.GetLineFromPosition(rawString.OpeningDelimiterStart);
		var closingLine = text.Lines.GetLineFromPosition(rawString.ClosingDelimiterStart);
		var changes = new List<TextChange>();

		if (
			!TryGetExpectedRawStringIndent(
				rawStringExpression,
				text,
				rawString,
				openingLine,
				out var expectedRawStringIndent,
				out var openingIndentSpan
			)
		) {
			return document;
		}

		if (!openingIndentSpan.IsEmpty) {
			changes.Add(new TextChange(openingIndentSpan, GetLineBreak(text) + expectedRawStringIndent));
		}

		var currentRawStringIndent = text.ToString(TextSpan.FromBounds(closingLine.Start, rawString.ClosingDelimiterStart));

		if (currentRawStringIndent != expectedRawStringIndent) {
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
				return document;
			}

			changes.Add(
				new TextChange(
					TextSpan.FromBounds(closingLine.Start, rawString.ClosingDelimiterStart),
					expectedRawStringIndent
				)
			);
		}

		return document.WithText(text.WithChanges(changes.OrderBy(static change => change.Span.Start)));
	}

	private static bool TryGetExpectedRawStringIndent(
		ExpressionSyntax expression,
		SourceText text,
		RawStringExpression rawString,
		TextLine openingLine,
		out string expectedRawStringIndent,
		out TextSpan openingIndentSpan
	) {
		expectedRawStringIndent = text.ToString(TextSpan.FromBounds(openingLine.Start, rawString.OpeningDelimiterStart));
		openingIndentSpan = default;

		if (expression.Parent is not ReturnStatementSyntax returnStatement) {
			return true;
		}

		if (returnStatement.Expression != expression) {
			return true;
		}

		if (returnStatement.ReturnKeyword.IsMissing) {
			return false;
		}

		openingIndentSpan = TextSpan.FromBounds(
			returnStatement.ReturnKeyword.Span.End,
			rawString.OpeningDelimiterStart
		);

		var betweenReturnAndExpression = text.ToString(openingIndentSpan);

		if (!betweenReturnAndExpression.All(static character => character is ' ' or '\t' or '\r' or '\n')) {
			return false;
		}

		var returnLine = text.Lines.GetLineFromPosition(returnStatement.ReturnKeyword.SpanStart);
		var returnIndent = GetLineIndentation(text, returnLine);
		expectedRawStringIndent = returnIndent + GetIndentUnit(returnIndent);

		return true;
	}

	private static bool TryGetMultilineRawStringExpression(
		ExpressionSyntax expression,
		out RawStringExpression rawString
	) {
		rawString = default;
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

		if (quoteCount < 3
			|| !expressionText.EndsWith(delimiter, StringComparison.Ordinal)
			|| !expressionText.Any(static character => character is '\r' or '\n')) {
			return false;
		}

		rawString = new RawStringExpression(
			expression.SpanStart,
			expression.Span.End - quoteCount
		);

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

	private static string GetIndentUnit(string lineIndent) {
		if (lineIndent.Contains('\t')) {
			return "\t";
		}

		if (lineIndent.Length >= 4 && lineIndent.Length % 4 == 0) {
			return "    ";
		}

		if (lineIndent.Length >= 2 && lineIndent.Length % 2 == 0) {
			return "  ";
		}

		if (lineIndent.Length > 0) {
			return new string(' ', lineIndent.Length);
		}

		return "\t";
	}

	private static string GetLineBreak(SourceText text) {
		foreach (var line in text.Lines) {
			if (line.EndIncludingLineBreak > line.End) {
				return text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
			}
		}

		return "\r\n";
	}

	private readonly struct RawStringExpression {
		public RawStringExpression(
			int openingDelimiterStart,
			int closingDelimiterStart
		) {
			OpeningDelimiterStart = openingDelimiterStart;
			ClosingDelimiterStart = closingDelimiterStart;
		}

		public int OpeningDelimiterStart { get; }

		public int ClosingDelimiterStart { get; }
	}
}
