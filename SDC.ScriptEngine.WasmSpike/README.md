# SDC.ScriptEngine.WasmSpike

## What this project is

`SDC.ScriptEngine.WasmSpike` is a focused Blazor WebAssembly (WASM) proof-of-feasibility spike. It is not the main host application; instead, it manually proves the hardest browser-side steps one layer lower: loading browser-deployed assembly bytes, compiling C# in-browser with Roslyn, loading the compiled assembly, and mutating a live Structured Data Capture (SDC) Object Model (OM) node.

## Basic architecture

- `SDC.ScriptEngine.WasmSpike.csproj` targets `net10.0`, references `..\SDC.Schema\SDC.Schema.csproj`, and adds the explicit Roslyn package directly. Unlike `SDC.ScriptEngine.BlazorTest`, it does **not** reference `SDC.ScriptEngine`; it prototypes the same browser-side mechanism by hand.
- `Pages\Spike.razor` is the heart of the project. It:
  - discovers deployed assemblies from the .NET 10 asset manifest,
  - fetches those assembly bytes from `_framework/`,
  - builds Roslyn metadata references,
  - compiles two small scripts,
  - loads them into `SpikeScriptLoadContext`,
  - checks `BaseType` type identity,
  - and verifies that a script-mutated `QuestionItemType.name` change is visible to the caller.
- `SpikeScriptLoadContext.cs` is the custom load context used by the spike.
- `SPIKE_RESULTS.md` is the companion results note describing the intended checks and current outcome.
- Relationship to other projects:
  - direct project reference: `SDC.Schema`,
  - conceptual precursor to the browser work described in [../docs/architecture/wasm-blazor.md](../docs/architecture/wasm-blazor.md).

## State of completion

- Rough scope: one spike page, one custom load-context file, and one results note.
- `SPIKE_RESULTS.md` currently says build compilation succeeded but the browser-runtime checklist is still pending, so the project should be read as an active spike rather than a closed result.
- No project-authored `TODO` or `FIXME` comments were found in the authored source files. Vendor files under `wwwroot\lib` do contain upstream `TODO` notes, but those are library baggage rather than project backlog.
- Clearly relevant open issue: [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16). For the broader hosting and concurrency context, see [../docs/architecture/wasm-blazor.md](../docs/architecture/wasm-blazor.md).
