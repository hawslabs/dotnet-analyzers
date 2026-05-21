using Microsoft.CodeAnalysis.Diagnostics;

namespace HawsLabs.Analyzers;

internal static class AnalyzerConfigOptionsExtensions {
	public static bool TryGetTrimmedValue(this AnalyzerConfigOptions options, string name, out string value) {
		if (
			!options.TryGetValue(name, out var rawValue)
			&& !options.TryGetValue(name.ToLowerInvariant(), out rawValue)
		) {
			value = string.Empty;
			return false;
		}

		value = rawValue.Trim();
		return true;
	}

	public static bool TryGetPositiveInt32(this AnalyzerConfigOptions options, string name, out int value) {
		if (
			options.TryGetValue(name, out var rawValue)
			&& int.TryParse(rawValue, out value)
			&& value > 0
		) {
			return true;
		}

		value = 0;
		return false;
	}
}
