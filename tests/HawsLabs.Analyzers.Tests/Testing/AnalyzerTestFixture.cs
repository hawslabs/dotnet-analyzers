using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace HawsLabs.Analyzers.Tests.Testing;

public abstract class AnalyzerTestFixture<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new() {
	protected static Task VerifyAnalyzerAsync(string source) {
		var test = CreateAnalyzerTest(source);
		return test.RunAsync();
	}

	protected static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expectedDiagnostics) {
		var test = CreateAnalyzerTest(source);
		test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
		return test.RunAsync();
	}

	private static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateAnalyzerTest(string source) {
		return new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> {
			TestCode = source,
		};
	}
}

public abstract class CodeFixTestFixture<TAnalyzer, TCodeFix> : AnalyzerTestFixture<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new() {
	protected static Task VerifyCodeFixAsync(string source, string fixedSource) {
		var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> {
			TestCode = source,
			FixedCode = fixedSource,
		};

		return test.RunAsync();
	}

	protected static Task VerifyCodeFixAsync(
		string source,
		string fixedSource,
		MarkupMode markupMode,
		params DiagnosticResult[] expectedDiagnostics
	) {
		var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> {
			TestCode = source,
			FixedCode = fixedSource,
		};

		test.TestState.MarkupHandling = markupMode;
		test.FixedState.MarkupHandling = markupMode;
		test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

		return test.RunAsync();
	}
}
