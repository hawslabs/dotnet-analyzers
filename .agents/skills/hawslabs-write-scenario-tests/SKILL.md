---
name: hawslabs-write-scenario-tests
description: Create or update HawsLabs tests for analyzers, code fixes, or other implementations using clear fixtures and focused source scenarios.
argument-hint: "[scenario]"
---
# HawsLabs Scenario Test Skill

1. Identify whether the scenario belongs in analyzer unit tests, code-fix tests, integration tests, or another existing test layer.
2. Reuse existing test helpers and fixtures before adding new ones.
3. Model each scenario around the smallest source input and the expected diagnostic or fixed output.
4. Keep shared Arrange and Act steps in helpers or fixtures only when that improves readability.
5. Cover positive, negative, and edge-case behavior for the rule under test.
6. Assert only the behavior that matters: diagnostic Id, location, message, or exact fixed source.
7. If the repository lacks a suitable test project, add the smallest one consistent with the current structure.
8. Keep tests readable and avoid over-abstracting assertions.
