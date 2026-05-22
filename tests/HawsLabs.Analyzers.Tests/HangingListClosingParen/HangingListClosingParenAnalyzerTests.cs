using Xunit;

namespace HawsLabs.Analyzers.Tests.HangingListClosingParen;

public sealed class HangingListClosingParenAnalyzerTests : HangingListClosingParenTestFixture {
	[Fact]
	public Task ReportsDiagnosticForHangingArgumentListWithoutDedicatedClosingParenLine() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(
				1,
				2{|HA9000:)|};
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticForHangingParameterListWithoutDedicatedClosingParenLine() {
		return VerifyAnalyzerAsync(InType(
			"""
			private static void Test(
				int first,
				int second{|HA9000:)|} {
			}
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticWhenLaterArgumentMovesToNewLine() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(1,
				2{|HA9001:)|};
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticForPrimaryConstructorWithBaseListOnNextLineWhenParametersShareOpeningLine() {
		return VerifyAnalyzerAsync(
			"""
			namespace TestCode {
				public sealed class Derived(string name, int count{|HA9002:)|}
					: Base(name) {
				}

				public abstract class Base(string name);
			}
			"""
		);
	}

	[Fact]
	public Task ReportsDiagnosticForExpressionBodiedMethodWithArrowOnNextLine() {
		return VerifyAnalyzerAsync(InType(
			"""
			private static string Format(
				string value,
				int count
			{|HA9002:)|}
				=> value;
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticForExpressionBodiedInvocationWithOverIndentedHangingArguments() {
		return VerifyAnalyzerAsync(ExpressionBodiedInvocationWithOverIndentedHangingArguments());
	}

	[Fact]
	public Task ReportsDiagnosticForPrimaryConstructorWithBaseListOnNextLine() {
		return VerifyAnalyzerAsync(PrimaryConstructorBaseListOnNextLine());
	}

	[Fact]
	public Task ReportsDiagnosticForHangingWhileConditionWithoutDedicatedClosingParenLine() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			var lineText = " ";
			var index = 0;

			while (
				index < lineText.Length
				&& (lineText[index] == ' ' || lineText[index] == '\t'){|HA9000:)|} {
				index++;
			}
			"""
		));
	}

	[Fact]
	public Task DoesNotReportDiagnosticForSingleLineArgumentList() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(1, 2);
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticWhenFirstArgumentStaysOnOpeningLineAndLaterArgumentMovesToNewLine() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(1,
				2{|HA9001:)|};
			"""
		));
	}

	[Fact]
	public Task DoesNotReportDiagnosticForCorrectlyFormattedHangingArgumentList() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(
				1,
				2
			);
			"""
		));
	}

	[Fact]
	public Task DoesNotReportDiagnosticForGroupedTrailingParens() {
		return VerifyAnalyzerAsync(
			InMethodBody(
				"""
				CallWithFactory(() => Create(
					1,
					2
				));
				""",
				"""
				private static void CallWithFactory(Func<int> factory) {
				}

				private static int Create(int first, int second) {
					return first + second;
				}
				"""
			)
		);
	}
}
