using HawsLabs.Analyzers.Tests.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace HawsLabs.Analyzers.Tests.ReturnRawStringLiteral;

public abstract class ReturnRawStringLiteralTestFixture
	: CodeFixTestFixture<
		HawsLabs.Analyzers.ReturnRawStringLiteralAnalyzer,
		HawsLabs.Analyzers.ReturnRawStringLiteralCodeFixProvider> {
	protected static DiagnosticResult Diagnostic() {
		return new DiagnosticResult(
			ReturnRawStringLiteralAnalyzer.DiagnosticId,
			DiagnosticSeverity.Warning
		);
	}

	protected static Task VerifyAnalyzerWithoutMarkupAsync(
		string source,
		params DiagnosticResult[] expectedDiagnostics
	) {
		return VerifyAnalyzerAsync(source, MarkupMode.None, expectedDiagnostics);
	}

	protected static Task VerifyCodeFixWithoutMarkupAsync(
		string source,
		string fixedSource,
		params DiagnosticResult[] expectedDiagnostics
	) {
		return VerifyCodeFixAsync(source, fixedSource, MarkupMode.None, expectedDiagnostics);
	}
}
