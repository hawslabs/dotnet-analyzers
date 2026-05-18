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
                equivalenceKey: Title),
            diagnostic);

        return Task.CompletedTask;
    }

    private static async Task<Document> FixAsync(
        Document document,
        Diagnostic diagnostic,
        CancellationToken cancellationToken) {
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
        var fixedText = text.Replace(
            new TextSpan(closeParen.SpanStart, 0),
            lineBreak + expectedIndent);

        return document.WithText(fixedText);
    }

    private static string GetLineIndentation(SourceText text, TextLine line) {
        var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));
        var index = 0;

        while (
            index < lineText.Length
            && (lineText[index] == ' ' || lineText[index] == '\t')) {
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
