# SDC_ObjectModel

Serialize/Deserialize SDC XML templates from/to the SDC Object Model (OM). Edit SDC templates inside the SDC OM. Validate SDC XML. Compare SDC XML versions.

## Projects

### Core

| Project | Purpose |
|---|---|
| [`SDC.Schema`](SDC.Schema/README.md) | The core SDC Object Model (OM) class library and serializers. |
| [`SDC.Schema.Tests`](SDC.Schema.Tests/README.md) | MSTest unit and integration tests for `SDC.Schema` (serialization, validation, tree construction/mutation, ad-hoc attributes, thread safety). |
| [`Benchmarks`](Benchmarks/README.md) | BenchmarkDotNet performance benchmarks for selected `SDC.Schema` operations. |

### Quality Assurance (QA)

| Project | Purpose |
|---|---|
| [`SDC.Schema.QA`](SDC.Schema.QA/README.md) | Best-practice rules engine that checks a hydrated SDC tree using only the public `SDC.Schema` API, and reports findings for tooling/human review. |
| [`SDC.Schema.QA.Tests`](SDC.Schema.QA.Tests/README.md) | MSTest project verifying `SDC.Schema.QA`'s rules and its bridge to the core validation pipeline. |
| [`SDC.Schema.QA.ExampleGenerator`](SDC.Schema.QA.ExampleGenerator/README.md) | Console app that generates the numbered example artifacts referenced by the QA best-practices documentation. |

### Scripting (SDC.ScriptEngine)

| Project | Purpose |
|---|---|
| [`SDC.ScriptEngine`](SDC.ScriptEngine/README.md) | Core scripting library — compiles/runs small C# script bodies with Roslyn against live SDC OM nodes. |
| [`SDC.ScriptEngine.Tests`](SDC.ScriptEngine.Tests/README.md) | MSTest behavioral test suite for `SDC.ScriptEngine`. |
| [`SDC.ScriptEngine.WpfTest`](SDC.ScriptEngine.WpfTest/README.md) | Desktop (WPF) comparison host — compile/run/cache scripts and inspect canonical-hash behavior outside the browser. |
| [`SDC.ScriptEngine.BlazorTest`](SDC.ScriptEngine.BlazorTest/README.md) | Main Blazor WebAssembly (WASM) host — demonstrates pre-compiled and experimental in-browser Roslyn script execution. |
| [`SDC.ScriptEngine.WasmSpike`](SDC.ScriptEngine.WasmSpike/README.md) | Focused WASM proof-of-feasibility spike for the hardest browser-side steps (in-browser Roslyn compilation, assembly loading). |

### WASM thread-safety investigation (SDC.ScriptEngine.BlazorAsyncTests)

Active investigation into real multi-threaded WASM behavior — see
[`..docs/architecture/thread-safety.md`](..docs/architecture/thread-safety.md) for the scope-caveat
banner and current findings (issues [#17](https://github.com/rmoldwin/SDC_ObjectModel/issues/17),
[#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20)–[#25](https://github.com/rmoldwin/SDC_ObjectModel/issues/25),
[#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28)).

| Project | Purpose |
|---|---|
| [`SDC.ScriptEngine.BlazorAsyncTests`](SDC.ScriptEngine.BlazorAsyncTests/README.md) | Phase 1 (single-threaded) WASM test harness — establishes the baseline before multi-threading. |
| [`SDC.ScriptEngine.BlazorAsyncTests.Phase2`](SDC.ScriptEngine.BlazorAsyncTests.Phase2/README.md) | Phase 2 (multi-threaded, `WasmEnableThreads=true`) WASM test harness — surfaced the still-open thread-safety bugs. |
| [`SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server`](SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server/README.md) | Thin ASP.NET Core host that injects the COOP/COEP headers `SharedArrayBuffer` requires, so Phase 2 can actually run multi-threaded. |
| [`SDC.ScriptEngine.BlazorAsyncTests.Shared`](SDC.ScriptEngine.BlazorAsyncTests.Shared/README.md) | Shared test-runner/tree-builder library used by both the Phase 1 and Phase 2 hosts. |

## Serializers

The SDC OM supports four serialization formats:

| Serializer | Format | Library |
|---|---|---|
| `SdcSerializer<T>` | XML | `System.Xml.Serialization.XmlSerializer` |
| `SdcSerializerJson<T>` | JSON | `Newtonsoft.Json` |
| `SdcSerializerBson<T>` | BSON | `Newtonsoft.Json` (BsonDataWriter/BsonDataReader) |
| `SdcSerializerMsgPack<T>` | MessagePack | `Newtonsoft.Msgpack` (MessagePackWriter/MessagePackReader) |

## Documentation

- [`..docs/summary.md`](..docs/summary.md) — start here: the technical knowledge base index (architecture, roadmap, glossary, conventions).
- [`..docs/roadmap.md`](..docs/roadmap.md) — planned work, each item linked to a GitHub issue.
- [`sessions/README.md`](sessions/README.md) — AI session continuity/handoff document index.


