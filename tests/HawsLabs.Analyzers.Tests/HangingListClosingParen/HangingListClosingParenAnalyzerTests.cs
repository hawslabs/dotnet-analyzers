using Xunit;

namespace HawsLabs.Analyzers.Tests.HangingListClosingParen;

public sealed class HangingListClosingParenAnalyzerTests : HangingListClosingParenTestFixture {
	[Fact]
	public Task ReportsDiagnosticForHangingArgumentListWithoutDedicatedClosingParenLine() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(
				1,
				2[|)|];
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticForHangingParameterListWithoutDedicatedClosingParenLine() {
		return VerifyAnalyzerAsync(InType(
			"""
			private static void Test(
				int first,
				int second[|)|] {
			}
			"""
		));
	}

	[Fact]
	public Task ReportsDiagnosticForExpressionBodiedMethodWithArrowOnNextLine() {
		return VerifyAnalyzerAsync(InType(
			"""
			private sealed class DistributedApplication {
				public ResourceNotificationService ResourceNotifications { get; } = new();
			}

			private sealed class ResourceNotificationService {
				public ResourceWaitOperation WaitForResourceHealthyAsync(
					string resourceName,
					System.Threading.CancellationToken cancellationToken
				) => new();
			}

			private sealed class ResourceWaitOperation {
				public System.Threading.Tasks.Task WaitAsync(
					TimeSpan timeout,
					System.Threading.CancellationToken cancellationToken
				) => System.Threading.Tasks.Task.CompletedTask;
			}

			private static System.Threading.Tasks.Task WaitForResourceHealthyAsync(
				this DistributedApplication app,
				string resourceName,
				TimeSpan timeout,
				System.Threading.CancellationToken cancellationToken = default
			[|)|]
				=> app.ResourceNotifications
					.WaitForResourceHealthyAsync(resourceName, cancellationToken)
					.WaitAsync(timeout, cancellationToken);
			"""
		));
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
				&& (lineText[index] == ' ' || lineText[index] == '\t')[|)|] {
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
	public Task DoesNotReportDiagnosticWhenFirstArgumentStaysOnOpeningLine() {
		return VerifyAnalyzerAsync(InMethodBody(
			"""
			CallTarget(1,
				2);
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
