# SDC.ScriptEngine.BlazorTest

## What this project is

`SDC.ScriptEngine.BlazorTest` is the main Blazor WebAssembly (WASM) host for the scripting work. It demonstrates two browser-side paths for `SDC.ScriptEngine`: running pre-compiled script bytes against live Structured Data Capture (SDC) Object Model (OM) nodes today, and experimentally compiling/running scripts in-browser with Roslyn after loading browser-side assembly references.

## Basic architecture

- `SDC.ScriptEngine.BlazorTest.csproj` targets `net10.0`, references `..\SDC.ScriptEngine\SDC.ScriptEngine.csproj`, and adds the explicit Roslyn package needed by the browser reference loader.
- `Program.cs` registers the `HttpClient` base address used to fetch browser-deployed assemblies.
- `Pages\Home.razor` is the main harness. It contains:
  - a Phase 1 pre-compiled-byte panel,
  - a Phase 2 live compile/run panel,
  - a hash inspector,
  - and an engine diagnostics log.
- `WasmReferenceProvider.cs` implements the browser-specific reference-loading path by fetching assembly bytes from `_framework/`, converting them to Roslyn metadata references, and caching the result.
- `wwwroot\wasm-boot-helpers.js` extracts the .NET 10 boot manifest from `dotnet.*.js` so the provider can find the fingerprinted assembly names actually deployed by the browser host.
- Relationship to other projects:
  - direct project reference: `SDC.ScriptEngine`,
  - transitive OM dependency: `SDC.Schema` through the engine,
  - shared hosting context: see [../docs/architecture/wasm-blazor.md](../docs/architecture/wasm-blazor.md).

## State of completion

- Rough scope: one main Razor page plus one browser reference provider and one JavaScript manifest helper.
- The project already demonstrates the current "desktop precompile, browser run" path and includes an explicit browser-side compile/run path for experimentation.
- No project-authored `TODO` or `FIXME` comments were found in the current source files.
- Browser-side caveats, packaging assumptions, and the separate unmerged WebAssembly thread-safety work are documented in [../docs/architecture/wasm-blazor.md](../docs/architecture/wasm-blazor.md).
- Clearly relevant open issues: [#14](https://github.com/rmoldwin/SDC_ObjectModel/issues/14), [#15](https://github.com/rmoldwin/SDC_ObjectModel/issues/15), [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16).
