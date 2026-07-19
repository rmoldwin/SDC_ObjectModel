// ============================================================================
// SdcScriptEngineStressTests.cs
// SDC.ScriptEngine.Tests — stress / load tests
// ============================================================================
//
// Stress tests are exempt from the 1-second unit test rule.
// They intentionally exercise load, concurrency, and memory behavior.
// Run with: dotnet test --filter TestCategory=Stress

using SDC.Schema;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

/// <summary>
/// Stress and load tests for <see cref="SdcScriptEngine"/>.
/// Exercises concurrency and memory behavior under load.
/// </summary>
[TestClass]
public class SdcScriptEngineStressTests
{
    [TestInitialize]
    public void Init() => BaseType.ResetLastTopNode();

    [TestCleanup]
    public void Cleanup() => BaseType.ResetLastTopNode();

    // ── 1. Concurrent compilation stress test ────────────────────────────────

    /// <summary>
    /// Compiles 20 distinct scripts concurrently via <c>Task.WhenAll</c> and
    /// asserts that all succeed, each produces a unique canonical hash, and
    /// each has non-empty IL bytes.
    /// </summary>
    // Stress test — intentionally runs multiple compilations. Expected duration: 5-15s. Exempt from the 1-second per-test rule.
    [TestMethod]
    [TestCategory("Stress")]
    public async Task ConcurrentCompilation_ManyScripts_AllSucceed()
    {
        const int N = 20;

        // Each engine uses the AppDomainReferenceProvider (desktop path).
        // A single shared engine is correct here — the engine's internal cache
        // and ConcurrentDictionary are thread-safe by design.
        var engine = ScriptEngineTestHelper.CreateEngine();

        // Build N distinct scripts by embedding a unique literal in each one.
        // The distinct string guarantees a unique canonical hash per script.
        var scripts = Enumerable.Range(0, N)
            .Select(i => $"var marker{i} = \"script_variant_{i}\"; // unique body")
            .ToArray();

        // Fire all compilations concurrently.
        var compileTasks = scripts
            .Select(s => engine.CompileAsync(s))
            .ToArray();

        var results = await Task.WhenAll(compileTasks);

        // --- all compilations must succeed ---
        for (int i = 0; i < N; i++)
        {
            Assert.IsTrue(results[i].Success,
                $"Script {i} must compile successfully. " +
                $"Errors: {string.Join("; ", results[i].Diagnostics.Select(d => d.Message))}");
        }

        // --- each result must have non-empty IL bytes ---
        for (int i = 0; i < N; i++)
        {
            Assert.IsNotNull(results[i].CompiledIL,
                $"Script {i} must produce non-null IL bytes.");
            Assert.IsTrue(results[i].CompiledIL!.Length > 0,
                $"Script {i} must produce non-empty IL bytes.");
        }

        // --- each result must have a distinct canonical hash (no cross-contamination) ---
        var hashes = results.Select(r => r.CanonicalHash).ToArray();
        var distinctCount = hashes.Distinct().Count();
        Assert.AreEqual(N, distinctCount,
            $"All {N} scripts must produce unique canonical hashes. " +
            $"Found only {distinctCount} distinct hashes — possible cross-contamination.");
    }

    // ── 2. ALC memory reclamation under load ─────────────────────────────────

    /// <summary>
    /// Compiles 10 unique scripts sequentially, forces GC, and asserts that
    /// total managed memory growth stays below 50 MB.
    /// </summary>
    /// <remarks>
    /// ALCs stay alive in the cache by design (the cache holds a reference
    /// to each <see cref="SdcScriptLoadContext"/> so that cached entries
    /// remain invocable without recompilation).  However, Roslyn's large
    /// intermediate objects — syntax trees, bound trees, and the
    /// CSharpCompilation object itself — are not referenced by the cache
    /// entry and should be collected after each compilation.  This test
    /// verifies that the memory footprint stays bounded under repeated loads.
    /// </remarks>
    // Stress test — measures memory growth from repeated compilations. Exempt from the 1-second per-test rule.
    [TestMethod]
    [TestCategory("Stress")]
    public async Task AlcUnload_AfterCompile_MemoryReclaimed()
    {
        var engine = ScriptEngineTestHelper.CreateEngine();

        // Use a timestamp-based suffix to guarantee these scripts are NOT
        // already in a shared cache (each test run gets fresh unique scripts).
        long runId = DateTime.UtcNow.Ticks;

        // Warm up: compile one script to load Roslyn infrastructure so the
        // baseline measurement is not penalized by the first-time JIT cost.
        await engine.CompileAsync(
            $"var warmup_{runId} = \"warmup\";");

        // Force GC to settle memory before taking the baseline.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memBefore = GC.GetTotalMemory(true);

        // Compile 10 more unique scripts sequentially (not concurrently, so
        // each intermediate Roslyn object is eligible for collection before
        // the next compilation starts — this is the intended reclamation window).
        for (int i = 0; i < 10; i++)
        {
            var result = await engine.CompileAsync(
                $"var load_{runId}_{i} = \"memory_test_script_{i}\"; // unique");

            // Each compilation must succeed; a failure here is a test setup error.
            Assert.IsTrue(result.Success,
                $"Sequential compile {i} must succeed during memory test. " +
                $"Errors: {string.Join("; ", result.Diagnostics.Select(d => d.Message))}");
        }

        // Force GC three times to give finalizers and the GC multiple passes
        // to collect Roslyn's intermediate compilation objects.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memAfter = GC.GetTotalMemory(true);
        long growthBytes = memAfter - memBefore;
        const long maxGrowthBytes = 50L * 1024 * 1024; // 50 MB

        Assert.IsTrue(growthBytes < maxGrowthBytes,
            $"Managed memory growth after 10 sequential compilations + GC must be " +
            $"< 50 MB. Actual growth: {growthBytes / (1024 * 1024.0):F1} MB. " +
            $"This may indicate that large Roslyn intermediate objects are being " +
            $"kept alive unexpectedly (e.g., rooted by a static or a closure).");
    }
}
