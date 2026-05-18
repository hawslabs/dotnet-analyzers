using HawsLabs.Analyzers.Tests.Testing;

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
}
