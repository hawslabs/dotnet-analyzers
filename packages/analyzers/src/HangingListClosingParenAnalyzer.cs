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
	private const int DefaultMaxLineLength = 110;

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
			+ "level as the line containing the opening parenthesis."
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context) {
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);
		context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
		context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
	}

	private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context) {
		var node = (ArgumentListSyntax)context.Node;

		if (node.Arguments.Count == 0) {
			return;
		}

		var reportedClosingParen = AnalyzeHangingList(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Arguments[0].GetFirstToken()
		) || AnalyzeSplitListItems(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Arguments.Select(static argument => argument.GetFirstToken()).ToArray()
		) || AnalyzeShortFirstCallWithMultilineArgument(
			context,
			node
		);

		if (!reportedClosingParen) {
			AnalyzeRawStringLiteralArguments(context, node);
		}
	}

	private static bool AnalyzeShortFirstCallWithMultilineArgument(
		SyntaxNodeAnalysisContext context,
		ArgumentListSyntax node
	) {
		if (
			node.Arguments.Count != 1
			|| node.OpenParenToken.IsMissing
			|| node.CloseParenToken.IsMissing
			|| !TryGetFirstInvocationMemberAccess(node, out var memberAccess)
		) {
			return false;
		}

		var argument = node.Arguments[0];
		var firstArgumentToken = argument.GetFirstToken();
		var lastArgumentToken = argument.GetLastToken();

		if (firstArgumentToken.IsMissing || lastArgumentToken.IsMissing) {
			return false;
		}

		var tree = node.SyntaxTree;

		if (tree is null) {
			return false;
		}

		var text = tree.GetText(context.CancellationToken);
		var openLine = text.Lines.GetLineFromPosition(node.OpenParenToken.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(node.CloseParenToken.SpanStart);
		var firstArgumentLine = text.Lines.GetLineFromPosition(firstArgumentToken.SpanStart);
		var lastArgumentLine = text.Lines.GetLineFromPosition(lastArgumentToken.SpanStart);

		if (
			firstArgumentLine.LineNumber != openLine.LineNumber
			|| lastArgumentLine.LineNumber == openLine.LineNumber
			|| closeLine.LineNumber != lastArgumentLine.LineNumber
		) {
			return false;
		}

		var actualPrefix = text.ToString(TextSpan.FromBounds(closeLine.Start, node.CloseParenToken.SpanStart));

		if (actualPrefix.All(static character => character is ' ' or '\t')) {
			return false;
		}

		var maxLineLength = GetMaxLineLength(context, tree);

		if (!CanMoveInvocationToReceiverLine(text, memberAccess, node.OpenParenToken, maxLineLength)) {
			return false;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, node.CloseParenToken.GetLocation()));
		return true;
	}

	private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context) {
		var node = (ParameterListSyntax)context.Node;

		if (node.Parameters.Count == 0) {
			return;
		}

		var reportedClosingParen = AnalyzeHangingList(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Parameters[0].GetFirstToken()
		) || AnalyzeSplitListItems(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Parameters.Select(static parameter => parameter.GetFirstToken()).ToArray()
		);

		if (!reportedClosingParen) {
			AnalyzeExpressionBodiedParameterList(context, node);
			AnalyzeBaseListParameterList(context, node);
		}
	}

	private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context) {
		var node = (WhileStatementSyntax)context.Node;

		AnalyzeHangingList(
			context,
			node.OpenParenToken,
			node.CloseParenToken,
			node.Condition.GetFirstToken()
		);
	}

	private static bool AnalyzeHangingList(
		SyntaxNodeAnalysisContext context,
		SyntaxToken openParen,
		SyntaxToken closeParen,
		SyntaxToken firstItemToken
	) {
		if (openParen.IsMissing || closeParen.IsMissing || firstItemToken.IsMissing) {
			return false;
		}

		var tree = closeParen.SyntaxTree;

		if (tree is null) {
			return false;
		}

		var text = tree.GetText(context.CancellationToken);

		var openLine = text.Lines.GetLineFromPosition(openParen.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var firstItemLine = text.Lines.GetLineFromPosition(firstItemToken.SpanStart);

		if (openLine.LineNumber == closeLine.LineNumber) {
			return false;
		}

		if (firstItemLine.LineNumber == openLine.LineNumber) {
			return false;
		}

		var expectedIndent = GetLineIndentation(text, openLine);
		var actualPrefix = text.ToString(TextSpan.FromBounds(closeLine.Start, closeParen.SpanStart));

		if (actualPrefix == expectedIndent) {
			return false;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, closeParen.GetLocation()));
		return true;
	}

	private static bool AnalyzeSplitListItems(
		SyntaxNodeAnalysisContext context,
		SyntaxToken openParen,
		SyntaxToken closeParen,
		SyntaxToken[] itemTokens
	) {
		if (
			itemTokens.Length <= 1
			|| openParen.IsMissing
			|| closeParen.IsMissing
			|| itemTokens.Any(static token => token.IsMissing)
		) {
			return false;
		}

		var tree = closeParen.SyntaxTree;

		if (tree is null) {
			return false;
		}

		var text = tree.GetText(context.CancellationToken);
		var openLineNumber = text.Lines.GetLineFromPosition(openParen.SpanStart).LineNumber;
		var itemLineNumbers = itemTokens
			.Select(token => text.Lines.GetLineFromPosition(token.SpanStart).LineNumber)
			.ToArray();

		if (
			!itemLineNumbers.Any(lineNumber => lineNumber == openLineNumber)
			|| !itemLineNumbers.Any(lineNumber => lineNumber != openLineNumber)
		) {
			return false;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, closeParen.GetLocation()));
		return true;
	}

	private static void AnalyzeRawStringLiteralArguments(
		SyntaxNodeAnalysisContext context,
		ArgumentListSyntax node
	) {
		if (node.OpenParenToken.IsMissing || node.CloseParenToken.IsMissing || node.Arguments.Count == 0) {
			return;
		}

		var firstItemToken = node.Arguments[0].GetFirstToken();

		if (firstItemToken.IsMissing) {
			return;
		}

		var tree = node.SyntaxTree;

		if (tree is null) {
			return;
		}

		var text = tree.GetText(context.CancellationToken);
		var openLine = text.Lines.GetLineFromPosition(node.OpenParenToken.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(node.CloseParenToken.SpanStart);
		var firstItemLine = text.Lines.GetLineFromPosition(firstItemToken.SpanStart);

		if (openLine.LineNumber == closeLine.LineNumber) {
			return;
		}

		if (firstItemLine.LineNumber == openLine.LineNumber) {
			return;
		}

		var expectedCloseIndent = GetLineIndentation(text, openLine);
		var actualClosePrefix = text.ToString(TextSpan.FromBounds(closeLine.Start, node.CloseParenToken.SpanStart));

		if (actualClosePrefix != expectedCloseIndent) {
			return;
		}

		foreach (var argument in node.Arguments) {
			if (argument.Expression is not LiteralExpressionSyntax literalExpression) {
				continue;
			}

			AnalyzeRawStringLiteralArgument(context, text, literalExpression.Token);
		}
	}

	private static void AnalyzeExpressionBodiedParameterList(
		SyntaxNodeAnalysisContext context,
		ParameterListSyntax node
	) {
		if (
			node.OpenParenToken.IsMissing
			|| node.CloseParenToken.IsMissing
			|| node.Parameters.Count == 0
			|| !TryGetExpressionBody(node, out var expressionBody)
			|| expressionBody.ArrowToken.IsMissing
		) {
			return;
		}

		var firstItemToken = node.Parameters[0].GetFirstToken();

		if (firstItemToken.IsMissing) {
			return;
		}

		var tree = node.SyntaxTree;

		if (tree is null) {
			return;
		}

		var text = tree.GetText(context.CancellationToken);
		var openLine = text.Lines.GetLineFromPosition(node.OpenParenToken.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(node.CloseParenToken.SpanStart);
		var firstItemLine = text.Lines.GetLineFromPosition(firstItemToken.SpanStart);
		var arrowLine = text.Lines.GetLineFromPosition(expressionBody.ArrowToken.SpanStart);

		if (openLine.LineNumber == closeLine.LineNumber) {
			return;
		}

		if (firstItemLine.LineNumber == openLine.LineNumber) {
			return;
		}

		if (closeLine.LineNumber == arrowLine.LineNumber) {
			return;
		}

		var gapSpan = TextSpan.FromBounds(node.CloseParenToken.Span.End, expressionBody.ArrowToken.SpanStart);

		if (!text.ToString(gapSpan).All(static character => char.IsWhiteSpace(character))) {
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, node.CloseParenToken.GetLocation()));
	}

	private static void AnalyzeBaseListParameterList(
		SyntaxNodeAnalysisContext context,
		ParameterListSyntax node
	) {
		if (
			node.OpenParenToken.IsMissing
			|| node.CloseParenToken.IsMissing
			|| node.Parameters.Count == 0
			|| !TryGetBaseList(node, out var baseList)
			|| baseList.ColonToken.IsMissing
		) {
			return;
		}

		var firstItemToken = node.Parameters[0].GetFirstToken();

		if (firstItemToken.IsMissing) {
			return;
		}

		var tree = node.SyntaxTree;

		if (tree is null) {
			return;
		}

		var text = tree.GetText(context.CancellationToken);
		var openLine = text.Lines.GetLineFromPosition(node.OpenParenToken.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(node.CloseParenToken.SpanStart);
		var firstItemLine = text.Lines.GetLineFromPosition(firstItemToken.SpanStart);
		var colonLine = text.Lines.GetLineFromPosition(baseList.ColonToken.SpanStart);

		if (
			openLine.LineNumber == closeLine.LineNumber
			&& (
				node.Parameters.Count <= 1
				|| colonLine.LineNumber == closeLine.LineNumber
			)
		) {
			return;
		}

		if (
			firstItemLine.LineNumber == openLine.LineNumber
			&& openLine.LineNumber != closeLine.LineNumber
		) {
			return;
		}

		if (closeLine.LineNumber == colonLine.LineNumber) {
			return;
		}

		var gapSpan = TextSpan.FromBounds(node.CloseParenToken.Span.End, baseList.ColonToken.SpanStart);

		if (!text.ToString(gapSpan).All(static character => char.IsWhiteSpace(character))) {
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, node.CloseParenToken.GetLocation()));
	}

	private static void AnalyzeRawStringLiteralArgument(
		SyntaxNodeAnalysisContext context,
		SourceText text,
		SyntaxToken token
	) {
		if (!TryGetMultilineRawStringDelimiter(token, out var delimiter)) {
			return;
		}

		var closingDelimiterStart = token.Span.End - delimiter.Length;

		if (closingDelimiterStart < token.SpanStart) {
			return;
		}

		var openingLine = text.Lines.GetLineFromPosition(token.SpanStart);
		var closingLine = text.Lines.GetLineFromPosition(closingDelimiterStart);

		if (openingLine.LineNumber == closingLine.LineNumber) {
			return;
		}

		var expectedIndent = GetLineIndentation(text, openingLine);
		var actualIndent = GetLineIndentation(text, closingLine);

		if (actualIndent == expectedIndent) {
			return;
		}

		context.ReportDiagnostic(
			Diagnostic.Create(
				Rule,
				Location.Create(
					context.Node.SyntaxTree,
					new TextSpan(closingDelimiterStart, delimiter.Length)
				)
			)
		);
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

	private static int GetMaxLineLength(
		SyntaxNodeAnalysisContext context,
		SyntaxTree tree
	) {
		var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(tree);

		if (
			options.TryGetValue("max_line_length", out var value)
			&& int.TryParse(value, out var maxLineLength)
			&& maxLineLength > 0
		) {
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

		if (!text.ToString(gapSpan).All(static character => char.IsWhiteSpace(character))) {
			return false;
		}

		var receiverText = text.ToString(TextSpan.FromBounds(receiverLine.Start, memberAccess.Expression.Span.End));
		var invocationText = text.ToString(TextSpan.FromBounds(memberAccess.OperatorToken.SpanStart, openParen.Span.End));

		return receiverText.Length + invocationText.Length <= maxLineLength;
	}

	private static bool TryGetFirstInvocationMemberAccess(
		ArgumentListSyntax node,
		out MemberAccessExpressionSyntax memberAccess
	) {
		memberAccess = null!;

		if (node.Parent is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax value }) {
			return false;
		}

		var memberName = value.Name switch {
			GenericNameSyntax genericName => genericName.Identifier.ValueText,
			IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
			_ => string.Empty,
		};

		if (!memberName.StartsWith("First", StringComparison.Ordinal)) {
			return false;
		}

		memberAccess = value;
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

		return tokenText.EndsWith(delimiter, StringComparison.Ordinal)
			&& tokenText.Any(static character => character is '\r' or '\n');
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
