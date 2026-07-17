# SDC.ScriptEngine.Tests

## What this project is

`SDC.ScriptEngine.Tests` is the Microsoft Test (MSTest) behavioral test suite for `SDC.ScriptEngine`. It verifies that scripts compile, cache, load, and run correctly against live Structured Data Capture (SDC) Object Model (OM) nodes from `SDC.Schema`, including the pre-compiled-document workflow used by the browser and desktop hosts.

## Basic architecture

- `SDC.ScriptEngine.Tests.csproj` targets `net10.0` and references both `..\SDC.ScriptEngine\SDC.ScriptEngine.csproj` and `..\SDC.Schema\SDC.Schema.csproj`.
- `ScriptEngineTestHelper.cs` is the shared test infrastructure. It creates engines, builds minimal OM trees, and exposes a compile counter for cache-behavior assertions.
- `SdcScriptCompilerTests.cs`, `SdcScriptDiagnosticsTests.cs`, `SdcScriptCacheTests.cs`, and `SdcScriptRunnerTests.cs` cover compile-time, diagnostic, cache, and execution behavior.
- `SdcScriptEngineIntegrationTests.cs`, `SdcScriptPrecompiledTests.cs`, and `SdcScriptOmMutationTests.cs` cover the end-to-end and OM-mutation paths, including the stored-hash plus stored-byte workflow.
- `WasmReferenceProviderTests.cs` validates the `PreloadedReferenceProvider` bridge used by browser-style reference loading.
- `SdcScriptEngineStressTests.cs` contains opt-in stress tests tagged with `TestCategory("Stress")` for heavier concurrency and memory checks.

## State of completion

- Rough scope: 10 test/source files, including one shared helper and one stress-test class.
- The suite covers the main production paths: compile, execute, cache hits, Base64 round-trip, hash mismatch handling, and live mutation of `QuestionItemType` and response nodes.
- No project-authored `TODO` or `FIXME` comments were found in the current test sources.
- The stress tests are deliberately separated behind `TestCategory("Stress")` rather than mixed into the default fast path.
- Clearly relevant open issue: [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16), because the preloaded-reference path tested here is the same shape used by the browser host.
