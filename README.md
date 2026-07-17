# SDC.Schema — SDC Object Model (OM) for .NET

This repository implements the **Structured Data Capture (SDC)** standard's **Object Model
(OM)** for .NET: a class library that represents an SDC form (a Form Design File, or FDF) as an
in-memory tree, plus the serialization, validation, rules, and tooling built around it.

For the full technical knowledge base — architecture chapters, a running roadmap linked to
GitHub issues, and a glossary of every initialism used in this project — see
**[docs/summary.md](docs/summary.md)**. Session continuity/handoff notes live in
**[sessions/](sessions/README.md)**. This README gives a top-level map of the solution; each
project folder also has its own `README.md` with more detail.

## Projects in this solution

| Project | Role |
|---|---|
| [`SDC.Schema`](SDC.Schema/README.md) | The core SDC Object Model (OM) class library: the node hierarchy, (de)serialization (XML/JSON/BSON/MessagePack), built-in validation, the rules engine, tree comparison (`CompareTrees`), and the `SdcUtil` helper library. Generated substantially by Xsd2Code++ from XML Schema Definition (XSD) files, with hand customizations layered on top. |
| [`SDC.Schema.Tests`](SDC.Schema.Tests/README.md) | Unit/functional tests for `SDC.Schema`, plus supporting sample SDC XML instances (`Test Files/`) and prior technical write-ups (`Documentation/`, including `Archived Plans/`). |
| [`SDC.Schema.QA`](SDC.Schema.QA/README.md) | The Quality Assurance (QA) engine: the Best Practice (`BP-*`) rule catalog that checks SDC trees for structural/serialization/mutation problems beyond basic schema validity. |
| [`SDC.Schema.QA.Tests`](SDC.Schema.QA.Tests/README.md) | Tests for `SDC.Schema.QA`. |
| [`SDC.Schema.QA.ExampleGenerator`](SDC.Schema.QA.ExampleGenerator/README.md) | A console app that generates the numbered example SDC XML files used to demonstrate and exercise QA rule behavior. |
| [`SDC.ScriptEngine`](SDC.ScriptEngine/README.md) | The scripting/rules-execution engine that evaluates form-embedded scripts and expressions against the SDC Object Model (OM). |
| [`SDC.ScriptEngine.Tests`](SDC.ScriptEngine.Tests/README.md) | Tests for `SDC.ScriptEngine`. |
| [`SDC.ScriptEngine.BlazorTest`](SDC.ScriptEngine.BlazorTest/README.md) | A Blazor WebAssembly (WASM) host used to exercise `SDC.ScriptEngine` running in a browser. |
| [`SDC.ScriptEngine.WasmSpike`](SDC.ScriptEngine.WasmSpike/README.md) | An exploratory WebAssembly (WASM) spike project for scripting-engine browser hosting concerns. |
| [`SDC.ScriptEngine.WpfTest`](SDC.ScriptEngine.WpfTest/README.md) | A Windows Presentation Foundation (WPF) desktop host used as a non-WASM comparison point for `SDC.ScriptEngine`. |
| [`Benchmarks`](Benchmarks/README.md) | Performance benchmarks for SDC.Schema operations. |

## Where to start

- New to the object model? Start with
  [docs/architecture/object-model-overview.md](docs/architecture/object-model-overview.md).
- Looking for what's in flight? See [docs/roadmap.md](docs/roadmap.md) (every item is linked to
  a GitHub issue) and the open issues at
  https://github.com/rmoldwin/SDC_ObjectModel/issues.
- Unsure what an abbreviation means? Check [docs/glossary.md](docs/glossary.md) — this project
  has a hard "no cryptic jargon" rule (see [docs/conventions.md](docs/conventions.md)); every
  initialism should be spelled out on first use, including in commit titles, code comments, and
  issue titles.
- `docs/` holds **work-in-progress** technical material; once a topic is settled, it is promoted
  into the project **wiki**, which is the durable, polished reference.

## Keeping this README updated

This file (and every project's own `README.md`) should be reviewed and updated as part of each
pull request that changes a project's scope, architecture, or completion status — not just
periodically. If a change adds/removes a project, moves major functionality between projects, or
resolves/introduces a significant open issue, update the relevant README(s) in the same PR.
