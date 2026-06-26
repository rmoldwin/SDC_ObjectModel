// ============================================================================
// WasmReferenceProvider.cs
// SDC.ScriptEngine.BlazorTest — Blazor WASM reference provider
// ============================================================================
//
// OVERVIEW
// --------
// WasmReferenceProvider is the Blazor WASM implementation of
// ISdcScriptReferenceProvider.  Unlike the desktop AppDomainReferenceProvider
// (which enumerates AppDomain.GetAssemblies()), WASM assemblies have no
// on-disk Location path.  Instead this provider:
//
//   1. Calls the getWasmBootManifest() JavaScript helper (defined in
//      wwwroot/wasm-boot-helpers.js) to extract the .NET 10 boot manifest
//      embedded inside dotnet.*.js.  The manifest lists every assembly with
//      its fingerprinted filename (e.g. "SDC.Schema.uate0am3dw.dll").
//
//   2. For each entry in the manifest's "assembly" and "coreAssembly" arrays,
//      fetches the raw DLL bytes via HttpClient from the _framework/ path.
//      These are raw PE/COFF bytes because WasmEnableWebcil=false is set in
//      the .csproj (required for MetadataReference.CreateFromImage to work).
//
//   3. Creates a MetadataReference for each successfully-fetched DLL.
//
//   4. Caches the result so subsequent CompileAsync() calls are instant.
//
// .NET 10 BOOT MANIFEST NOTE
// --------------------------
// Prior to .NET 10, Blazor deployed a blazor.boot.json file at a well-known
// URL.  In .NET 10 this file no longer exists.  The manifest is now embedded
// as a JSON literal inside the fingerprinted dotnet.*.js file between markers:
//
//   /*json-start*/{ ... }/*json-end*/
//
// The JavaScript helper in wasm-boot-helpers.js handles the extraction.
//
// GRACEFUL FALLBACK
// -----------------
// If the manifest cannot be found (e.g., the JS helper fails, the markers
// are absent from the JS file, or the preload link is not yet in the DOM),
// WasmReferenceProvider falls back to a hardcoded list of critical DLL names
// (non-fingerprinted).  These non-fingerprinted names may not match what the
// SDK deployed if content-hashing is in use, so most fetches will 404 — only
// a minimal subset of assemblies will load.  Scripts requiring SDC.Schema or
// Roslyn will likely fail to compile.  This fallback is a last resort.
//
// SYNCHRONOUS INTERFACE COMPATIBILITY
// ------------------------------------
// GetReferences() (the ISdcScriptReferenceProvider method) throws
// NotSupportedException because WASM reference loading is inherently async.
// Call GetReferencesAsync() first, then wrap the result in a
// PreloadedReferenceProvider and pass that to SdcScriptEngine:
//
//   var refs    = await wasmProvider.GetReferencesAsync();
//   var preload = new PreloadedReferenceProvider(refs);
//   var engine  = new SdcScriptEngine(preload);
//
// PROGRESS REPORTING
// ------------------
// Subscribe to the LoadingProgress event before calling GetReferencesAsync()
// to receive (loaded, total) updates as each DLL is fetched:
//
//   provider.LoadingProgress += (loaded, total) => {
//       // Update progress bar or counter in the UI
//       StateHasChanged();
//   };

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.JSInterop;

namespace SDC.ScriptEngine.BlazorTest;

/// <summary>
/// Blazor WASM implementation of <see cref="ISdcScriptReferenceProvider"/>.
/// Fetches assembly metadata references by downloading raw PE/COFF DLL bytes
/// from the <c>_framework/</c> virtual directory via <see cref="HttpClient"/>,
/// using the .NET 10 boot manifest to discover fingerprinted DLL filenames.
/// </summary>
/// <remarks>
/// <para>
/// This class is designed for Blazor WASM only.  On desktop runtimes, use
/// <see cref="AppDomainReferenceProvider"/> instead.
/// </para>
/// <para>
/// <b>Usage pattern</b>:
/// <code>
/// // In the Blazor component OnInitializedAsync or on button click:
/// provider.LoadingProgress += (loaded, total) => { _progress = $"{loaded}/{total}"; StateHasChanged(); };
/// var refs    = await provider.GetReferencesAsync();
/// var preload = new PreloadedReferenceProvider(refs);
/// var engine  = new SdcScriptEngine(preload);
/// </code>
/// </para>
/// <para>
/// <b>Requirements</b>:
/// <list type="bullet">
///   <item><c>WasmEnableWebcil=false</c> — DLLs must be raw PE/COFF so that
///         <c>MetadataReference.CreateFromImage()</c> can read them.</item>
///   <item><c>WasmEnableInterpreter=true</c> — Required for dynamically
///         loaded IL assemblies to execute at runtime.</item>
///   <item>The JS helper <c>getWasmBootManifest</c> must be registered
///         in <c>wwwroot/wasm-boot-helpers.js</c>.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class WasmReferenceProvider : ISdcScriptReferenceProvider
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private IReadOnlyList<MetadataReference>? _cached;

    /// <summary>
    /// Fired each time one DLL has been fetched and converted to a
    /// <see cref="MetadataReference"/>.  Parameters are
    /// <c>(int loaded, int total)</c> — the number loaded so far and the
    /// total number of assemblies discovered in the boot manifest.
    /// </summary>
    /// <remarks>
    /// Subscribe before calling <see cref="GetReferencesAsync"/> to receive
    /// incremental progress updates suitable for a progress bar or counter.
    /// In Blazor WASM (single-threaded), it is safe to call
    /// <c>StateHasChanged()</c> directly from this handler.
    /// </remarks>
    public event Action<int, int>? LoadingProgress;

    /// <summary>
    /// Creates a new <see cref="WasmReferenceProvider"/>.
    /// </summary>
    /// <param name="httpClient">
    /// An <see cref="HttpClient"/> whose <c>BaseAddress</c> is set to the
    /// Blazor app origin (typically registered as a scoped service in
    /// <c>Program.cs</c> with <c>BaseAddress = builder.HostEnvironment.BaseAddress</c>).
    /// </param>
    /// <param name="jsRuntime">
    /// The Blazor <see cref="IJSRuntime"/> for calling the JavaScript boot
    /// manifest helper.
    /// </param>
    public WasmReferenceProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsRuntime  = jsRuntime  ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    /// <summary>
    /// Not supported in WASM — reference loading requires async I/O.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    /// <remarks>
    /// Call <see cref="GetReferencesAsync"/> instead, then wrap the result
    /// in a <see cref="PreloadedReferenceProvider"/> and pass it to
    /// <see cref="SdcScriptEngine"/>.
    /// </remarks>
    public IReadOnlyList<MetadataReference> GetReferences()
        => throw new NotSupportedException(
            "Use GetReferencesAsync() in WASM — reference loading requires async HttpClient I/O.");

    /// <summary>
    /// Asynchronously discovers all deployed assemblies from the .NET 10
    /// boot manifest, fetches their raw DLL bytes from <c>_framework/</c>,
    /// and returns a list of <see cref="MetadataReference"/> objects for
    /// Roslyn compilation.
    /// </summary>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// A read-only list of <see cref="MetadataReference"/> objects —
    /// one per successfully-fetched DLL.  DLLs that return 404 or fail
    /// for any reason are silently skipped (a warning is logged to the
    /// browser console).
    /// </returns>
    /// <remarks>
    /// Results are cached after the first call.  Subsequent calls return the
    /// cached list immediately without any I/O.
    /// </remarks>
    public async Task<IReadOnlyList<MetadataReference>> GetReferencesAsync(
        CancellationToken ct = default)
    {
        if (_cached is not null)
            return _cached;

        // ── Step 1: Discover fingerprinted DLL names ──────────────────────
        var assemblies = await DiscoverAssembliesAsync(ct).ConfigureAwait(false);

        // ── Step 2: Fetch each DLL and build MetadataReference list ──────
        var refs  = new List<MetadataReference>(assemblies.Count);
        int total = assemblies.Count;
        int loaded = 0;

        foreach (var (_, name) in assemblies)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync($"_framework/{name}", ct)
                                            .ConfigureAwait(false);
                // CreateFromImage requires raw PE/COFF bytes.
                // WasmEnableWebcil=false guarantees the bytes start with "MZ".
                refs.Add(MetadataReference.CreateFromImage(bytes));
                loaded++;
                LoadingProgress?.Invoke(loaded, total);
            }
            catch (Exception ex)
            {
                // Log to browser console and continue — best-effort loading.
                // Common causes: 404 for assemblies not deployed to _framework/,
                // or bad image bytes if WasmEnableWebcil was accidentally enabled.
                Console.WriteLine(
                    $"[WasmReferenceProvider] Skipping {name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        _cached = refs.AsReadOnly();
        return _cached;
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    /// <summary>
    /// Calls the <c>getWasmBootManifest</c> JS helper to retrieve the .NET 10
    /// boot manifest and returns the list of (virtualPath, fingerprinted name)
    /// pairs for all managed assemblies.  Falls back to a hardcoded list if
    /// the manifest cannot be obtained.
    /// </summary>
    private async Task<IReadOnlyList<(string virtualPath, string name)>> DiscoverAssembliesAsync(
        CancellationToken ct)
    {
        bool manifestLoaded = false;
        JsonElement manifest = default;

        try
        {
            // InvokeAsync<JsonElement> returns a JsonElement with ValueKind=Null
            // when the JS function returns null (e.g., markers not found).
            manifest = await _jsRuntime
                .InvokeAsync<JsonElement>("getWasmBootManifest", ct)
                .ConfigureAwait(false);

            manifestLoaded = manifest.ValueKind != JsonValueKind.Null
                          && manifest.ValueKind != JsonValueKind.Undefined;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WasmReferenceProvider] getWasmBootManifest failed: {ex.GetType().Name}: {ex.Message}. " +
                "Using hardcoded fallback assembly list.");
        }

        if (!manifestLoaded)
        {
            Console.WriteLine(
                "[WasmReferenceProvider] Boot manifest unavailable — " +
                "falling back to non-fingerprinted DLL names. " +
                "Most fetches may 404 if the SDK used fingerprinted names.");
            return GetHardcodedFallback();
        }

        return ParseManifestAssemblies(manifest);
    }

    /// <summary>
    /// Parses the <c>resources.assembly</c> and <c>resources.coreAssembly</c>
    /// arrays from the .NET 10 boot manifest JSON.
    /// </summary>
    /// <remarks>
    /// Each entry in these arrays has the shape:
    /// <code>
    /// { "virtualPath": "SDC.Schema.dll", "name": "SDC.Schema.uate0am3dw.dll", ... }
    /// </code>
    /// where <c>name</c> is the fingerprinted filename deployed to
    /// <c>_framework/</c>.
    /// </remarks>
    private static IReadOnlyList<(string virtualPath, string name)> ParseManifestAssemblies(
        JsonElement manifest)
    {
        var list = new List<(string, string)>();

        if (!manifest.TryGetProperty("resources", out var resources))
            return list;

        // Enumerate both managed assembly sections.
        foreach (var sectionName in new[] { "assembly", "coreAssembly" })
        {
            if (!resources.TryGetProperty(sectionName, out var section))
                continue;
            if (section.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var entry in section.EnumerateArray())
            {
                string? virtualPath = null;
                string? name = null;

                if (entry.TryGetProperty("virtualPath", out var vpEl))
                    virtualPath = vpEl.GetString();
                if (entry.TryGetProperty("name", out var nameEl))
                    name = nameEl.GetString();

                if (!string.IsNullOrEmpty(virtualPath) && !string.IsNullOrEmpty(name))
                    list.Add((virtualPath!, name!));
            }
        }

        return list;
    }

    /// <summary>
    /// Returns a minimal hardcoded list of critical assembly names that are
    /// required for SDC script compilation.  Used only when the boot manifest
    /// is unavailable.
    /// </summary>
    /// <remarks>
    /// These names are <em>non-fingerprinted</em>.  In a standard .NET 10
    /// publish the SDK appends a content hash to each filename (e.g.,
    /// <c>SDC.Schema.uate0am3dw.dll</c>), so fetching these non-fingerprinted
    /// names will likely fail with 404.  The fallback is a last resort for
    /// environments where dotnet.*.js is not yet in the DOM or the sentinel
    /// markers were stripped by a CDN.
    /// </remarks>
    private static IReadOnlyList<(string virtualPath, string name)> GetHardcodedFallback()
        => new (string, string)[]
        {
            ("SDC.Schema.dll",                   "SDC.Schema.dll"),
            ("System.Private.CoreLib.dll",        "System.Private.CoreLib.dll"),
            ("System.Runtime.dll",               "System.Runtime.dll"),
            ("System.Linq.dll",                  "System.Linq.dll"),
            ("System.Collections.dll",           "System.Collections.dll"),
            ("Microsoft.CodeAnalysis.dll",        "Microsoft.CodeAnalysis.dll"),
            ("Microsoft.CodeAnalysis.CSharp.dll", "Microsoft.CodeAnalysis.CSharp.dll"),
        };
}
