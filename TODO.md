# TODO

- [ ] Example: Add a new feature to the plan and implement it.



Create `.agents/instructions/hawslabs-analysis-rule-docs.instructions.md` which instructs the AI how to maintain the documentation for the custom roslyn code analyzers / code fixes in this repo. Right now there is only one rule in the repo, but I want you to document how `Meziantou.Analyzer` documents their rules:

- https://github.com/meziantou/Meziantou.Analyzer/tree/main/docs
- https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0001.md (but I want the rules folder to be all lowercase)

I think i want the actual rules to be documented similar to this:
https://github.com/TestableIO/System.IO.Abstractions.Analyzers/blob/develop/docs/IO0004.MD

mixed with this:
https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP014.md


I like the table at the top, Description, Motivation, How to fix violations, Code with Diagnostic, and Code with Fix sections per rule.