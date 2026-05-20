namespace HawsLabs.Analyzers.Tests.Testing;

internal static class CSharpSource {
	public static string InMethodBody(string body, string? supportingMembers = null) {
		var members = JoinBlocks(
			"""
private static void CallTarget(int first, int second) {
}
""",
			supportingMembers,
			$$"""
private static void Test() {
{{FormatBlock(body, 1)}}
}
"""
		);

		return InType(members);
	}

	public static string InType(string members) {
		return $$"""
using System;

internal static class TestCode {
{{FormatBlock(members, 1)}}
}
""";
	}

	private static string JoinBlocks(params string?[] blocks) {
		return string.Join(
			Environment.NewLine + Environment.NewLine,
			blocks
				.Where(static block => !string.IsNullOrWhiteSpace(block))
				.Select(static block => block!.ReplaceLineEndings(Environment.NewLine).Trim('\r', '\n'))
		);
	}

	private static string FormatBlock(string text, int indentLevel) {
		var normalized = text.ReplaceLineEndings(Environment.NewLine).Trim('\r', '\n');

		if (string.IsNullOrWhiteSpace(normalized)) {
			return string.Empty;
		}

		var indent = new string('\t', indentLevel);
		return indent + normalized.Replace(Environment.NewLine, Environment.NewLine + indent);
	}
}
