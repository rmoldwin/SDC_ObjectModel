// ============================================================================
// PreloadedReferenceProvider.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// PURPOSE
// -------
// This is a trivial adapter that bridges the gap between asynchronous
// reference loading (required on Blazor WASM) and the synchronous
// ISdcScriptReferenceProvider interface that SdcScriptEngine expects.
//
// HOW IT IS USED
// --------------
// 1. A platform-specific async loader (e.g., WasmReferenceProvider in the
//    Blazor host project) fetches DLL bytes and creates MetadataReference
//    objects asynchronously.
// 2. Once loading is complete the caller wraps the list in a
//    PreloadedReferenceProvider, which exposes it via the synchronous
//    ISdcScriptReferenceProvider.GetReferences() method.
// 3. A SdcScriptEngine is constructed with the PreloadedReferenceProvider.
//    From that point on, every CompileAsync() call retrieves references in
//    O(1) (a field read) — no further I/O occurs.
//
// DESKTOP NOTE
// ------------
// On desktop you would typically use AppDomainReferenceProvider instead.
// PreloadedReferenceProvider is useful on desktop only when you want to
// tightly control the exact set of references (e.g., integration tests that
// verify the interface contract via a known reference list).

using Microsoft.CodeAnalysis;

namespace SDC.ScriptEngine;

/// <summary>
/// An <see cref="ISdcScriptReferenceProvider"/> implementation that holds a
/// pre-loaded, already-resolved set of <see cref="MetadataReference"/> objects
/// and returns them synchronously.
/// </summary>
/// <remarks>
/// <para>
/// This adapter is the recommended solution for Blazor WASM, where assembly
/// metadata references must be fetched asynchronously via <c>HttpClient</c>
/// before compilation can begin.  The async loader does the heavy lifting;
/// once loading is complete the result is stored here so that
/// <see cref="SdcScriptEngine"/> — which calls
/// <see cref="GetReferences"/> synchronously — can function without
/// modification.
/// </para>
/// <para>
/// Typical Blazor WASM usage:
/// <code>
/// // 1. Async load in the component
/// var refs = await wasmRefProvider.GetReferencesAsync();
///
/// // 2. Wrap for synchronous interface compatibility
/// var preloaded = new PreloadedReferenceProvider(refs);
///
/// // 3. Create engine — all subsequent CompileAsync() calls are instant
/// var engine = new SdcScriptEngine(preloaded);
/// </code>
/// </para>
/// <para>
/// Desktop/test usage (override the reference list with a known subset):
/// <code>
/// var desktopRefs = new AppDomainReferenceProvider().GetReferences();
/// var preloaded = new PreloadedReferenceProvider(desktopRefs);
/// var engine = new SdcScriptEngine(preloaded);
/// </code>
/// </para>
/// </remarks>
public sealed class PreloadedReferenceProvider : ISdcScriptReferenceProvider
{
    private readonly IReadOnlyList<MetadataReference> _references;

    /// <summary>
    /// Initializes a new <see cref="PreloadedReferenceProvider"/> with the
    /// given set of already-loaded metadata references.
    /// </summary>
    /// <param name="references">
    /// The pre-loaded metadata references to return from
    /// <see cref="GetReferences"/>.  Must not be null; may be empty (though
    /// an empty list will produce Roslyn "type not found" errors at compile time).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="references"/> is null.
    /// </exception>
    public PreloadedReferenceProvider(IEnumerable<MetadataReference> references)
    {
        if (references is null) throw new ArgumentNullException(nameof(references));
        // Materialise into a list once so GetReferences() is a pure O(1) field read.
        _references = references as IReadOnlyList<MetadataReference>
                      ?? new List<MetadataReference>(references).AsReadOnly();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns the list supplied to the constructor.  This call is O(1) — no
    /// I/O or computation occurs.
    /// </remarks>
    public IReadOnlyList<MetadataReference> GetReferences() => _references;
}
