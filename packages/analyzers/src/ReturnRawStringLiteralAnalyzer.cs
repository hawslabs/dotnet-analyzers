using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnRawStringLiteralAnalyzer : DiagnosticAnalyzer {
	public const string DiagnosticId = DiagnosticIds.ReturnRawStringLiteral;

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Format multiline raw string literal indentation",
		messageFormat: "Multiline raw string literal should have consistently indented delimiters and content",
		category: DiagnosticCategories.Style,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description:
			"Multiline raw string literal content and closing delimiters should be indented "
			+ "with the opening delimiter. When returned directly, the opening delimiter "
			+ "should start on the line after return and be indented one level deeper."
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context) {
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeRawStringExpression, SyntaxKind.StringLiteralExpression);
		context.RegisterSyntaxNodeAction(AnalyzeRawStringExpression, SyntaxKind.InterpolatedStringExpression);
	}

	private static void AnalyzeRawStringExpression(SyntaxNodeAnalysisContext context) {
		var node = (ExpressionSyntax)context.Node;

		if (!RawStringLiteralInfo.TryCreate(node, out var rawString)) {
			return;
		}

		var tree = node.SyntaxTree;

		if (tree is null) {
			return;
		}

		var text = tree.GetText(context.CancellationToken);
		var openingLine = text.Lines.GetLineFromPosition(rawString.OpeningDelimiterStart);
		var closingLine = text.Lines.GetLineFromPosition(rawString.ClosingDelimiterStart);

		if (
			!TryGetExpectedRawStringIndent(
				node,
				text,
				rawString,
				openingLine,
				out var expectedRawStringIndent,
				out var expectedOpeningLineNumber
			)
		) {
			return;
		}

		var openingPrefix = text.ToString(TextSpan.FromBounds(openingLine.Start, rawString.OpeningDelimiterStart));
		var closingPrefix = text.ToString(TextSpan.FromBounds(closingLine.Start, rawString.ClosingDelimiterStart));

		if (
			openingLine.LineNumber == expectedOpeningLineNumber
			&& openingPrefix == expectedRawStringIndent
			&& closingPrefix == expectedRawStringIndent
		) {
			return;
		}

		context.ReportDiagnostic(
			Diagnostic.Create(
				Rule,
				Location.Create(
					tree,
					new TextSpan(rawString.OpeningDelimiterStart, rawString.OpeningDelimiterLength)
				)
			)
		);
	}

	private static bool TryGetExpectedRawStringIndent(
		ExpressionSyntax expression,
		SourceText text,
		RawStringLiteralInfo rawString,
		TextLine openingLine,
		out string expectedRawStringIndent,
		out int expectedOpeningLineNumber
	) {
		var openingPrefix = text.ToString(TextSpan.FromBounds(openingLine.Start, rawString.OpeningDelimiterStart));
		expectedRawStringIndent = openingPrefix;
		expectedOpeningLineNumber = openingLine.LineNumber;

		if (expression.Parent is not ReturnStatementSyntax returnStatement) {
			return true;
		}

		if (returnStatement.Expression != expression) {
			return true;
		}

		if (returnStatement.ReturnKeyword.IsMissing) {
			return false;
		}

		var betweenReturnAndExpression = text.ToString(
			TextSpan.FromBounds(returnStatement.ReturnKeyword.Span.End, rawString.OpeningDelimiterStart)
		);

		if (!betweenReturnAndExpression.All(static character => character is ' ' or '\t' or '\r' or '\n')) {
			return false;
		}

		var returnLine = text.Lines.GetLineFromPosition(returnStatement.ReturnKeyword.SpanStart);
		var returnIndent = text.GetLineIndentation(returnLine);
		expectedRawStringIndent = returnIndent + IndentationStyle.GetIndentUnit(returnIndent);
		expectedOpeningLineNumber = returnLine.LineNumber + 1;

		return true;
	}
}
