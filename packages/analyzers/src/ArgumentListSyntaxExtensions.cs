using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HawsLabs.Analyzers;

internal static class ArgumentListSyntaxExtensions {
	public static bool TryGetFirstInvocationMemberAccess(
		this ArgumentListSyntax node,
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
}
