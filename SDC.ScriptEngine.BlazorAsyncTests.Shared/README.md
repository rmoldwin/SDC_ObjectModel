# SDC.ScriptEngine.BlazorAsyncTests.Shared

## What this project is

`SDC.ScriptEngine.BlazorAsyncTests.Shared` is a plain class library factoring out the test-runner
building blocks common to both `SDC.ScriptEngine.BlazorAsyncTests` (Phase 1, single-threaded) and
`SDC.ScriptEngine.BlazorAsyncTests.Phase2` (multi-threaded), so the two Blazor test hosts don't
duplicate the same tree-building and sequential-test-execution logic.

## Basic architecture

- `SDC.ScriptEngine.BlazorAsyncTests.Shared.csproj` is a plain SDK-style class library targeting
  `net10.0`, with a project reference to `SDC.Schema`.
- `SdcTreeBuilder.cs` builds real Structured Data Capture (SDC) Object Model (OM) trees from the
  shared test fixtures for use by both Blazor hosts' test pages.
- `WasmTestCase.cs` defines the test-case model, including a `RequiresThreads` flag so
  thread-dependent cases can be skipped automatically in a single-threaded WASM environment.
- `WasmTestRunner.cs` runs `WasmTestCase` objects **sequentially, never concurrently**, to avoid
  `LastTopNode`-corruption that would occur from interleaved tree builds. It exposes
  `WasmThreadsEnabled` (true when `Environment.ProcessorCount > 1`, i.e. `WasmEnableThreads=true`
  actually took effect) and yields to the UI thread between tests so the browser stays responsive.

## State of completion

- Small, stable utility library (3 source files); not itself a test host — see
  `SDC.ScriptEngine.BlazorAsyncTests` and `SDC.ScriptEngine.BlazorAsyncTests.Phase2` for the actual
  interactive test pages that consume these types.
- No project-authored `TODO`/`FIXME` notes found in this project's source files.
