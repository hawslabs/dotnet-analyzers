using HawsLabs.Analyzers.Tests.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace HawsLabs.Analyzers.Tests.OneTrueBraceStyle;

public sealed class OneTrueBraceStyleAnalyzerTests {
	[Fact]
	public Task ReportsDiagnosticWhenOneTrueBraceStyleOptionsConflict() {
		return VerifyAnalyzerAsync(
			"""
			root = true

			[*]
			indent_brace_style = 1TBS

			[*.cs]
			csharp_prefer_braces = false
			csharp_new_line_before_open_brace = all
			csharp_new_line_before_else = true
			csharp_new_line_before_catch = true
			csharp_new_line_before_finally = true
			csharp_new_line_before_members_in_object_initializers = true
			csharp_new_line_before_members_in_anonymous_types = true
			csharp_new_line_between_query_expression_clauses = true
			""",
			Diagnostic("1TBS", "csharp_new_line_before_catch", "true", "false"),
			Diagnostic("1TBS", "csharp_new_line_before_else", "true", "false"),
			Diagnostic("1TBS", "csharp_new_line_before_finally", "true", "false"),
			Diagnostic("1TBS", "csharp_new_line_before_members_in_anonymous_types", "true", "false"),
			Diagnostic("1TBS", "csharp_new_line_before_members_in_object_initializers", "true", "false"),
			Diagnostic("1TBS", "csharp_new_line_before_open_brace", "all", "none"),
			Diagnostic("1TBS", "csharp_new_line_between_query_expression_clauses", "true", "false"),
			Diagnostic("1TBS", "csharp_prefer_braces", "false", "true")
		);
	}

	[Fact]
	public Task ReportsDiagnosticForOtbsCaseInsensitive() {
		return VerifyAnalyzerAsync(
			"""
			root = true

			[*]
			indent_brace_style = otbs
			""",
			Diagnostic("otbs", "csharp_new_line_before_catch", "<missing>", "false"),
			Diagnostic("otbs", "csharp_new_line_before_else", "<missing>", "false"),
			Diagnostic("otbs", "csharp_new_line_before_finally", "<missing>", "false"),
			Diagnostic("otbs", "csharp_new_line_before_members_in_anonymous_types", "<missing>", "false"),
			Diagnostic("otbs", "csharp_new_line_before_members_in_object_initializers", "<missing>", "false"),
			Diagnostic("otbs", "csharp_new_line_before_open_brace", "<missing>", "none"),
			Diagnostic("otbs", "csharp_new_line_between_query_expression_clauses", "<missing>", "false"),
			Diagnostic("otbs", "csharp_prefer_braces", "<missing>", "true")
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticWhenOneTrueBraceStyleOptionsMatchWithInlineSeverity() {
		return VerifyAnalyzerAsync(
			"""
			root = true

			[*]
			indent_brace_style = 1tbs

			[*.cs]
			csharp_prefer_braces = true:error
			csharp_new_line_before_open_brace = none
			csharp_new_line_before_else = false
			csharp_new_line_before_catch = false
			csharp_new_line_before_finally = false
			csharp_new_line_before_members_in_object_initializers = false
			csharp_new_line_before_members_in_anonymous_types = false
			csharp_new_line_between_query_expression_clauses = false
			"""
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticWhenPreferBracesSeverityIsSetByIde0011() {
		return VerifyAnalyzerAsync(
			"""
			root = true

			[*.cs]
			indent_brace_style = 1TBS

			csharp_prefer_braces = true
			dotnet_diagnostic.IDE0011.severity = error

			csharp_new_line_before_open_brace = none
			csharp_new_line_before_else = false
			csharp_new_line_before_catch = false
			csharp_new_line_before_finally = false
			csharp_new_line_before_members_in_object_initializers = false
			csharp_new_line_before_members_in_anonymous_types = false
			csharp_new_line_between_query_expression_clauses = false
			"""
		);
	}

	[Fact]
	public Task ReportsDiagnosticWhenPreferBracesSeverityIsNotError() {
		return VerifyAnalyzerAsync(
			"""
			root = true

			[*.cs]
			indent_brace_style = 1TBS

			csharp_prefer_braces = true:warning

			csharp_new_line_before_open_brace = none
			csharp_new_line_before_else = false
			csharp_new_line_before_catch = false
			csharp_new_line_before_finally = false
			csharp_new_line_before_members_in_object_initializers = false
			csharp_new_line_before_members_in_anonymous_types = false
			csharp_new_line_between_query_expression_clauses = false
			""",
			Diagnostic("1TBS", "dotnet_diagnostic.IDE0011.severity", "warning", "error")
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticWhenBraceStyleIsNotOneTrueBraceStyle() {
		return VerifyAnalyzerAsync(
			"""
			root = true

			[*]
			indent_brace_style = allman
			"""
		);
	}

	private static Task VerifyAnalyzerAsync(
		string editorConfig,
		params DiagnosticResult[] expectedDiagnostics
	) {
		var test = new CSharpAnalyzerTest<OneTrueBraceStyleAnalyzer, DefaultVerifier> {
			TestCode = "internal static class TestCode { }",
		};

		test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorConfig));
		test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

		return test.RunAsync();
	}

	private static DiagnosticResult Diagnostic() {
		return new DiagnosticResult(
			OneTrueBraceStyleAnalyzer.DiagnosticId,
			DiagnosticSeverity.Warning
		);
	}

	private static DiagnosticResult Diagnostic(
		string braceStyle,
		string optionName,
		string actualValue,
		string expectedValue
	) {
		return Diagnostic()
			.WithSpan(1, 1, 1, 9)
			.WithArguments(braceStyle, optionName, actualValue, expectedValue);
	}
}