using Microsoft.CodeAnalysis;

namespace HawsLabs.Analyzers;

internal static class SyntaxTokenExtensions {
	public static bool IsDefaultOrMissing(this SyntaxToken token) => token.RawKind == 0 || token.IsMissing;
}
