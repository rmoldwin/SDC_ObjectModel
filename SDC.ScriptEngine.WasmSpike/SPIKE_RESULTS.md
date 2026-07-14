# Wasm Spike Results

**Project:** `SDC.ScriptEngine.WasmSpike`  
**Branch:** `Features/NET10/ILandWASM/WasmSpike`  
**Date:** 2026-06-25  
**Purpose:** De-risking spike — prove Roslyn IL compilation + Assembly.Load works in Blazor WASM with live SDC OM nodes.

---

## Spike Design Summary

The spike creates a Blazor WASM standalone app that:
1. Fetches `blazor.boot.json` to discover the set of deployed DLLs.
2. Fetches each DLL as raw bytes and creates Roslyn `MetadataReference` objects.
3. Compiles a short C# script (`SpikeScript`) to IL using `CSharpCompilation.Emit()`.
4. Loads the IL into a custom `SpikeScriptLoadContext` (collectible ALC).
5. Constructs a live `QuestionItemType` node on the host side.
6. Invokes `SpikeScript.Execute(node)` via reflection.
7. Checks that `node.name` was mutated to `"MUTATED_BY_SCRIPT"` on the calling side.
8. Verifies ALC type identity by comparing `typeof(BaseType).AssemblyQualifiedName` from within the script against the host's value.

---

## Results Checklist

> ⚠️ **Status: NOT YET RUN IN BROWSER** — Build compilation succeeded; browser runtime results pending.
> Fill in this table after running the published app in a browser.

| Check | Result | Notes |
|-------|--------|-------|
| `WasmEnableWebcil=false` — DLLs appear as `.dll` (not `.wasm`) in publish output? | ⬜ TBD | Run `dotnet publish` and inspect `_framework/` folder |
| `MetadataReference.CreateFromImage()` succeeded for all assemblies? | ⬜ TBD | Check "References loaded" count in spike UI |
| `Assembly.Load(byte[])` succeeded? | ⬜ TBD | Check "Assembly loaded" in spike UI |
| Cross-ALC type identity held? (no `InvalidCastException` on cast) | ⬜ TBD | Check "ALC type identity check" in spike UI |
| `node.name` mutated to `"MUTATED_BY_SCRIPT"` on calling side? | ⬜ TBD | Check "node.name after" in spike UI |
| Overall PASS? | ⬜ TBD | Green ✅ in spike UI |

---

## Key Technical Decisions

### 1. `WasmEnableWebcil=false` (Critical)
.NET 8+ defaults to Webcil — a `.wasm`-wrapped PE format. `MetadataReference.CreateFromImage(bytes)` validates a PE header at offset 0 and rejects Webcil bytes with a `BadImageFormatException`. Setting `WasmEnableWebcil=false` forces raw PE/COFF DLLs into `_framework/`, making the bytes Roslyn-parseable.

**Verification:** After `dotnet publish`, the `_framework/` folder should contain `.dll` files (with fingerprint suffix like `SDC.Schema.uate0am3dw.dll`), not `.wasm` files for managed assemblies.

### 2. Custom `AssemblyLoadContext` with `Load() => null`
The `SpikeScriptLoadContext` returns `null` from `Load(AssemblyName)` for all assemblies. This makes the CLR fall back to the Default ALC for all type resolution, ensuring `BaseType` in the script and `BaseType` in the host are the **same CLR type object**. Without this, the script would load a second copy of `SDC.Schema.dll` into the child ALC, making the cross-ALC cast fail with `InvalidCastException`.

### 3. .NET 10 Asset Manifest — NOT blazor.boot.json
**⚠️ IMPORTANT CHANGE FROM .NET 6/7/8:**  
In .NET 10, Blazor WASM **no longer deploys `blazor.boot.json`**. The asset manifest is now embedded as a JSON literal inside `dotnet.js` (fingerprinted, e.g., `dotnet.pf18hs7ni0.js`) between the markers:
```javascript
/*json-start*/{ "mainAssemblyName": "...", "resources": { "assembly": [...], "coreAssembly": [...] } }/*json-end*/
```

Each assembly entry has:
```json
{ "virtualPath": "SDC.Schema.dll", "name": "SDC.Schema.uate0am3dw.dll", "hash": "sha256-...", "cache": "force-cache" }
```

The spike uses a JavaScript helper (`wwwroot/spike-helpers.js`) that:
1. Finds the `<link rel="preload">` element for `dotnet.*.js` in the DOM.
2. Re-fetches the fingerprinted URL.
3. Extracts the JSON block and returns it to C# via `IJSRuntime`.

The C# code then parses `resources.assembly` + `resources.coreAssembly` to enumerate all fingerprinted DLL names and fetch them.

### 4. No `PublishTrimmed` / No AOT
- `PublishTrimmed=false` — trimming removes types accessed only via reflection. Since Roslyn scripts call SDC OM types dynamically, trimming would silently remove needed members.
- `RunAOTCompilation=false` + `WasmEnableInterpreter=true` — dynamically loaded IL (from `Assembly.Load`) cannot be AOT-compiled ahead of time; it must be JIT/interpreted at runtime. The interpreter must be present to execute it.

---

## Unexpected Obstacles

> Fill in after running in browser.

| Obstacle | Resolution |
|----------|------------|
| _(none yet)_ | — |

---

## Implications for Main Plan

> Fill in after results are confirmed.

- If cross-ALC cast succeeds: The `Load() => null` pattern is validated and should be used in the production ScriptEngine ALC.
- If `MetadataReference.CreateFromImage` fails: Verify `WasmEnableWebcil=false` is in effect; check publish output.
- If `Assembly.Load` succeeds but invoke fails: Check interpreter presence; confirm `WasmEnableInterpreter=true` and `RunAOTCompilation=false`.
- SDC.Schema targets `net7.0` while the spike targets `net10.0`. This is supported by .NET's multi-targeting compatibility — net10.0 can consume net7.0 libraries.

---

## Build Verification

Build the spike to verify compilation succeeds before running in browser:

```bash
cd SDC.ScriptEngine.WasmSpike
dotnet build
dotnet publish -c Release
```

Check publish output at `bin/Release/net10.0/publish/wwwroot/_framework/` — all managed assemblies should have `.dll` extension (not `.wasm`) when `WasmEnableWebcil=false` is in effect.
