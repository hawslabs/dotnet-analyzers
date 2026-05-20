using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HawsLabs.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HangingListClosingParenCodeFixProvider))]
[Shared]
public sealed class HangingListClosingParenCodeFixProvider : CodeFixProvider {
	private const string Title = "Put closing parenthesis on its own line";

	public override ImmutableArray<string> FixableDiagnosticIds =>
		ImmutableArray.Create(HangingListClosingParenAnalyzer.DiagnosticId);

	public override FixAllProvider GetFixAllProvider() =>
		WellKnownFixAllProviders.BatchFixer;

	public override Task RegisterCodeFixesAsync(CodeFixContext context) {
		var diagnostic = context.Diagnostics[0];

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				cancellationToken => FixAsync(context.Document, diagnostic, cancellationToken),
				equivalenceKey: Title
			),
			diagnostic
		);

		return Task.CompletedTask;
	}

	private static async Task<Document> FixAsync(
		Document document,
		Diagnostic diagnostic,
		CancellationToken cancellationToken
	) {
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		if (root is null) {
			return document;
		}

		var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
		var closeParen = root.FindToken(diagnostic.Location.SourceSpan.Start);

		if (!closeParen.IsKind(SyntaxKind.CloseParenToken)) {
			return document;
		}

		var openParen = closeParen.Parent switch {
			ArgumentListSyntax argumentList => argumentList.OpenParenToken,
			ParameterListSyntax parameterList => parameterList.OpenParenToken,
			WhileStatementSyntax whileStatement => whileStatement.OpenParenToken,
			_ => default,
		};

		if (openParen.IsMissing) {
			return document;
		}

		var openLine = text.Lines.GetLineFromPosition(openParen.SpanStart);
		var closeLine = text.Lines.GetLineFromPosition(closeParen.SpanStart);
		var expectedIndent = GetLineIndentation(text, openLine);
		var actualPrefixSpan = TextSpan.FromBounds(closeLine.Start, closeParen.SpanStart);
		var actualPrefix = text.ToString(actualPrefixSpan);

		if (actualPrefix.All(static character => character is ' ' or '\t')) {
			return document.WithText(text.Replace(actualPrefixSpan, expectedIndent));
		}

		var lineBreak = GetLineBreak(text);

		if (
			TryFixRawStringLiteralClosingLine(
				text,
				closeParen,
				closeLine,
				actualPrefixSpan,
				actualPrefix,
				expectedIndent,
				lineBreak,
				out var fixedRawStringText
			)
		) {
			return document.WithText(fixedRawStringText);
		}

		var fixedText = text.Replace(
			new TextSpan(closeParen.SpanStart, 0),
			lineBreak + expectedIndent
		);

		return document.WithText(fixedText);
	}

	private static bool TryFixRawStringLiteralClosingLine(
		SourceText text,
		SyntaxToken closeParen,
		TextLine closeLine,
		TextSpan actualPrefixSpan,
		string actualPrefix,
		string expectedIndent,
		string lineBreak,
		out SourceText fixedText
	) {
		fixedText = text;

		if (
			!TryGetRawStringClosingDelimiter(
				actualPrefix,
				out var currentRawStringIndent,
				out var rawStringDelimiter
			)
		) {
			return false;
		}

		var rawStringToken = closeParen.GetPreviousToken();

		if (!IsMultilineRawStringLiteral(rawStringToken, rawStringDelimiter)) {
			return false;
		}

		var openingLine = text.Lines.GetLineFromPosition(rawStringToken.SpanStart);

		if (openingLine.LineNumber == closeLine.LineNumber) {
			return false;
		}

		var expectedRawStringIndent = GetLineIndentation(text, openingLine);
		var changes = new List<TextChange> {
			new(
				actualPrefixSpan,
				expectedRawStringIndent + rawStringDelimiter + lineBreak + expectedIndent
			),
		};

		if (expectedRawStringIndent != currentRawStringIndent) {
			for (
				var lineNumber = openingLine.LineNumber + 1;
				lineNumber < closeLine.LineNumber;
				lineNumber++
			) {
				var line = text.Lines[lineNumber];
				var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));

				if (lineText.Length == 0) {
					continue;
				}

				if (currentRawStringIndent.Length == 0) {
					changes.Add(new TextChange(new TextSpan(line.Start, 0), expectedRawStringIndent));
					continue;
				}

				if (!lineText.StartsWith(currentRawStringIndent, StringComparison.Ordinal)) {
					if (string.IsNullOrWhiteSpace(lineText)) {
						continue;
					}

					return false;
				}

				changes.Add(
					new TextChange(
						new TextSpan(line.Start, currentRawStringIndent.Length),
						expectedRawStringIndent
					)
				);
			}
		}

		fixedText = text.WithChanges(changes);
		return true;
	}

	private static bool TryGetRawStringClosingDelimiter(
		string actualPrefix,
		out string indent,
		out string delimiter
	) {
		indent = string.Empty;
		delimiter = string.Empty;

		var delimiterStart = 0;

		while (
			delimiterStart < actualPrefix.Length
			&& actualPrefix[delimiterStart] is ' ' or '\t'
		) {
			delimiterStart++;
		}

		var delimiterEnd = actualPrefix.Length;

		while (
			delimiterEnd > delimiterStart
			&& actualPrefix[delimiterEnd - 1] is ' ' or '\t'
		) {
			delimiterEnd--;
		}

		if (delimiterEnd - delimiterStart < 3) {
			return false;
		}

		for (var index = delimiterStart; index < delimiterEnd; index++) {
			if (actualPrefix[index] != '"') {
				return false;
			}
		}

		indent = actualPrefix.Substring(0, delimiterStart);
		delimiter = actualPrefix.Substring(delimiterStart, delimiterEnd - delimiterStart);
		return true;
	}

	private static bool IsMultilineRawStringLiteral(
		SyntaxToken token,
		string delimiter
	) {
		var tokenText = token.Text;

		return tokenText.StartsWith(delimiter, StringComparison.Ordinal)
			&& tokenText.EndsWith(delimiter, StringComparison.Ordinal)
			&& tokenText.Any(static character => character is '\r' or '\n');
	}

	private static string GetLineIndentation(SourceText text, TextLine line) {
		var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));
		var index = 0;

		while (
			index < lineText.Length
			&& (lineText[index] == ' ' || lineText[index] == '\t')
		) {
			index++;
		}

		return lineText.Substring(0, index);
	}

	private static string GetLineBreak(SourceText text) {
		foreach (var line in text.Lines) {
			if (line.EndIncludingLineBreak > line.End) {
				return text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
			}
		}

		return "\r\n";
	}
}
