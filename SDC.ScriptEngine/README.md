# SDC.ScriptEngine

## What this project is

`SDC.ScriptEngine` is the core scripting library for the Structured Data Capture (SDC) solution. It compiles small C# script bodies with Roslyn, runs them against live Structured Data Capture (SDC) Object Model (OM) nodes from `SDC.Schema`, and supports both compile-and-run and pre-compiled-document workflows.

## Basic architecture

- `SDC.ScriptEngine.csproj` targets `net10.0` and has a direct `<ProjectReference>` to `..\SDC.Schema\SDC.Schema.csproj`.
- `SdcScriptEngine.cs` is the orchestrator. It canonicalizes script text, computes the canonical hash, compiles with Roslyn, caches compiled outputs, loads the generated assembly, and invokes `SdcScript.Execute(BaseType sdc)`.
- `SdcScriptTemplate.cs` defines the generated wrapper around the user-authored method body.
- `SdcScriptCanonicalizer.cs`, `SdcScriptHashInspector.cs`, and `ScriptHashMismatchException.cs` define the hash/integrity workflow used by stored scripts.
- `SdcScriptLoadContext.cs` and `SdcScriptCache.cs` implement the execution/runtime layer: one load context per compiled script plus a cache keyed by canonical hash.
- `ISdcScriptReferenceProvider.cs`, `AppDomainReferenceProvider.cs`, and `PreloadedReferenceProvider.cs` are the platform seam for Roslyn references. Desktop hosts use the application-domain path; browser hosts preload references and hand them in synchronously.
- `SdcScriptResults.cs`, `SdcScriptEngineOptions.cs`, and `SdcScriptNode.cs` define the public result types, options, and the current Phase 1 storage container for source text, stored hash, and compiled bytes.

## State of completion

- Rough scope: one library project with 13 source files, centered on compile, cache, load, hash-check, and run behavior.
- The library already supports live OM mutation, cached execution, and the pre-compiled-document path used by the host projects in this repository.
- No project-authored `TODO` or `FIXME` comments were found in the current source files.
- `SdcScriptNode` is explicitly a temporary storage type, not a schema-backed OM node yet.
- Clearly relevant open issues: [#14](https://github.com/rmoldwin/SDC_ObjectModel/issues/14), [#15](https://github.com/rmoldwin/SDC_ObjectModel/issues/15), [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16).
