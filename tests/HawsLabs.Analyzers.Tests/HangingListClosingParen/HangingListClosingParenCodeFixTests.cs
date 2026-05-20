using Xunit;

namespace HawsLabs.Analyzers.Tests.HangingListClosingParen;

public sealed class HangingListClosingParenCodeFixTests : HangingListClosingParenTestFixture {
	[Fact]
	public Task MovesArgumentListClosingParenToItsOwnLine() {
		return VerifyCodeFixAsync(
			InMethodBody(
				"""
CallTarget(
	1,
	2[|)|];
"""),
			InMethodBody(
				"""
CallTarget(
	1,
	2
);
"""));
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
"""),
			InType(
				"""
private static void Test(
	int first,
	int second
) {
}
"""));
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
"""),
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
"""));
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
"""),
			InMethodBody(
				"""
CallTarget(
	1,
	2
);
"""));
	}
}
