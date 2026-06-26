// ============================================================================
// SdcScriptCache.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// This file contains two internal types:
//
//   SdcScriptEntry  — an immutable record that bundles the ALC and the resolved
//                     Execute MethodInfo for a single compiled script.
//
//   SdcScriptCache  — a thread-safe dictionary from canonical hash → entry,
//                     preventing redundant recompilation of the same script.
//
// CACHE DESIGN
// ------------
// Key:   canonical SHA-256 hash (from SdcScriptCanonicalizer.ComputeCanonicalHash).
//        Two scripts with the same tokens but different formatting produce the
//        SAME hash and therefore share one cache entry.
//
// Value: SdcScriptEntry, which keeps the SdcScriptLoadContext alive.
//        Without holding a reference to the ALC, the GC is free to collect it
//        and unload the script assembly, making the cached MethodInfo invalid.
//
// ONE ALC PER DISTINCT HASH
// --------------------------
// We create exactly ONE ALC per canonical hash.  If the engine is asked to
// compile the same script 1,000 times (e.g., applied to 1,000 SDC tree
// nodes), it compiles once and reuses the cached MethodInfo for all 1,000
// invocations.  This avoids both the Roslyn compilation cost and ALC
// accumulation from repeated loads.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SDC.ScriptEngine;

/// <summary>
/// Holds a loaded script assembly and the resolved <c>Execute</c> method
/// for fast, repeated invocation without recompilation.
/// </summary>
/// <param name="LoadContext">
/// The custom <see cref="SdcScriptLoadContext"/> that owns the script assembly.
/// Must be kept alive (referenced) as long as the script may be invoked;
/// releasing all references allows the GC to unload the ALC (desktop only).
/// </param>
/// <param name="ExecuteMethod">
/// The <see cref="MethodInfo"/> for <c>SdcScript.Execute(BaseType sdc)</c>
/// as resolved via reflection after assembly load.
/// Cached here to avoid repeated <c>GetMethod</c> calls on every invocation.
/// </param>
/// <param name="ILBytes">
/// The raw IL bytes produced by Roslyn for this script.
/// Stored so that a cache-hit path in <c>CompileAsync</c> can return the
/// original IL in <c>SdcScriptCompileResult.CompiledIL</c> without
/// recompiling or re-emitting.
/// </param>
internal sealed record SdcScriptEntry(
    SdcScriptLoadContext LoadContext,
    MethodInfo ExecuteMethod,
    byte[] ILBytes);

/// <summary>
/// Thread-safe in-memory cache of compiled script assemblies, keyed by
/// canonical SHA-256 hash.
/// </summary>
/// <remarks>
/// <para>
/// Uses <see cref="ConcurrentDictionary{TKey,TValue}"/> for lock-free reads
/// and safe concurrent writes, suitable for use from multiple threads
/// (e.g., parallel MSTest runs or background Blazor tasks).
/// </para>
/// <para>
/// Cache entries are never evicted in Phase 1.  The number of distinct scripts
/// in a typical SDC deployment is small (dozens, not millions), so unbounded
/// growth is acceptable.  Phase 2 can add LRU eviction if needed.
/// </para>
/// </remarks>
internal sealed class SdcScriptCache
{
    // ConcurrentDictionary<hash, entry> — hash is the 64-char lowercase hex
    // canonical SHA-256.  Thread-safe for all read and write operations.
    private readonly ConcurrentDictionary<string, SdcScriptEntry> _entries = new();

    /// <summary>
    /// Attempts to retrieve a cached entry by canonical hash.
    /// </summary>
    /// <param name="canonicalHash">
    /// The 64-character lowercase hex SHA-256 canonical hash.
    /// </param>
    /// <param name="entry">
    /// Set to the cached entry if found; <see langword="null"/> otherwise.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the hash was found in the cache.
    /// </returns>
    public bool TryGet(string canonicalHash, [NotNullWhen(true)] out SdcScriptEntry? entry)
        => _entries.TryGetValue(canonicalHash, out entry);

    /// <summary>
    /// Stores a new entry in the cache.  If an entry with the same hash
    /// already exists (from a concurrent compile), the existing entry wins
    /// and the provided entry is silently discarded.
    /// </summary>
    /// <param name="canonicalHash">Cache key (canonical SHA-256 hex).</param>
    /// <param name="entry">The entry to store.</param>
    public void Store(string canonicalHash, SdcScriptEntry entry)
    {
        // TryAdd is a no-op if the key already exists — this handles the
        // rare race where two threads compile the same script concurrently.
        // The first writer wins; the second writer's entry (and its ALC) will
        // be collected by the GC when no references remain.
        _entries.TryAdd(canonicalHash, entry);
    }

    /// <summary>
    /// Returns the number of distinct scripts currently in the cache.
    /// Useful for diagnostics and tests.
    /// </summary>
    public int Count => _entries.Count;
}
