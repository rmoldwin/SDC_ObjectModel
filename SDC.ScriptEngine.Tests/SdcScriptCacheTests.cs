// ============================================================================
// SdcScriptCacheTests.cs
// SDC.ScriptEngine.Tests — compilation cache behavioral tests
// ============================================================================
//
// The engine maintains an in-process cache keyed by canonical hash.  A cache
// hit skips Roslyn compilation entirely, reusing the cached MethodInfo.
// These tests verify:
//
//   - Repeated execution of the same script only compiles ONCE.
//   - Different scripts each get their own cache entry.
//   - The cache survives many calls without eviction (no TTL in Phase 1).
//   - Scripts differing only in whitespace share a single cache entry because
//     the canonical hash — not the raw source text — is the cache key.
//
// Measurement technique: the CompileCountingProvider spy (in TestHelper) wraps
// the AppDomainReferenceProvider and increments a counter each time
// GetReferences() is called.  Because GetReferences() is called exactly once
// per Roslyn compilation, the counter is a reliable proxy for cache misses.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

[TestClass]
public class SdcScriptCacheTests
{
    // ── 1. Same script twice → one compile ───────────────────────────────────

    [TestMethod]
    public async Task ExecuteAsync_SameScriptTwice_OnlyCompilesOnce()
    {
        var (engine, getCompileCount) = ScriptEngineTestHelper.CreateEngineWithCompileCounter();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();
        const string script = "var x = 1;";

        // Execute the same script twice in sequence.
        var r1 = await engine.ExecuteAsync(script, q);
        var r2 = await engine.ExecuteAsync(script, q);

        // Both executions must succeed.
        Assert.IsTrue(r1.Success && r2.Success,
            "Both executions of a valid script must succeed.");

        // GetReferences should have been called exactly once — the second
        // execution must have been served from the cache without recompiling.
        Assert.AreEqual(1, getCompileCount(),
            "The same script must only be compiled once; second call must hit the cache.");
    }

    // ── 2. Different scripts → separate cache entries ─────────────────────────

    [TestMethod]
    public async Task ExecuteAsync_DifferentScripts_ProduceSeparateCacheEntries()
    {
        var (engine, getCompileCount) = ScriptEngineTestHelper.CreateEngineWithCompileCounter();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string scriptA = "var a = 1;";
        const string scriptB = "var b = 2;";

        await engine.ExecuteAsync(scriptA, q);
        await engine.ExecuteAsync(scriptB, q);

        // Two distinct scripts have two distinct canonical hashes and therefore
        // two separate cache entries, requiring two Roslyn compilations.
        Assert.AreEqual(2, getCompileCount(),
            "Two different scripts must each trigger one compilation.");
    }

    // ── 3. Cache survives many calls ──────────────────────────────────────────

    [TestMethod]
    public async Task ExecuteAsync_CacheSurvivesMultipleCalls()
    {
        var (engine, getCompileCount) = ScriptEngineTestHelper.CreateEngineWithCompileCounter();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();
        const string script = "var n = 42;";

        // Run the same script five times.
        for (int i = 0; i < 5; i++)
            await engine.ExecuteAsync(script, q);

        // All five calls must have been served from the cache after the first
        // compilation.  There is no TTL or LRU eviction in Phase 1.
        Assert.AreEqual(1, getCompileCount(),
            "Five calls to the same script must result in exactly one compilation.");
    }

    // ── 4. Whitespace-diff scripts share one cache entry ──────────────────────

    [TestMethod]
    public async Task ExecuteAsync_ScriptsDifferingOnlyInWhitespace_ShareCacheEntry()
    {
        // The cache key is the CANONICAL hash, not the raw source text.
        // Two scripts with different whitespace but identical tokens produce
        // the same canonical hash and therefore share a single cache entry.
        // This means reformatting a script never causes a spurious recompile.
        var (engine, getCompileCount) = ScriptEngineTestHelper.CreateEngineWithCompileCounter();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string compact = "var x = 1;";
        const string spacious = "var  x  =  1 ;";  // same tokens, different spacing

        var r1 = await engine.ExecuteAsync(compact, q);
        var r2 = await engine.ExecuteAsync(spacious, q);

        Assert.IsTrue(r1.Success && r2.Success,
            "Both whitespace variants must execute successfully.");

        // Only ONE compilation should have occurred because both scripts
        // canonicalize to the same token stream and the same hash.
        Assert.AreEqual(1, getCompileCount(),
            "Scripts differing only in whitespace must share one cache entry.");
    }
}
