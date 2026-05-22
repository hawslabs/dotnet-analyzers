using HawsLabs.Analyzers.Tests.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace HawsLabs.Analyzers.Tests.HangingListClosingParen;

public abstract class HangingListClosingParenTestFixture
	: CodeFixTestFixture<
		HawsLabs.Analyzers.HangingListClosingParenAnalyzer,
		HawsLabs.Analyzers.HangingListClosingParenCodeFixProvider> {
	protected static string InMethodBody(string body, string? supportingMembers = null) {
		return CSharpSource.InMethodBody(body, supportingMembers);
	}

	protected static string InType(string members) {
		return CSharpSource.InType(members);
	}

	protected static string PrimaryConstructorBaseListOnNextLine() {
		return """
			namespace TestCode;

			public sealed class Derived(
				string name,
				int count
			{|HA9002:)|}
				: Base(name) {
			}

			public abstract class Base(string name);
			""";
	}

	protected static string PrimaryConstructorBaseListOnClosingParenLine() {
		var source = PrimaryConstructorBaseListOnNextLine().Replace("{|HA9002:)|}", ")", StringComparison.Ordinal);

		return source.Replace(
			")\r\n\t: Base(name)",
			") : Base(name)",
			StringComparison.Ordinal
		).Replace(
			")\n\t: Base(name)",
			") : Base(name)",
			StringComparison.Ordinal
		);
	}

	protected static string ExpressionBodiedInvocationWithOverIndentedHangingArguments() {
		return """
			using System.Threading.Tasks;

			namespace TestCode;

			public static class Calls {
			    private static Task<int> TargetAsync(
			        string name,
			        int count,
			        bool enabled
			    ) => Task.FromResult(0);

			    public static Task<int> TestAsync() => TargetAsync(
			            "name",
			            1,
			            true
			    {|HA9006:)|};
			}
			""";
	}

	protected static string ExpressionBodiedInvocationWithAlignedHangingArguments() {
		return ExpressionBodiedInvocationWithOverIndentedHangingArguments()
			.Replace("{|HA9006:)|}", ")", StringComparison.Ordinal)
			.Replace(
				"            \"name\",",
				"        \"name\",",
				StringComparison.Ordinal
			).Replace(
				"            1,",
				"        1,",
				StringComparison.Ordinal
			).Replace(
				"            true",
				"        true",
				StringComparison.Ordinal
			);
	}

	protected static DiagnosticResult Diagnostic() {
		return new DiagnosticResult(
			HangingListClosingParenAnalyzer.DiagnosticId,
			DiagnosticSeverity.Warning
		);
	}

	protected static Task VerifyCodeFixWithoutMarkupAsync(
		string source,
		string fixedSource,
		params DiagnosticResult[] expectedDiagnostics
	) {
		return VerifyCodeFixAsync(source, fixedSource, MarkupMode.None, expectedDiagnostics);
	}
}
