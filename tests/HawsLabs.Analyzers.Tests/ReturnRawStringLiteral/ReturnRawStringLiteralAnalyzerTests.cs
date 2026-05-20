using Xunit;

namespace HawsLabs.Analyzers.Tests.ReturnRawStringLiteral;

public sealed class ReturnRawStringLiteralAnalyzerTests : ReturnRawStringLiteralTestFixture {
	[Fact]
	public Task ReportsDiagnosticForReturnedInterpolatedRawStringOnReturnLine() {
		return VerifyAnalyzerWithoutMarkupAsync(
			""""
			internal static class TestCode {
				public static string InType(string members) {
					return $$"""
			using System;

			internal static class GeneratedCode {
			}
			""";
				}
			}
			"""",
			Diagnostic().WithSpan(3, 10, 3, 15)
		);
	}

	[Fact]
	public Task ReportsDiagnosticForReturnedRawStringWithUnderIndentedClosingDelimiter() {
		return VerifyAnalyzerAsync(
			""""
			internal static class TestCode {
				public static string Create() {
					return
						[|"""|]
			value
			""";
				}
			}
			""""
		);
	}

	[Fact]
	public Task ReportsDiagnosticForInterpolatedRawStringArgumentWithUnderIndentedDelimiter() {
		return VerifyAnalyzerWithoutMarkupAsync(
			""""
			internal static class TestCode {
				public static string InMethodBody(string body) {
					return JoinBlocks(
						$$"""
			private static void Test() {
			{{FormatBlock(body, 1)}}
			}
			"""
					);
				}

				private static string JoinBlocks(params string[] blocks) {
					return string.Concat(blocks);
				}

				private static string FormatBlock(string body, int indentLevel) {
					return body;
				}
			}
			"""",
			Diagnostic().WithSpan(4, 4, 4, 9)
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticForFormattedReturnedInterpolatedRawString() {
		return VerifyAnalyzerWithoutMarkupAsync(
			""""
			internal static class TestCode {
				public static string InType(string members) {
					return
						$$"""
						using System;

						internal static class GeneratedCode {
						}
						""";
				}
			}
			""""
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticForFormattedRawStringArgument() {
		return VerifyAnalyzerWithoutMarkupAsync(
			""""
			internal static class TestCode {
				public static string InMethodBody() {
					return JoinBlocks(
						"""
						private static void Test() {
						}
						"""
					);
				}

				private static string JoinBlocks(params string[] blocks) {
					return string.Concat(blocks);
				}
			}
			""""
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticForSingleLineRawString() {
		return VerifyAnalyzerAsync(
			""""
			internal static class TestCode {
				public static string Create() {
					return """value""";
				}
			}
			""""
		);
	}

	[Fact]
	public Task DoesNotReportDiagnosticForRegularStringLiteral() {
		return VerifyAnalyzerAsync(
			"""
			internal static class TestCode {
				public static string Create() {
					return "value";
				}
			}
			"""
		);
	}
}
