using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OneTrueBraceStyleAnalyzer : DiagnosticAnalyzer {
	public const string DiagnosticId = DiagnosticIds.OneTrueBraceStyle;

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Keep 1TBS brace settings consistent",
		messageFormat: "indent_brace_style={0} requires '{1} = {3}', but the effective value is '{2}'",
		category: DiagnosticCategories.Style,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description:
			"When indent_brace_style is set to 1TBS or OTBS, the C# brace and newline formatting "
			+ "options should use the matching one true brace style values."
	);

	private static readonly ImmutableArray<ExpectedOption> ExpectedOptions = ImmutableArray.Create(
		new ExpectedOption("csharp_new_line_before_open_brace", "none"),
		new ExpectedOption("csharp_new_line_before_else", "false"),
		new ExpectedOption("csharp_new_line_before_catch", "false"),
		new ExpectedOption("csharp_new_line_before_finally", "false"),
		new ExpectedOption("csharp_new_line_before_members_in_object_initializers", "false"),
		new ExpectedOption("csharp_new_line_before_members_in_anonymous_types", "false"),
		new ExpectedOption("csharp_new_line_between_query_expression_clauses", "false")
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context) {
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(static compilationContext => {
			var diagnosticOptions = compilationContext.Compilation.Options.SpecificDiagnosticOptions;
			compilationContext.RegisterSyntaxTreeAction(treeContext => AnalyzeSyntaxTree(treeContext, diagnosticOptions));
		});
	}

	private static void AnalyzeSyntaxTree(
		SyntaxTreeAnalysisContext context,
		ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions
	) {
		var optionsProvider = context.Options.AnalyzerConfigOptionsProvider;
		var options = optionsProvider.GetOptions(context.Tree);

		if (!TryGetOneTrueBraceStyle(options, out var braceStyle)) {
			return;
		}

		var location = GetDiagnosticLocation(context);

		foreach (var option in ExpectedOptions) {
			Check(context, options, location, braceStyle, option);
		}

		CheckPreferBraces(context, options, optionsProvider.GlobalOptions, diagnosticOptions, location, braceStyle);
	}

	private static void Check(
		SyntaxTreeAnalysisContext context,
		AnalyzerConfigOptions options,
		Location location,
		string braceStyle,
		ExpectedOption option
	) {
		var actual = option.GetActualValue(options);

		if (string.Equals(actual, option.Value, StringComparison.OrdinalIgnoreCase)) {
			return;
		}

		context.ReportDiagnostic(
			Diagnostic.Create(
				Rule,
				location,
				braceStyle,
				option.Name,
				actual,
				option.Value
			)
		);
	}

	private static void CheckPreferBraces(
		SyntaxTreeAnalysisContext context,
		AnalyzerConfigOptions options,
		AnalyzerConfigOptions globalOptions,
		ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions,
		Location location,
		string braceStyle
	) {
		var actual = options.TryGetTrimmedValue("csharp_prefer_braces", out var rawValue)
			? rawValue
			: "<missing>";

		var preferenceValue = GetValueBeforeColon(actual);

		if (!string.Equals(preferenceValue, "true", StringComparison.OrdinalIgnoreCase)) {
			context.ReportDiagnostic(
				Diagnostic.Create(
					Rule,
					location,
					braceStyle,
					"csharp_prefer_braces",
					actual,
					"true"
				)
			);
		}

		if (HasErrorSeverity(actual) || HasRuleErrorSeverity(options, globalOptions, diagnosticOptions)) {
			return;
		}

		if (!TryGetReportedSeverity(options, globalOptions, diagnosticOptions, actual, out var severity)) {
			return;
		}

		context.ReportDiagnostic(
			Diagnostic.Create(
				Rule,
				location,
				braceStyle,
				"dotnet_diagnostic.IDE0011.severity",
				severity,
				"error"
			)
		);
	}

	private static bool TryGetOneTrueBraceStyle(
		AnalyzerConfigOptions options,
		out string braceStyle
	) {
		braceStyle = string.Empty;

		if (!options.TryGetValue("indent_brace_style", out var value)) {
			return false;
		}

		var normalizedValue = value.Trim();

		if (
			!string.Equals(normalizedValue, "1TBS", StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(normalizedValue, "OTBS", StringComparison.OrdinalIgnoreCase)
		) {
			return false;
		}

		braceStyle = normalizedValue;
		return true;
	}

	private static bool HasRuleErrorSeverity(
		AnalyzerConfigOptions options,
		AnalyzerConfigOptions globalOptions,
		ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions
	) {
		return TryGetDiagnosticSeverity(options, globalOptions, diagnosticOptions, out var severity)
			&& string.Equals(severity, "error", StringComparison.OrdinalIgnoreCase);
	}

	private static bool TryGetReportedSeverity(
		AnalyzerConfigOptions options,
		AnalyzerConfigOptions globalOptions,
		ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions,
		string preferBracesValue,
		out string severity
	) {
		if (TryGetDiagnosticSeverity(options, globalOptions, diagnosticOptions, out severity)) {
			return true;
		}

		return TryGetValueAfterColon(preferBracesValue, out severity);
	}

	private static bool TryGetDiagnosticSeverity(
		AnalyzerConfigOptions options,
		AnalyzerConfigOptions globalOptions,
		ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions,
		out string severity
	) {
		if (
			options.TryGetTrimmedValue("dotnet_diagnostic.IDE0011.severity", out severity)
			|| globalOptions.TryGetTrimmedValue("dotnet_diagnostic.IDE0011.severity", out severity)
		) {
			return true;
		}

		if (!TryGetSpecificDiagnosticSeverity(diagnosticOptions, out var reportDiagnostic)) {
			severity = string.Empty;
			return false;
		}

		severity = GetSeverityName(reportDiagnostic);
		return true;
	}

	private static bool TryGetSpecificDiagnosticSeverity(
		ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions,
		out ReportDiagnostic severity
	) {
		return diagnosticOptions.TryGetValue("IDE0011", out severity)
			|| diagnosticOptions.TryGetValue("ide0011", out severity);
	}

	private static string GetSeverityName(ReportDiagnostic severity) {
		return severity switch {
			ReportDiagnostic.Error => "error",
			ReportDiagnostic.Warn => "warning",
			ReportDiagnostic.Info => "suggestion",
			ReportDiagnostic.Hidden => "silent",
			ReportDiagnostic.Suppress => "none",
			ReportDiagnostic.Default => "default",
			_ => severity.ToString(),
		};
	}

	private static string GetValueBeforeColon(string value) {
		var colonIndex = value.IndexOf(':');
		return colonIndex < 0 ? value.Trim() : value.Substring(0, colonIndex).Trim();
	}

	private static bool HasErrorSeverity(string value) {
		return TryGetValueAfterColon(value, out var severity)
			&& string.Equals(severity, "error", StringComparison.OrdinalIgnoreCase);
	}

	private static bool TryGetValueAfterColon(string value, out string severity) {
		var colonIndex = value.IndexOf(':');

		if (colonIndex < 0) {
			severity = string.Empty;
			return false;
		}

		severity = value.Substring(colonIndex + 1).Trim();
		return true;
	}

	private static Location GetDiagnosticLocation(SyntaxTreeAnalysisContext context) {
		var root = context.Tree.GetRoot(context.CancellationToken);
		var firstToken = root.GetFirstToken();

		if (firstToken.RawKind == 0) {
			return Location.Create(context.Tree, new TextSpan(0, 0));
		}

		return firstToken.GetLocation();
	}

	private readonly struct ExpectedOption {
		public ExpectedOption(string name, string value) {
			Name = name;
			Value = value;
		}

		public string Name { get; }

		public string Value { get; }

		public string GetActualValue(AnalyzerConfigOptions options) {
			return options.TryGetTrimmedValue(Name, out var actualValue) ? actualValue : "<missing>";
		}
	}
}
