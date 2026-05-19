using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HangingListClosingParenAnalyzer : DiagnosticAnalyzer {
	public const string DiagnosticId = "HA0001";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Put hanging-list closing parenthesis on its own line",
		messageFormat: "Closing parenthesis for a hanging multiline list should be on its own line and aligned with the opening line",
		category: "Formatting",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description:
			"When an argument or parameter list starts on the line after its opening parenthesis, "
			+ "the matching closing parenthesis should be on its own line and indented to the same "
			+ "level as the line containing the opening parenthesis.");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context) {
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);
		context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
	}

	private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context) {
		var node = (ArgumentListSyntax)context.Node;

		if (node.Arguments.Count == 0) {
			return;
		}

		AnalyzeHangingList(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Arguments[0].GetFirstToken());
	}

	private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context) {
		var node = (ParameterListSyntax)context.Node;

		if (node.Parameters.Count == 0) {
			return;
		}

		AnalyzeHangingList(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Parameters[0].GetFirstToken());
	}

	private static void AnalyzeHangingList(
		SyntaxNodeAnalysisContext context,
		SyntaxToken openParen,
		SyntaxToken closeParen,
		SyntaxToken firstItemToken) {
		if (openParen.IsMissing || closeParen.IsMissing || firstItemToken.IsMissing) {
			return;
		}

		var tree = closeParen.SyntaxTree;

		if (tree is null) {
			return;
		}

		var text = tree.GetText(context.CancellationToken);

		var openLine = text.Lines.GetLineFromPosition(openParen.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var firstItemLine = text.Lines.GetLineFromPosition(firstItemToken.SpanStart);

		if (openLine.LineNumber == closeLine.LineNumber) {
			return;
		}

		if (firstItemLine.LineNumber == openLine.LineNumber) {
			return;
		}

		var expectedIndent = GetLineIndentation(text, openLine);
		var actualPrefix = text.ToString(TextSpan.FromBounds(closeLine.Start, closeParen.SpanStart));

		if (actualPrefix == expectedIndent) {
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, closeParen.GetLocation()));
	}

	private static string GetLineIndentation(SourceText text, TextLine line) {
		var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));
		var index = 0;

		while (
			index < lineText.Length
			&& (lineText[index] == ' ' || lineText[index] == '\t')) {
			index++;
		}

		return lineText.Substring(0, index);
	}
}
