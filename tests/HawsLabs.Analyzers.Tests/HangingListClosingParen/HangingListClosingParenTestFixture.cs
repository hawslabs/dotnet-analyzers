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
			namespace BoringLab.ServiceBus.OnPrem.Client;

			public sealed class OnPremServiceBusClient(
				OnPremServiceClientConnection connection,
				ServiceBusClientAuthentication authentication
			{|HA9002:)|}
				: SignalRServiceBusClient(authentication) {
				protected override bool TryGetBusUrl(
					ServiceBusEnvelope envelope,
					out string busUrl,
					out ServiceBusSendResult unavailable
				) => TryUseConfiguredBusUrl(connection.BrokerUrl, out busUrl, out unavailable);
			}

			public sealed class OnPremServiceClientConnection {
				public string BrokerUrl => string.Empty;
			}

			public sealed class ServiceBusClientAuthentication;

			public sealed class ServiceBusEnvelope;

			public sealed class ServiceBusSendResult;

			public abstract class SignalRServiceBusClient(ServiceBusClientAuthentication authentication) {
				protected abstract bool TryGetBusUrl(
					ServiceBusEnvelope envelope,
					out string busUrl,
					out ServiceBusSendResult unavailable
				);

				protected bool TryUseConfiguredBusUrl(
					string brokerUrl,
					out string busUrl,
					out ServiceBusSendResult unavailable
				) {
					busUrl = brokerUrl;
					unavailable = new ServiceBusSendResult();
					return true;
				}
			}
			""";
	}

	protected static string PrimaryConstructorBaseListOnClosingParenLine() {
		var source = PrimaryConstructorBaseListOnNextLine().Replace("{|HA9002:)|}", ")", StringComparison.Ordinal);

		return source.Replace(
			")\r\n\t: SignalRServiceBusClient(authentication)",
			") : SignalRServiceBusClient(authentication)",
			StringComparison.Ordinal
		).Replace(
			")\n\t: SignalRServiceBusClient(authentication)",
			") : SignalRServiceBusClient(authentication)",
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
