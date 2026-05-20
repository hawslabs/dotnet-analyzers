using HawsLabs.Analyzers.Tests.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace HawsLabs.Analyzers.Tests.HangingListClosingParen;

public abstract class HangingListClosingParenTestFixture
	: CodeFixTestFixture<
		HawsLabs.Analyzers.HangingListClosingParenAnalyzer,
		HawsLabs.Analyzers.HangingListClosingParenCodeFixProvider> {
	protected static string InMethodBody(string body, string? supportingMembers = null) {
		return CSharpSource.InMethodBody(body, supportingMembers);
	}

	protected static string InType(string members) {
		return CSharpSource.InType(members);
	}

	protected static DiagnosticResult Diagnostic() {
		return new DiagnosticResult(
			HangingListClosingParenAnalyzer.DiagnosticId,
			DiagnosticSeverity.Warning
		);
	}

	protected static Task VerifyCodeFixWithoutMarkupAsync(
		string source,
		string fixedSource,
		params DiagnosticResult[] expectedDiagnostics
	) {
		return VerifyCodeFixAsync(source, fixedSource, MarkupMode.None, expectedDiagnostics);
	}
}
