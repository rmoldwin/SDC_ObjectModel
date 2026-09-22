# SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server

## What this project is

`SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server` is a thin ASP.NET Core host whose only job is to
serve the `SDC.ScriptEngine.BlazorAsyncTests.Phase2` Blazor WebAssembly (WASM) client with the
Cross-Origin-Opener-Policy (COOP) and Cross-Origin-Embedder-Policy (COEP) response headers that
`SharedArrayBuffer` requires. Without those headers, the Phase 2 client's `WasmEnableThreads=true`
build cannot actually run multi-threaded in the browser — it is not optional plumbing, it is the
reason this project exists.

## Basic architecture

- `SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server.csproj` targets `net10.0` via the
  `Microsoft.NET.Sdk.Web` SDK and references the Phase 2 WASM client project so
  `UseBlazorFrameworkFiles()` can serve its build output.
- `Program.cs` sets the COOP/COEP headers **before** `UseBlazorFrameworkFiles()`/`UseStaticFiles()`
  — middleware order is load-bearing here, since the headers must apply to `_framework/*.wasm` and
  `blazor.boot.json` as well as the page itself.
- For real multi-threaded WASM, the client should be published and this server run against the
  published output (`dotnet publish ... -c Release -o publish_out`, then run the server DLL from
  that output folder) rather than `dotnet run`, per the comment at the top of `Program.cs`.

## State of completion

- Minimal, single-purpose host — one `Program.cs` file, no additional application logic. Complete
  for its narrow purpose (serving Phase 2 with the correct isolation headers).
- See `SDC.ScriptEngine.BlazorAsyncTests.Phase2`'s README for the actual test scope this host
  enables, and [../..docs/architecture/thread-safety.md](../..docs/architecture/thread-safety.md)
  for the WASM thread-safety findings produced using it.
