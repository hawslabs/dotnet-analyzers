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
			.FirstOrDefault(static expression => RawStringLiteralInfo.TryCreate(expression, out _));

		if (rawStringExpression is null) {
			return document;
		}

		if (!RawStringLiteralInfo.TryCreate(rawStringExpression, out var rawString)) {
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
			changes.Add(new TextChange(openingIndentSpan, text.GetLineBreak() + expectedRawStringIndent));
		}

		var currentRawStringIndent = text.ToString(TextSpan.FromBounds(closingLine.Start, rawString.ClosingDelimiterStart));

		if (currentRawStringIndent != expectedRawStringIndent) {
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
		RawStringLiteralInfo rawString,
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
		var returnIndent = text.GetLineIndentation(returnLine);
		expectedRawStringIndent = returnIndent + IndentationStyle.GetIndentUnit(returnIndent);

		return true;
	}
}
