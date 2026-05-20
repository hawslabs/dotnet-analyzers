using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnRawStringLiteralAnalyzer : DiagnosticAnalyzer {
	public const string DiagnosticId = "HA0002";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Format multiline raw string literal indentation",
		messageFormat: "Multiline raw string literal should have consistently indented delimiters and content",
		category: "Formatting",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description:
			"Multiline raw string literal content and closing delimiters should be indented "
			+ "with the opening delimiter. When returned directly, the opening delimiter "
			+ "should start on the line after return and be indented one level deeper."
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context) {
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeRawStringExpression, SyntaxKind.StringLiteralExpression);
		context.RegisterSyntaxNodeAction(AnalyzeRawStringExpression, SyntaxKind.InterpolatedStringExpression);
	}

	private static void AnalyzeRawStringExpression(SyntaxNodeAnalysisContext context) {
		var node = (ExpressionSyntax)context.Node;

		if (!TryGetMultilineRawStringExpression(node, out var rawString)) {
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
		RawStringExpression rawString,
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
		var returnIndent = GetLineIndentation(text, returnLine);
		expectedRawStringIndent = returnIndent + GetIndentUnit(returnIndent);
		expectedOpeningLineNumber = returnLine.LineNumber + 1;

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
			dollarCount + quoteCount,
			expression.Span.End - quoteCount
		);

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

	private readonly struct RawStringExpression {
		public RawStringExpression(
			int openingDelimiterStart,
			int openingDelimiterLength,
			int closingDelimiterStart
		) {
			OpeningDelimiterStart = openingDelimiterStart;
			OpeningDelimiterLength = openingDelimiterLength;
			ClosingDelimiterStart = closingDelimiterStart;
		}

		public int OpeningDelimiterStart { get; }

		public int OpeningDelimiterLength { get; }

		public int ClosingDelimiterStart { get; }
	}
}
