using Xunit;

namespace HawsLabs.Analyzers.Tests.HangingListClosingParen;

public sealed class HangingListClosingParenCodeFixTests : HangingListClosingParenTestFixture {
	[Fact]
	public Task FormatsArgumentListWrappingRawStringLiteral() {
		return VerifyCodeFixAsync(
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
			CallTarget(
				1,
				2);
			"""[|)|]);
				}
			}
			"""",
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
						CallTarget(
							1,
							2);
						"""
					));
				}
			}
			""""
		);
	}

	[Fact]
	public Task FormatsArgumentListWrappingRawStringLiteralWhenClosingParenAlreadyHasDedicatedLine() {
		return VerifyCodeFixAsync(
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
			CallTarget(
				1,
				2
			);
			"""
			[|)|]);
				}
			}
			"""",
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
						CallTarget(
							1,
							2
						);
						"""
					));
				}
			}
			""""
		);
	}

	[Fact]
	public Task FormatsGroupedTrailingParensInsideRawStringArgument() {
		return VerifyCodeFixAsync(
			""""
			using System;
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body, string supportingMembers) {
					return body + supportingMembers;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(
						InMethodBody(
							"""
			CallWithFactory(() => Create(
				1,
				2
			));
			[|"""|],
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
			"""",
			""""
			using System;
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body, string supportingMembers) {
					return body + supportingMembers;
				}

				public static Task Test() {
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
			""""
		);
	}

	[Fact]
	public Task FormatsArgumentListWrappingSingleLineRawStringLiteral() {
		return VerifyCodeFixAsync(
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
			CallTarget(1, 2);
			"""[|)|]);
				}
			}
			"""",
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
						CallTarget(1, 2);
						"""
					));
				}
			}
			""""
		);
	}

	[Fact]
	public Task FormatsParameterListWrappingRawStringLiteral() {
		return VerifyCodeFixAsync(
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InType(string members) {
					return members;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InType(
						"""
			private static void Test(
				int first,
				int second) {
			}
			"""[|)|]);
				}
			}
			"""",
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InType(string members) {
					return members;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InType(
						"""
						private static void Test(
							int first,
							int second) {
						}
						"""
					));
				}
			}
			""""
		);
	}

	[Fact]
	public Task FormatsWhileConditionWrappingRawStringLiteralWithDiagnosticMarkup() {
		return VerifyCodeFixWithoutMarkupAsync(
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
					return VerifyAnalyzerAsync(InMethodBody(
						"""
			var lineText = " ";
			var index = 0;

			while (
				index < lineText.Length
				&& (lineText[index] == ' ' || lineText[index] == '\t')[|)|] {
				index++;
			}
			"""));
				}
			}
			"""",
			""""
			using System.Threading.Tasks;

			internal static class TestCode {
				private static Task VerifyAnalyzerAsync(string source) {
					return Task.CompletedTask;
				}

				private static string InMethodBody(string body) {
					return body;
				}

				public static Task Test() {
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
			}
			"""",
			Diagnostic().WithSpan(23, 4, 23, 5)
		);
	}

	[Fact]
	public Task MovesArgumentListClosingParenToItsOwnLine() {
		return VerifyCodeFixAsync(
			InMethodBody(
				"""
				CallTarget(
					1,
					2[|)|];
				"""
			),
			InMethodBody(
				"""
				CallTarget(
					1,
					2
				);
				"""
			)
		);
	}

	[Fact]
	public Task MovesParameterListClosingParenToItsOwnLine() {
		return VerifyCodeFixAsync(
			InType(
				"""
				private static void Test(
					int first,
					int second[|)|] {
				}
				"""
			),
			InType(
				"""
				private static void Test(
					int first,
					int second
				) {
				}
				"""
			)
		);
	}

	[Fact]
	public Task MovesExpressionBodiedMethodArrowToClosingParenLine() {
		return VerifyCodeFixAsync(
			InType(
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
			),
			InType(
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
				) => app.ResourceNotifications
						.WaitForResourceHealthyAsync(resourceName, cancellationToken)
						.WaitAsync(timeout, cancellationToken);
				"""
			)
		);
	}

	[Fact]
	public Task MovesPrimaryConstructorBaseListToClosingParenLine() {
		return VerifyCodeFixAsync(
			PrimaryConstructorBaseListOnNextLine(),
			PrimaryConstructorBaseListOnClosingParenLine()
		);
	}

	[Fact]
	public Task MovesWhileConditionClosingParenToItsOwnLine() {
		return VerifyCodeFixAsync(
			InMethodBody(
				"""
				var lineText = " ";
				var index = 0;

				while (
					index < lineText.Length
					&& (lineText[index] == ' ' || lineText[index] == '\t')[|)|] {
					index++;
				}
				"""
			),
			InMethodBody(
				"""
				var lineText = " ";
				var index = 0;

				while (
					index < lineText.Length
					&& (lineText[index] == ' ' || lineText[index] == '\t')
				) {
					index++;
				}
				"""
			)
		);
	}

	[Fact]
	public Task RealignsExistingClosingParenLine() {
		return VerifyCodeFixAsync(
			InMethodBody(
				"""
				CallTarget(
					1,
					2
						[|)|];
				"""
			),
			InMethodBody(
				"""
				CallTarget(
					1,
					2
				);
				"""
			)
		);
	}
}
