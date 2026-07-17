# Running in WebAssembly (WASM) / Blazor

> **Status:** `SDC.ScriptEngine` already supports browser-hosted execution of pre-compiled Intermediate Language (IL) bytes, and this repository contains two browser-side compile/run prototypes. Production WebAssembly hosting is still incomplete.
>
> **Unmerged hardening work:** A read-only `git log --oneline Features/NET10/Net10Main..Features/NET10/ILandWASM/Main` check from this worktree shows substantial WebAssembly-specific hardening ahead of `Features/NET10/Net10Main`, including Sprint C through Sprint F commits for concurrent-dictionary migration, read/write lock coverage, per-tree child-node mutation locking, deferred child sorting, thread-safe unique-ID handling, and consolidated threading notes. That work lives on `Features/NET10/ILandWASM/Main` and related sprint branches and is **not yet merged** into `Features/NET10/Net10Main`.
>
> **Phase2 update:** the in-browser multi-threaded Blazor test harness (`SDC.ScriptEngine.BlazorAsyncTests.Phase2`, previously on its own unmerged `BlazorAsyncTests/Phase2` branch) was merged into `Features/NET10/ILandWASM/Main` via PR #43. It is documented in the new section below. It is still only on `ILandWASM/Main`, not yet on `Net10Main`.

This chapter is about the script-hosting story around `SDC.ScriptEngine`, not the general browser rendering story for the Structured Data Capture (SDC) Object Model (OM).

## What `SDC.ScriptEngine` is

`SDC.ScriptEngine` is a `net10.0` library that compiles and runs small C# script bodies against live `SDC.Schema` nodes. Its public surface is built around four methods on `SdcScriptEngine`:

- `CompileAsync(scriptText)` — canonicalizes the script, computes a stable hash, compiles with Roslyn, and returns compiled IL bytes plus diagnostics.
- `RunAsync(compiledIL, canonicalHash, sdcNode)` — loads previously compiled IL bytes and invokes `SdcScript.Execute(BaseType sdc)` against a live OM node.
- `ExecuteAsync(scriptText, sdcNode)` — convenience path that compiles and immediately runs.
- `ExecutePrecompiledAsync(scriptText, storedSourceHash, compiledILBase64, sdcNode)` — fast path for documents that already carry compiled bytes plus a stored hash.

The core implementation is intentionally small and explicit:

- `SdcScriptTemplate.cs` wraps the user-authored body into a generated `public static class SdcScript` with `Execute(BaseType sdc)`.
- `SdcScriptCanonicalizer.cs` and `SdcScriptHashInspector.cs` normalize script text and compute the persisted canonical hash.
- `SdcScriptCache.cs` caches compiled scripts by canonical hash so repeated runs do not recompile.
- `SdcScriptLoadContext.cs` loads one compiled assembly at a time and deliberately resolves dependencies from the host load context so the script sees the same `SDC.Schema` types as the caller.
- `ISdcScriptReferenceProvider.cs`, `AppDomainReferenceProvider.cs`, and `PreloadedReferenceProvider.cs` separate the platform-specific "where do Roslyn references come from?" problem from the engine itself.
- `SdcScriptNode.cs` is a temporary storage container for `Source`, `SourceHash`, and `CompiledILBase64`; it is not yet a schema-backed OM node type.

The relationship to the OM is direct, not abstract. The engine API takes `SDC.Schema.BaseType`, and the tests in `SDC.ScriptEngine.Tests` prove that scripts cast that base node to concrete OM types such as `QuestionItemType` and `string_DEtype`, then mutate live properties such as `name`, `title`, `enabled`, `mustImplement`, and response `val`. Those mutations are visible to the caller after the script returns, which is the main reason the load-context design matters.

## Host projects in this repository

### `SDC.ScriptEngine.WpfTest`: desktop comparison host

`SDC.ScriptEngine.WpfTest` is the easiest place to see the intended non-browser host model. It references both `SDC.ScriptEngine` and `SDC.Schema`, builds an interactive desktop window, and exercises three concrete paths:

- compile only,
- run cached IL bytes,
- run the pre-compiled-document path through `ExecutePrecompiledAsync`.

`MainWindow.xaml` and `MainWindow.xaml.cs` also expose the canonical-hash workflow directly, so this project doubles as the clearest comparison point for the browser hosts below.

### `SDC.ScriptEngine.BlazorTest`: browser host with two phases

`SDC.ScriptEngine.BlazorTest` is the main Blazor host in the solution. It references `SDC.ScriptEngine` directly and uses `Pages/Home.razor` as an interactive harness with two distinct modes.

**Phase 1: pre-compiled IL execution**

On startup, `Home.razor` first tries the desktop-style `AppDomainReferenceProvider`. In the browser this is expected to fail for compilation purposes because the loaded assemblies have no file-backed `Location`, so Roslyn receives zero usable references. The page then falls back to a hard-coded desktop-produced Base64 payload plus a stored canonical hash and uses `ExecutePrecompiledAsync` to prove that browser-side execution against a live `QuestionItemType` still works.

**Phase 2: live in-browser compilation**

The same page also includes a second panel that loads assembly references in-browser and then calls `SdcScriptEngine.CompileAsync()` and `RunAsync()` directly. The key host-specific pieces are:

- `WasmReferenceProvider.cs` — fetches assembly bytes from `_framework/`, converts them to Roslyn metadata references, caches them, and exposes progress callbacks.
- `wwwroot/wasm-boot-helpers.js` — extracts the .NET 10 boot manifest embedded inside `dotnet.*.js`.
- `Program.cs` — registers the `HttpClient` base address needed by the reference provider.

So `SDC.ScriptEngine.BlazorTest` is not just a static sample page; it is the repository's concrete "desktop precompile + browser run" host, plus an experimental "browser compile + browser run" host.

### `SDC.ScriptEngine.WasmSpike`: lower-level proof-of-feasibility spike

`SDC.ScriptEngine.WasmSpike` is a more manual, lower-level experiment than `SDC.ScriptEngine.BlazorTest`. It does **not** reference `SDC.ScriptEngine`; instead, `Pages/Spike.razor` performs the critical browser-side steps directly:

1. discover deployed assemblies from the .NET 10 asset manifest,
2. fetch assembly bytes from `_framework/`,
3. create Roslyn metadata references from those bytes,
4. compile a small script in-browser,
5. load the compiled assembly into `SpikeScriptLoadContext`,
6. construct a live `QuestionItemType`,
7. invoke the compiled method by reflection,
8. verify that the mutation is visible on the host side.

It also compiles a second script whose only job is to report `typeof(BaseType).AssemblyQualifiedName`, then compares that value with the host's view of `BaseType`. That makes the spike a direct type-identity proof, not just a user-interface sample.

`SPIKE_RESULTS.md` is important context: it says the project build compiled successfully, but the browser-runtime checklist is still marked "not yet run in browser" and still pending. In other words, this spike demonstrates the intended mechanism in code, but its results document does **not** claim final browser verification yet.

## WebAssembly-specific caveats

### `AppDomainReferenceProvider` is desktop-only

`AppDomainReferenceProvider` works by enumerating the current application domain and wrapping file-backed assemblies in Roslyn references. That is a reasonable desktop default, but it is not a browser reference-discovery strategy. The Blazor host comments document the exact failure mode: the browser runtime can enumerate loaded assemblies, but those assemblies do not expose usable disk paths, so the provider returns zero practical references.

That is why the browser host needs `WasmReferenceProvider` and the manifest-extraction helper instead of trying to reuse the desktop path unchanged.

### Browser-side compilation depends on the deployed assembly format

Both browser projects explicitly set `WasmEnableWebcil=false`, keep the interpreter enabled, and keep publish trimming disabled. In repository terms, the current browser-hosting story assumes:

- raw managed assembly files are available under `_framework/`,
- dynamically loaded script assemblies can execute interpretively,
- reflection-visible `SDC.Schema` members are not trimmed away.

Those settings are acceptable for experiments and internal tooling, but they are also a reminder that production WebAssembly packaging still needs additional work.

### `Task.Run` is not a desktop thread-pool substitute in the browser

`SdcScriptEngine` uses `Task.Run(...)` around compilation and execution so desktop hosts do not block their user-interface thread. Its own comments call out the browser nuance: on single-threaded WebAssembly, `Task.Run(...)` is cooperative scheduling, not a true "run this on another operating-system thread" escape hatch. That difference matters when reading browser timings, diagnosing responsiveness, or reasoning about whether a browser host is actually exercising the same concurrency surface as a multi-threaded desktop test.

### Load-context unloading is best-effort in the browser

`SdcScriptLoadContext` is collectible by design, but its own comments explicitly warn that browser-side unloading may not happen in practice during the current phase. That is a memory-growth caveat, not a correctness caveat: the important behavioral requirement is preserving type identity between host nodes and script-visible nodes.

### WebAssembly thread-safety is a separate, still-open investigation

Do **not** read the completed desktop thread-safety chapter as proof that WebAssembly multi-threading is already solved. The banner at the top of [thread-safety.md](thread-safety.md) is the authoritative scope warning: the completed desktop investigation and the still-open WebAssembly investigation use overlapping thread-safety labels (`TS-#`) for different bugs.

For WebAssembly hosting, the practical guidance is:

- the desktop fixes documented in [thread-safety.md](thread-safety.md) are real, but scoped,
- the browser-side multi-threading backlog remains open,
- the main body of that work currently lives on unmerged `Features/NET10/ILandWASM/*` branches rather than on `Net10Main`.

## How Blazor WebAssembly starts up, and how its multi-threading works

This section explains the underlying browser/runtime mechanics behind the `SDC.ScriptEngine.BlazorAsyncTests.Phase2`
project (merged via PR #43), since this is new, low-level material that the rest of this repo's docs don't otherwise
cover. It is deliberately written with external reference links, because these are browser/runtime behaviors defined
outside this codebase, not SDC-specific concepts.

### Startup sequence (single-threaded, the default for every browser host in this repo)

1. The browser loads `wwwroot/index.html`, which references `_framework/blazor.webassembly.js`.
2. That script fetches `_framework/blazor.boot.json` — a manifest listing the .NET runtime files
   (e.g. `dotnet.native.wasm`) and every application/framework assembly (`.dll`) the app needs, each with an
   integrity hash.
3. The browser downloads those files and instantiates the WebAssembly (WASM) module containing the Mono
   runtime — the open-source .NET runtime implementation used to execute .NET Intermediate Language (IL) inside
   the browser's WASM sandbox.
4. Mono loads the application assemblies and invokes the app's `Program.Main` entry point (in this repo:
   `SDC.ScriptEngine.BlazorAsyncTests.Phase2/Program.cs`, which calls
   `WebAssemblyHostBuilder.CreateDefault(args)`), which then renders the root Razor component.

Official reference: [ASP.NET Core Blazor startup](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup)
and [Blazor hosting models — WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models#blazor-webassembly).

### Why WebAssembly is single-threaded by default, and what multi-threading requires

Browsers run JavaScript/WASM on a single main thread by default. Real multi-threading inside the browser is done with
**Web Workers** (separate JS execution contexts) sharing memory through a **SharedArrayBuffer** — a JavaScript memory
buffer multiple workers can read/write directly. Because `SharedArrayBuffer`'s high-resolution timing can be abused
for Spectre-style side-channel attacks, browsers only expose it on pages that are **cross-origin isolated**, which
requires the server to send two HTTP response headers on every response:

- **`Cross-Origin-Opener-Policy: same-origin`** (COOP) — isolates the browser window from other origins.
- **`Cross-Origin-Embedder-Policy: require-corp`** (COEP) — requires every embedded resource to be same-origin or
  explicitly opt in via CORS/`Cross-Origin-Resource-Policy`.

References: [MDN: SharedArrayBuffer](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/SharedArrayBuffer),
[MDN: Cross-Origin-Opener-Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cross-Origin-Opener-Policy),
[MDN: Cross-Origin-Embedder-Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cross-Origin-Embedder-Policy).

Mono's WebAssembly runtime builds .NET multi-threading on top of this: it emulates the standard POSIX thread
(**pthread**) API using a fixed-size pool of Web Workers, so each .NET `Thread` maps to one browser worker. The pool
size is controlled by the `WasmPThreadPoolSize` MSBuild property. Enabling this whole feature is a single MSBuild
property, `WasmEnableThreads=true`, which requires the `wasm-tools` SDK workload (installed via
`dotnet workload install wasm-tools`; verify with `dotnet workload list`) — that workload is what produces the
thread-enabled native WASM runtime binaries (`dotnet.native.worker.*.mjs`) actually used at run time.

Official reference: [ASP.NET Core Blazor with .NET on Web Workers](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-with-dotnet-on-web-workers).

### How this shows up concretely in `SDC.ScriptEngine.BlazorAsyncTests.Phase2`

- The client project (`SDC.ScriptEngine.BlazorAsyncTests.Phase2.csproj`) sets `WasmEnableThreads=true`,
  `WasmPThreadPoolSize=4`, `WasmEnableInterpreter=true` (interpreter mode, not AOT — see the glossary entry for
  **AOT**), and `WasmEnableWebcil=false`.
- A separate **`SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server`** ASP.NET Core host project exists purely to inject
  the COOP/COEP headers on every response — the built-in Blazor **DevServer** used by `dotnet run` does not send
  them, so multi-threaded WASM only works against a real published build served by this host:
  `dotnet publish` the client, then `cd publish_out && dotnet SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server.dll`.
  In dev mode (`dotnet run`), the Mono native WASM binary does not even have pthread support compiled in, so
  `WasmEnableThreads=true` silently has no effect until you publish.
- The pool size was deliberately reduced from an initial `16` to `4` because Mono's WASM runtime raised a
  pthread-attach assertion failure (`mono-threads-wasm.c:201`) when too many workers tried to attach at boot —
  a Mono/browser worker-pool limit, not an SDC OM bug.
- That same pool-size limit caused a real test-design bug (Cat2/Test6, tracked as TS-5, issue
  [#22](https://github.com/rmoldwin/SDC_ObjectModel/issues/22)): `System.Threading.Barrier(N)` needs `N+1` free
  pool threads (N participants plus one for the post-phase callback), so with `WasmPThreadPoolSize=4` and
  `THREAD_COUNT=4` every pool slot was already occupied and the callback deadlocked. The fix replaced `Barrier`
  with a plain `new Thread()` + `CountdownEvent`, which gives the same "all start together" guarantee without
  consuming a pool slot.

### Where this fits in the bigger thread-safety picture

This is a separate, lower layer than the OM-level concurrency work in [thread-safety.md](thread-safety.md): that
chapter is about locking inside `SDC.Schema` types (`BaseType`, dictionaries, etc.); this section is about the
browser/runtime machinery (Web Workers, `SharedArrayBuffer`, Mono's pthread emulation) that those OM-level fixes
run on top of once hosted in a multi-threaded WASM context. Bugs can originate in either layer, and the `TS-#`
label numbering is shared across both — see the scope warning at the top of that chapter.

## Status

### Browser-scripting backlog

- [#14](https://github.com/rmoldwin/SDC_ObjectModel/issues/14) — add an OM-level event model so scripts attached to `IdentifiedExtensionType` nodes can be triggered by user interaction.
- [#15](https://github.com/rmoldwin/SDC_ObjectModel/issues/15) — verify stored script hashes against a published registry of approved hashes.
- [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16) — support in-browser Roslyn compilation through a WebAssembly-specific reference provider.

### Separate WebAssembly concurrency backlog

These are not "nice to have" browser features; they are the still-open concurrency defects and follow-up work called out by the thread-safety scope caveat and the roadmap:

- [#17](https://github.com/rmoldwin/SDC_ObjectModel/issues/17) — move ambient tree-build state away from `[ThreadStatic]` for asynchronous/browser scenarios.
- [#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20) and [#25](https://github.com/rmoldwin/SDC_ObjectModel/issues/25) — root-cause duplicate-key failures seen under real WebAssembly lock contention.
- [#21](https://github.com/rmoldwin/SDC_ObjectModel/issues/21) — root-cause the array-copy exception seen during concurrent tree comparison on WebAssembly.
- [#23](https://github.com/rmoldwin/SDC_ObjectModel/issues/23) — replace the remaining non-thread-safe reflection binding caches.
- [#24](https://github.com/rmoldwin/SDC_ObjectModel/issues/24) — re-test the earlier deadlock scenario after the cache fix.
- [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28) — investigate run-order-dependent parallel test failures caused by ambient state leaking across pooled workers.
