# SDC.Schema.QA.Tests

## What the project is

`SDC.Schema.QA.Tests` is the Microsoft Test (MSTest) test project for `SDC.Schema.QA`. It verifies that each Quality Assurance (QA) rule reports the expected findings when given real Structured Data Capture (SDC) object-model trees, and it also proves that the QA report layer stays in sync with the core `SDC.Schema` validation pipeline.

## Basic architecture

- The test files are organized mostly one class per rule or feature:
  - `CoherenceValidationBridgeRuleTests.cs` tests the bridge between `ValidateTree()` findings and the QA report.
  - `DuplicateIdRuleTests.cs`, `EmptyExtensionRuleTests.cs`, `NoInternalStateInJsonRuleTests.cs`, `TreeIntegrityRuleTests.cs`, and `UnresolvedRejectedValuesRuleTests.cs` each cover one default QA rule.
- `MSTestSettings.cs` documents why method-level parallelism is intentionally disabled for this project: ambient state in `SDC.Schema` can leak across pooled worker threads and make isolated tests interfere with each other.
- The project file `SDC.Schema.QA.Tests.csproj` references both `..\SDC.Schema.QA\SDC.Schema.QA.csproj` and `..\SDC.Schema\SDC.Schema.csproj`. That means this project directly tests the QA engine while also building real SDC trees from the core library.
- No other source subfolders are present here; this is a deliberately small, focused test project.

## State of completion

- Rough scale: 7 C# source files with 17 `[TestMethod]` tests, focused on one rule area per file plus the project-wide MSTest configuration note.
- TODO and FIXME notes: no `TODO` or `FIXME` matches were found in this project folder during a text search.
- Open roadmap issues clearly relevant to this project:
  - [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28) — intermittent `SDC.Schema.QA.Tests` failures under method-level parallelism caused by ambient state leaking across reused worker threads; `MSTestSettings.cs` records the current workaround.
