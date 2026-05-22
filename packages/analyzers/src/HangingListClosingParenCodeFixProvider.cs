using System.Collections.Immutable;
using System.Composition;
using System.Text;
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
	private const int DefaultMaxLineLength = 110;

	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
		HangingListClosingParenAnalyzer.DiagnosticId,
		HangingListClosingParenAnalyzer.SplitListItemsDiagnosticId,
		HangingListClosingParenAnalyzer.ParameterListContinuationDiagnosticId,
		HangingListClosingParenAnalyzer.ShortFirstCallDiagnosticId,
		HangingListClosingParenAnalyzer.HangingListItemIndentationDiagnosticId
	);

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
		var expectedIndent = text.GetLineIndentation(openLine);
		var actualPrefixSpan = TextSpan.FromBounds(closeLine.Start, closeParen.SpanStart);
		var actualPrefix = text.ToString(actualPrefixSpan);
		var lineBreak = text.GetLineBreak(closeLine);
		var maxLineLength = GetMaxLineLength(document, root.SyntaxTree);

		if (TryExpandShortFirstCall(text, closeParen, lineBreak, maxLineLength, out var fixedFirstCallText)) {
			return document.WithText(fixedFirstCallText);
		}

		if (TryExpandSplitList(text, closeParen, expectedIndent, lineBreak, out var expandedListText)) {
			return document.WithText(expandedListText);
		}

		if (TryFixHangingListItemIndentation(text, closeParen, expectedIndent, out var fixedItemIndentationText)) {
			return document.WithText(fixedItemIndentationText);
		}

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

	private static bool TryExpandShortFirstCall(
		SourceText text,
		SyntaxToken closeParen,
		string lineBreak,
		int maxLineLength,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			closeParen.Parent is not ArgumentListSyntax { Arguments.Count: 1 } argumentList
			|| argumentList.OpenParenToken.IsMissing
			|| argumentList.CloseParenToken.IsMissing
			|| !argumentList.TryGetFirstInvocationMemberAccess(out var memberAccess)
			|| !CanMoveInvocationToReceiverLine(text, memberAccess, argumentList.OpenParenToken, maxLineLength)
		) {
			return false;
		}

		var argument = argumentList.Arguments[0];
		var firstArgumentToken = argument.GetFirstToken();
		var lastArgumentToken = argument.GetLastToken();

		if (firstArgumentToken.IsMissing || lastArgumentToken.IsMissing) {
			return false;
		}

		var openLine = text.Lines.GetLineFromPosition(argumentList.OpenParenToken.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(argumentList.CloseParenToken.SpanStart);
		var firstArgumentLine = text.Lines.GetLineFromPosition(firstArgumentToken.SpanStart);
		var lastArgumentLine = text.Lines.GetLineFromPosition(lastArgumentToken.SpanStart);

		if (
			firstArgumentLine.LineNumber != openLine.LineNumber
			|| lastArgumentLine.LineNumber == openLine.LineNumber
			|| closeLine.LineNumber != lastArgumentLine.LineNumber
		) {
			return false;
		}

		var actualPrefix = text.ToString(TextSpan.FromBounds(closeLine.Start, argumentList.CloseParenToken.SpanStart));

		if (actualPrefix.All(static character => character is ' ' or '\t')) {
			return false;
		}

		var receiverLine = text.Lines.GetLineFromPosition(memberAccess.Expression.SpanStart);
		var closeIndent = text.GetLineIndentation(receiverLine);
		var itemIndent = text.GetLineIndentation(openLine);
		var builder = new StringBuilder();
		builder.Append(lineBreak);
		builder.Append(itemIndent);
		builder.Append(argument.ToString());
		builder.Append(lineBreak);
		builder.Append(closeIndent);

		var changes = new List<TextChange> {
			new(
				TextSpan.FromBounds(memberAccess.Expression.Span.End, memberAccess.OperatorToken.SpanStart),
				string.Empty
			),
			new(
				TextSpan.FromBounds(argumentList.OpenParenToken.Span.End, argumentList.CloseParenToken.SpanStart),
				builder.ToString()
			),
		};

		fixedText = text.WithChanges(changes.OrderBy(static change => change.Span.Start));
		return true;
	}

	private static bool TryExpandSplitList(
		SourceText text,
		SyntaxToken closeParen,
		string expectedIndent,
		string lineBreak,
		out SourceText fixedText
	) {
		fixedText = text;

		if (closeParen.Parent is ArgumentListSyntax argumentList) {
			return TryExpandSeparatedList(
				text,
				argumentList.OpenParenToken,
				argumentList.CloseParenToken,
				argumentList.Arguments.Select(static argument => (SyntaxNode)argument).ToArray(),
				argumentList.Arguments.GetSeparators().ToArray(),
				expectedIndent,
				lineBreak,
				continuationToken: default,
				fixedText: out fixedText
			);
		}

		if (closeParen.Parent is ParameterListSyntax parameterList) {
			parameterList.TryGetContinuationToken(out var continuationToken);

			return TryExpandSeparatedList(
				text,
				parameterList.OpenParenToken,
				parameterList.CloseParenToken,
				parameterList.Parameters.Select(static parameter => (SyntaxNode)parameter).ToArray(),
				parameterList.Parameters.GetSeparators().ToArray(),
				expectedIndent,
				lineBreak,
				continuationToken,
				out fixedText
			);
		}

		return false;
	}

	private static bool TryExpandSeparatedList(
		SourceText text,
		SyntaxToken openParen,
		SyntaxToken closeParen,
		SyntaxNode[] items,
		SyntaxToken[] separators,
		string expectedIndent,
		string lineBreak,
		SyntaxToken continuationToken,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			items.Length <= 1
			|| openParen.IsMissing
			|| closeParen.IsMissing
			|| items.Any(static item => item.IsMissing)
		) {
			return false;
		}

		var openLineNumber = text.Lines.GetLineFromPosition(openParen.SpanStart).LineNumber;
		var itemLineNumbers = items
			.Select(item => text.Lines.GetLineFromPosition(item.GetFirstToken().SpanStart).LineNumber)
			.ToArray();
		var hasItemOnOpeningLine = itemLineNumbers.Any(lineNumber => lineNumber == openLineNumber);
		var hasItemAfterOpeningLine = itemLineNumbers.Any(lineNumber => lineNumber != openLineNumber);
		var hasContinuationAfterCloseParen = HasContinuationAfterCloseParen(text, closeParen, continuationToken);

		if (
			!hasItemOnOpeningLine
			|| (!hasItemAfterOpeningLine && !hasContinuationAfterCloseParen)
		) {
			return false;
		}

		var itemIndent = GetContinuationIndent(text, openParen, items, continuationToken, expectedIndent);
		var hasTrailingSeparator = separators.Length >= items.Length;
		var builder = new StringBuilder();

		for (var index = 0; index < items.Length; index++) {
			builder.Append(lineBreak);
			builder.Append(itemIndent);
			builder.Append(items[index].ToString());

			if (index < items.Length - 1 || hasTrailingSeparator) {
				builder.Append(',');
			}
		}

		builder.Append(lineBreak);
		builder.Append(expectedIndent);

		var changes = new List<TextChange> {
			new(TextSpan.FromBounds(openParen.Span.End, closeParen.SpanStart), builder.ToString()),
		};

		if (hasContinuationAfterCloseParen) {
			var gapSpan = TextSpan.FromBounds(closeParen.Span.End, continuationToken.SpanStart);
			changes.Add(new TextChange(gapSpan, " "));
		}

		fixedText = text.WithChanges(changes.OrderBy(static change => change.Span.Start));
		return true;
	}

	private static string GetContinuationIndent(
		SourceText text,
		SyntaxToken openParen,
		SyntaxNode[] items,
		SyntaxToken continuationToken,
		string expectedIndent
	) {
		var openLineNumber = text.Lines.GetLineFromPosition(openParen.SpanStart).LineNumber;

		var expectedContinuationIndent = GetExpectedContinuationIndent(expectedIndent);

		foreach (var item in items) {
			var itemLine = text.Lines.GetLineFromPosition(item.GetFirstToken().SpanStart);

			if (itemLine.LineNumber != openLineNumber) {
				var itemIndent = text.GetLineIndentation(itemLine);
				return itemIndent == expectedContinuationIndent ? itemIndent : expectedContinuationIndent;
			}
		}

		if (!continuationToken.IsDefaultOrMissing()) {
			var continuationLine = text.Lines.GetLineFromPosition(continuationToken.SpanStart);
			var continuationIndent = text.GetLineIndentation(continuationLine);

			if (continuationIndent.Length > expectedIndent.Length) {
				return continuationIndent;
			}
		}

		return expectedContinuationIndent;
	}

	private static bool TryFixHangingListItemIndentation(
		SourceText text,
		SyntaxToken closeParen,
		string expectedIndent,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			closeParen.Parent is not ArgumentListSyntax argumentList
			|| argumentList.OpenParenToken.IsMissing
			|| argumentList.CloseParenToken.IsMissing
			|| argumentList.Arguments.Count == 0
		) {
			return false;
		}

		var openLine = text.Lines.GetLineFromPosition(argumentList.OpenParenToken.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(argumentList.CloseParenToken.SpanStart);

		if (openLine.LineNumber == closeLine.LineNumber) {
			return false;
		}

		var actualClosePrefix = text.ToString(TextSpan.FromBounds(closeLine.Start, argumentList.CloseParenToken.SpanStart));

		if (actualClosePrefix != expectedIndent) {
			return false;
		}

		var itemIndent = GetExpectedContinuationIndent(expectedIndent);
		var changes = new List<TextChange>();

		foreach (var argument in argumentList.Arguments) {
			if (argument.Expression is LiteralExpressionSyntax literalExpression && RawStringLiteralInfo.TryCreate(literalExpression.Token, out _)) {
				return false;
			}

			var firstToken = argument.GetFirstToken();

			if (firstToken.IsMissing) {
				return false;
			}

			var itemLine = text.Lines.GetLineFromPosition(firstToken.SpanStart);

			if (itemLine.LineNumber == openLine.LineNumber) {
				return false;
			}

			var itemPrefixSpan = TextSpan.FromBounds(itemLine.Start, firstToken.SpanStart);
			var itemPrefix = text.ToString(itemPrefixSpan);

			if (!itemPrefix.All(static character => character is ' ' or '\t')) {
				return false;
			}

			if (itemPrefix != itemIndent) {
				changes.Add(new TextChange(itemPrefixSpan, itemIndent));
			}
		}

		if (changes.Count == 0) {
			return false;
		}

		fixedText = text.WithChanges(changes.OrderBy(static change => change.Span.Start));
		return true;
	}

	private static string GetExpectedContinuationIndent(string expectedIndent) {
		return expectedIndent + IndentationStyle.GetIndentUnit(expectedIndent);
	}

	private static bool HasContinuationAfterCloseParen(
		SourceText text,
		SyntaxToken closeParen,
		SyntaxToken continuationToken
	) {
		if (continuationToken.IsDefaultOrMissing()) {
			return false;
		}

		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var continuationLine = text.Lines.GetLineFromPosition(continuationToken.SpanStart);

		if (closeLine.LineNumber == continuationLine.LineNumber) {
			return false;
		}

		var gapSpan = TextSpan.FromBounds(closeParen.Span.End, continuationToken.SpanStart);
		return text.IsWhiteSpace(gapSpan);
	}

	private static bool TryFixExpressionBodyArrowLine(
		SourceText text,
		SyntaxToken closeParen,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			closeParen.Parent is not ParameterListSyntax parameterList
			|| !parameterList.TryGetExpressionBody(out var expressionBody)
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

		if (!text.IsWhiteSpace(gapSpan)) {
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
			|| !parameterList.TryGetBaseList(out var baseList)
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

		if (!text.IsWhiteSpace(gapSpan)) {
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
			!RawStringLiteralIndentation.TryGetClosingDelimiter(
				actualPrefix,
				out var currentRawStringIndent,
				out var rawStringDelimiter
			)
		) {
			return false;
		}

		var rawStringToken = closeParen.GetPreviousToken();

		if (!RawStringLiteralInfo.TryCreate(rawStringToken, out var rawString) || rawString.Delimiter != rawStringDelimiter) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawString.OpeningDelimiterStart);

		if (openingLine.LineNumber == closeLine.LineNumber) {
			return false;
		}

		var expectedRawStringIndent = text.GetLineIndentation(openingLine);
		var changes = new List<TextChange> {
			new(
				actualPrefixSpan,
				expectedRawStringIndent + rawStringDelimiter + lineBreak + expectedIndent
			),
		};

		if (
			expectedRawStringIndent != currentRawStringIndent
			&& !RawStringLiteralIndentation.TryAddContentIndentChanges(
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

		if (!RawStringLiteralInfo.TryCreate(rawStringToken, out var rawString)) {
			return false;
		}

		var closingDelimiterStart = rawString.ClosingDelimiterStart;

		if (closingDelimiterStart < rawStringToken.SpanStart) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawString.OpeningDelimiterStart);
		var rawStringClosingLine = text.Lines.GetLineFromPosition(closingDelimiterStart);
		var closeParenLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);

		if (
			openingLine.LineNumber == rawStringClosingLine.LineNumber
			|| rawStringClosingLine.LineNumber >= closeParenLine.LineNumber
		) {
			return false;
		}

		var expectedRawStringIndent = text.GetLineIndentation(openingLine);
		var currentRawStringIndent = text.GetLineIndentation(rawStringClosingLine);
		var changes = new List<TextChange>();

		if (expectedRawStringIndent != currentRawStringIndent) {
			if (
				!RawStringLiteralIndentation.TryAddContentIndentChanges(
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

		if (!RawStringLiteralInfo.TryCreate(rawStringToken, out var rawString)) {
			return false;
		}

		var closingDelimiterStart = rawString.ClosingDelimiterStart;

		if (closingDelimiterStart < rawStringToken.SpanStart) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawString.OpeningDelimiterStart);
		var closingLine = text.Lines.GetLineFromPosition(closingDelimiterStart);

		if (openingLine.LineNumber == closingLine.LineNumber) {
			return false;
		}

		var expectedRawStringIndent = text.GetLineIndentation(openingLine);
		var currentRawStringIndent = text.GetLineIndentation(closingLine);

		if (expectedRawStringIndent == currentRawStringIndent) {
			return false;
		}

		var changes = new List<TextChange>();

		if (
			!RawStringLiteralIndentation.TryAddContentIndentChanges(
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

	private static int GetMaxLineLength(
		Document document,
		SyntaxTree tree
	) {
		var options = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);

		if (options.TryGetPositiveInt32("max_line_length", out var maxLineLength)) {
			return maxLineLength;
		}

		return DefaultMaxLineLength;
	}

	private static bool CanMoveInvocationToReceiverLine(
		SourceText text,
		MemberAccessExpressionSyntax memberAccess,
		SyntaxToken openParen,
		int maxLineLength
	) {
		var receiverLine = text.Lines.GetLineFromPosition(memberAccess.Expression.SpanStart);
		var dotLine = text.Lines.GetLineFromPosition(memberAccess.OperatorToken.SpanStart);

		if (receiverLine.LineNumber == dotLine.LineNumber) {
			return false;
		}

		var gapSpan = TextSpan.FromBounds(memberAccess.Expression.Span.End, memberAccess.OperatorToken.SpanStart);

		if (!text.IsWhiteSpace(gapSpan)) {
			return false;
		}

		var receiverText = text.ToString(TextSpan.FromBounds(receiverLine.Start, memberAccess.Expression.Span.End));
		var invocationText = text.ToString(TextSpan.FromBounds(memberAccess.OperatorToken.SpanStart, openParen.Span.End));

		return receiverText.Length + invocationText.Length <= maxLineLength;
	}

}
