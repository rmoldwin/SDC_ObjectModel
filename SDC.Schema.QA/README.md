# SDC.Schema.QA

## What the project is

`SDC.Schema.QA` is the Quality Assurance (QA) rules engine for the Structured Data Capture (SDC) object model. It runs best-practice checks against a hydrated SDC tree by using only the public Application Programming Interface (API) exposed by `SDC.Schema`, then reports findings in a structured form that can be rendered for automated tooling or human review.

## Basic architecture

- `Rules/` contains the rule engine and rule implementations:
  - `IQaRule.cs` defines the rule contract.
  - `QaEngine.cs` runs a supplied rule set, and its default rule list currently wires up six rules.
  - `Rules\AdHocAttributes\` contains the inert-extension check (`EmptyExtensionRule`).
  - `Rules\Construction\` contains tree-integrity checks.
  - `Rules\Mutation\` contains duplicate-Identifier checks.
  - `Rules\Serialization\` contains JavaScript Object Notation (JSON) serialization best-practice checks.
  - `Rules\Validation\` contains unresolved rejected-value checks and the bridge from `ValidateTree()` findings into QA findings.
- `Reporting/` contains the report model and rendering types: `QaFinding`, `QaReport`, `QaSeverity`, and `QaConvention`. `QaReport` renders both Markdown and self-contained HyperText Markup Language (HTML) output.
- The project file `SDC.Schema.QA.csproj` references `..\SDC.Schema\SDC.Schema.csproj` and describes this project as a public-API consumer only. Build-time dependencies go one way: `SDC.Schema.QA` depends on `SDC.Schema`, while `SDC.Schema.QA.Tests` and `SDC.Schema.QA.ExampleGenerator` depend on `SDC.Schema.QA`.
- The rule catalog described in `..docs\architecture\qa-best-practices.md` matches the category folders present here: ad-hoc attributes, construction, mutation, serialization, and validation.

## State of completion

- Rough scale: 12 C# source files implement the rule engine, report types, and six default Best Practice rules covering ad-hoc attributes, construction integrity, mutation safety, serialization leakage, unresolved rejected values, and validation-report bridging.
- TODO and FIXME notes: no `TODO` or `FIXME` matches were found in this project folder during a text search.
- Open roadmap issues clearly scoped to this project: none are called out directly in `..docs/roadmap.md`. The roadmap currently tracks the underlying core-library serialization, validation, and thread-safety gaps more than the QA engine itself.
