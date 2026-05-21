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
			"""{|HA9000:)|});
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
			{|HA9000:)|});
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
			{|HA9000:"""|},
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
			"""{|HA9000:)|});
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
			"""{|HA9000:)|});
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
					2{|HA9000:)|};
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
					int second{|HA9000:)|} {
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
	public Task ExpandsArgumentListWhenLaterArgumentMovesToNewLine() {
		return VerifyCodeFixAsync(
			InMethodBody(
				"""
				CallTarget(1,
					2{|HA9001:)|};
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
	public Task MovesShortChainedFirstCallToOpeningLineWhenNestedArgumentListWraps() {
		return VerifyCodeFixAsync(
			"""
			using System;
			using System.Linq;

			internal static class TestCode {
				private static void Test() {
					var messageTypeName = string.Empty;
					var messageType = ServiceBusMessageRegistry.MessageTypes
						.FirstOrDefault(type => string.Equals(
							WolverineMessageNaming.ToMessageTypeName(type),
							messageTypeName,
							StringComparison.Ordinal
						){|HA9003:)|}!;
				}

				private static class ServiceBusMessageRegistry {
					public static Type[] MessageTypes { get; } = Array.Empty<Type>();
				}

				private static class WolverineMessageNaming {
					public static string ToMessageTypeName(Type type) => type.Name;
				}
			}
			""",
			"""
			using System;
			using System.Linq;

			internal static class TestCode {
				private static void Test() {
					var messageTypeName = string.Empty;
					var messageType = ServiceBusMessageRegistry.MessageTypes.FirstOrDefault(
						type => string.Equals(
							WolverineMessageNaming.ToMessageTypeName(type),
							messageTypeName,
							StringComparison.Ordinal
						)
					)!;
				}

				private static class ServiceBusMessageRegistry {
					public static Type[] MessageTypes { get; } = Array.Empty<Type>();
				}

				private static class WolverineMessageNaming {
					public static string ToMessageTypeName(Type type) => type.Name;
				}
			}
			"""
		);
	}

	[Fact]
	public Task ExpandsPrimaryConstructorParameterListWhenBaseListMovesToNewLine() {
		return VerifyCodeFixAsync(
			"""
			namespace System.Runtime.CompilerServices {
				internal static class IsExternalInit {
				}
			}

			namespace TestCode {
				using System;

				public interface IFrontendRoutedMessage {
				}

				public record CloudRouteUnavailable(Guid InstallationId, string MessageType, string Reason{|HA9002:)|}
					: IFrontendRoutedMessage {
				}
			}
			""",
			"""
			namespace System.Runtime.CompilerServices {
				internal static class IsExternalInit {
				}
			}

			namespace TestCode {
				using System;

				public interface IFrontendRoutedMessage {
				}

				public record CloudRouteUnavailable(
					Guid InstallationId,
					string MessageType,
					string Reason
				) : IFrontendRoutedMessage {
				}
			}
			"""
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
				{|HA9002:)|}
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
					&& (lineText[index] == ' ' || lineText[index] == '\t'){|HA9000:)|} {
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
						{|HA9000:)|};
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
