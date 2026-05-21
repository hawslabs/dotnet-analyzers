namespace HawsLabs.Analyzers;

internal static class IndentationStyle {
	public static string GetIndentUnit(string lineIndent) {
		if (lineIndent.Contains('\t')) {
			return "\t";
		}

		if (lineIndent.Length >= 4 && lineIndent.Length % 4 == 0) {
			return "    ";
		}

		if (lineIndent.Length >= 2 && lineIndent.Length % 2 == 0) {
			return "  ";
		}

		if (lineIndent.Length > 0) {
			return new string(' ', lineIndent.Length);
		}

		return "\t";
	}
}
