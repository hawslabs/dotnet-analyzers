using Xunit;

namespace HawsLabs.Analyzers.Tests.ReturnRawStringLiteral;

public sealed class ReturnRawStringLiteralCodeFixTests : ReturnRawStringLiteralTestFixture {
	[Fact]
	public Task FormatsReturnedInterpolatedRawString() {
		return VerifyCodeFixWithoutMarkupAsync(
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
			"""",
			Diagnostic().WithSpan(3, 10, 3, 15)
		);
	}

	[Fact]
	public Task FormatsInterpolatedRawStringArgument() {
		return VerifyCodeFixWithoutMarkupAsync(
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
	public Task FormatsReturnedRawStringWithRelativeContentIndentation() {
		return VerifyCodeFixAsync(
			""""
			internal static class TestCode {
				public static string Create() {
					return [|"""|]
			{
				"value": true
			}
			""";
				}
			}
			"""",
			""""
			internal static class TestCode {
				public static string Create() {
					return
						"""
						{
							"value": true
						}
						""";
				}
			}
			""""
		);
	}

	[Fact]
	public Task ReindentsReturnedRawStringThatAlreadyStartsAfterReturn() {
		return VerifyCodeFixAsync(
			""""
			internal static class TestCode {
				public static string Create() {
					return
						[|"""|]
			value
			""";
				}
			}
			"""",
			""""
			internal static class TestCode {
				public static string Create() {
					return
						"""
						value
						""";
				}
			}
			""""
		);
	}
}
