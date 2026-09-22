# SDC.ScriptEngine.BlazorAsyncTests

## What this project is

`SDC.ScriptEngine.BlazorAsyncTests` is a Blazor WebAssembly (WASM) test harness for the Phase 1
WASM thread-safety investigation (single-threaded WASM, `WasmEnableThreads=false`). It hosts a
set of interactive test pages that build real Structured Data Capture (SDC) Object Model (OM)
trees in the browser and exercise `CompareTrees<T>`, tree mutation, and basic concurrency
scenarios, sequentially, to establish a single-threaded baseline before the multi-threaded work
in `SDC.ScriptEngine.BlazorAsyncTests.Phase2`.

## Basic architecture

- `SDC.ScriptEngine.BlazorAsyncTests.csproj` targets `net10.0` via the `Microsoft.NET.Sdk.BlazorWebAssembly` SDK.
- `TestEngine/` holds the shared-style runner (`WasmTestRunner.cs`, `WasmTestCase.cs`,
  `SdcTreeBuilder.cs`) — the same design later factored out into
  `SDC.ScriptEngine.BlazorAsyncTests.Shared` for reuse by Phase 2.
- `TestPages/` contains the interactive Blazor test pages: `CompareTreesTests.razor`,
  `ComplexMutationTests.razor`, `ConcurrencyTests.razor`, and the `Index.razor` landing page.
- `TestResources/` holds the sample Extensible Markup Language (XML) fixtures
  (`BreastStagingTest2v1/v5.xml`, `CompareTreesTestV1/V2.xml`) used to build test trees.
- `Documentation/BlazorAsyncTests_Results.md` is the companion results note for this phase.
- Relationship to other projects: direct project reference to `SDC.Schema`; conceptual precursor
  to `SDC.ScriptEngine.BlazorAsyncTests.Phase2` and to
  [../..docs/architecture/wasm-blazor.md](../..docs/architecture/wasm-blazor.md).

## State of completion

- Single-threaded (Phase 1) baseline; superseded in scope by the multi-threaded Phase 2 project,
  but kept as the comparison baseline.
- Relevant open issues: [#17](https://github.com/rmoldwin/SDC_ObjectModel/issues/17),
  [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28) (see
  [../..docs/architecture/thread-safety.md](../..docs/architecture/thread-safety.md) for the
  scope-caveat banner covering this WASM investigation).
