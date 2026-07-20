# SDC.ScriptEngine.BlazorAsyncTests.Phase2

## What this project is

`SDC.ScriptEngine.BlazorAsyncTests.Phase2` is the multi-threaded follow-up to
`SDC.ScriptEngine.BlazorAsyncTests`, built to exercise real concurrent WASM execution
(`WasmEnableThreads=true`, requiring `SharedArrayBuffer`/cross-origin isolation, hence the
companion `SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server` host). It is the browser-side harness
that surfaced the still-open WASM thread-safety bugs tracked under
[../..docs/architecture/thread-safety.md](../..docs/architecture/thread-safety.md).

## Basic architecture

- `SDC.ScriptEngine.BlazorAsyncTests.Phase2.csproj` targets `net10.0` via the
  `Microsoft.NET.Sdk.BlazorWebAssembly` SDK, with `WasmEnableThreads=true`.
- `TestPages/` contains the concurrency-focused test pages: `BarrierTests.razor`,
  `CompareTreesParallelTests.razor`, `CompareTreesPerformanceTests.razor`,
  `SharedTopNodeTests.razor`, `ThreadSafetyReproTests.razor`, and `Index.razor`.
- `TestResources/` holds the same sample Extensible Markup Language (XML) fixtures as Phase 1
  (`BreastStagingTest2v1/v5.xml`, `CompareTreesTestV1/V2.xml`).
- `Documentation/BlazorAsyncTests_Phase2_Results.md` is the companion results note.
- Reuses the shared test-runner types from `SDC.ScriptEngine.BlazorAsyncTests.Shared` rather than
  duplicating the `TestEngine/` copy that Phase 1 still keeps locally.
- Must be run behind `SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server`, which injects the
  Cross-Origin-Opener-Policy/Cross-Origin-Embedder-Policy headers `SharedArrayBuffer` requires;
  it will not get real multi-threading if served any other way (e.g. a plain static file server).

## State of completion

- Active, still-open investigation. Found the multiple concurrent WASM race conditions/deadlocks
  tracked as issues [#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20)–[#25](https://github.com/rmoldwin/SDC_ObjectModel/issues/25)
  and [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28); see the scope-caveat banner in
  [../..docs/architecture/thread-safety.md](../..docs/architecture/thread-safety.md) — these
  `TS-#` labels reuse the desktop investigation's numbering scheme for **different, still-open**
  WASM-specific bugs.
- Not yet merged/resolved — needs further debugging before these tests can be considered
  passing/complete.
