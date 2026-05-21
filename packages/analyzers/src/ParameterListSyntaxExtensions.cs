using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HawsLabs.Analyzers;

internal static class ParameterListSyntaxExtensions {
	public static bool TryGetExpressionBody(
		this ParameterListSyntax node,
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

	public static bool TryGetBaseList(this ParameterListSyntax node, out BaseListSyntax baseList) {
		baseList = node.Parent switch {
			TypeDeclarationSyntax { BaseList: { } value } => value,
			_ => null!,
		};

		return baseList is not null;
	}

	public static bool TryGetContinuationToken(this ParameterListSyntax node, out SyntaxToken continuationToken) {
		if (node.TryGetBaseList(out var baseList) && !baseList.ColonToken.IsMissing) {
			continuationToken = baseList.ColonToken;
			return true;
		}

		if (node.TryGetExpressionBody(out var expressionBody) && !expressionBody.ArrowToken.IsMissing) {
			continuationToken = expressionBody.ArrowToken;
			return true;
		}

		continuationToken = default;
		return false;
	}
}
