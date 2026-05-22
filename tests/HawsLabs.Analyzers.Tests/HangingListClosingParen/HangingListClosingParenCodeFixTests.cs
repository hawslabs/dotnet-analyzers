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
	public Task FormatsExpressionBodiedExtensionMethodForwardingInvocation() {
		return VerifyCodeFixAsync(
			"""
			using System.Threading.Tasks;

			namespace TestCode;

			public sealed class Receiver;

			public static class SampleExtensions {
				public static Task<int> TargetAsync(this Receiver receiver, string first, string second) => Task.FromResult(0);

				public static Task<int> RelayAsync(
					this Receiver receiver,
					string value
				) => receiver.TargetAsync(
						"first",
						value
				{|HA9006:)|};
			}
			""",
			"""
			using System.Threading.Tasks;

			namespace TestCode;

			public sealed class Receiver;

			public static class SampleExtensions {
				public static Task<int> TargetAsync(this Receiver receiver, string first, string second) => Task.FromResult(0);

				public static Task<int> RelayAsync(
					this Receiver receiver,
					string value
				) => receiver.TargetAsync(
					"first",
					value
				);
			}
			"""
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
					var expected = string.Empty;
					var item = Items.Values
						.FirstOrDefault(type => string.Equals(
							Format(type),
							expected,
							StringComparison.Ordinal
						){|HA9003:)|}!;
				}

				private static class Items {
					public static string[] Values { get; } = Array.Empty<string>();
				}

				private static string Format(string value) => value;
			}
			""",
			"""
			using System;
			using System.Linq;

			internal static class TestCode {
				private static void Test() {
					var expected = string.Empty;
					var item = Items.Values.FirstOrDefault(
						type => string.Equals(
							Format(type),
							expected,
							StringComparison.Ordinal
						)
					)!;
				}

				private static class Items {
					public static string[] Values { get; } = Array.Empty<string>();
				}

				private static string Format(string value) => value;
			}
			"""
		);
	}

	[Fact]
	public Task ExpandsPrimaryConstructorParameterListWhenBaseListMovesToNewLine() {
		return VerifyCodeFixAsync(
			"""
			namespace TestCode {
				public sealed class Derived(string name, int count{|HA9002:)|}
					: Base(name) {
				}

				public abstract class Base(string name);
			}
			""",
			"""
			namespace TestCode {
				public sealed class Derived(
					string name,
					int count
				) : Base(name) {
				}

				public abstract class Base(string name);
			}
			"""
		);
	}

	[Fact]
	public Task MovesExpressionBodiedMethodArrowToClosingParenLine() {
		return VerifyCodeFixAsync(
			InType(
				"""
				private static string Format(
					string value,
					int count
				{|HA9002:)|}
					=> value;
				"""
			),
			InType(
				"""
				private static string Format(
					string value,
					int count
				) => value;
				"""
			)
		);
	}

	[Fact]
	public Task RealignsExpressionBodiedInvocationHangingArguments() {
		return VerifyCodeFixAsync(
			ExpressionBodiedInvocationWithOverIndentedHangingArguments(),
			ExpressionBodiedInvocationWithAlignedHangingArguments()
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
