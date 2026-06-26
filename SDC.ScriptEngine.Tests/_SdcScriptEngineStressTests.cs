// ============================================================================
// _SdcScriptEngineStressTests.cs
// SDC.ScriptEngine.Tests — stress/performance test stubs
// ============================================================================
//
// STUB FILE CONVENTION (matches project pattern):
//   - Filename starts with underscore (_) to sort it last in the file list.
//   - Class name starts with underscore.
//   - Method names start with underscore.
//   - Method bodies are empty or contain only a no-op assertion.
//
// These stubs serve as:
//   1. DOCUMENTATION: they name the scenarios that SHOULD be load-tested.
//   2. PLACEHOLDERS: they will be fleshed out when a perf test run is needed.
//   3. SAFE DEFAULTS: empty bodies mean they always pass, never block CI.
//
// WHY STUBS AND NOT REAL TESTS?
// -------------------------------
// Stress tests are non-deterministic in CI (shared runners, variable CPU load).
// They should only be run manually on dedicated hardware with a profiler.
// Keeping them as stubs in source control documents the intent without adding
// flaky failures to the CI gate.
//
// TO ACTIVATE A STUB:
// --------------------
// Remove the leading underscore from the method name, add an [Ignore] or a
// dedicated test category filter, and implement the body.  Or move the logic
// to a separate BenchmarkDotNet benchmark project.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDC.ScriptEngine.Tests;

/// <summary>
/// Stress and performance test stubs for <see cref="SDC.ScriptEngine.SdcScriptEngine"/>.
/// All methods are intentionally empty; see file-level comment for conventions.
/// </summary>
[TestClass]
public class _SdcScriptEngineStressTests
{
    /// <summary>
    /// Stub: compile and run 100 different scripts, assert all succeed.
    /// Validates that the cache, ALC pool, and reference provider hold up
    /// under a large number of distinct compilations in a single test run.
    /// </summary>
    [TestMethod]
    public void _Script_100UniqueScripts_AllSucceed()
    {
        // STUB — implement when stress testing is required.
        // Suggested approach:
        //   for (int i = 0; i < 100; i++)
        //       await engine.ExecuteAsync($"var x{i} = {i};", q);
        //   then Assert all results successful.
    }

    /// <summary>
    /// Stub: run the same script 10,000 times and assert execution stays fast.
    /// Validates that the cache never thrashes and the ALC overhead is bounded.
    /// </summary>
    [TestMethod]
    public void _Script_RepeatedRunsHighFrequency_StablePerformance()
    {
        // STUB — implement when performance benchmarks are required.
        // Suggested approach:
        //   Stopwatch sw = Stopwatch.StartNew();
        //   for (int i = 0; i < 10_000; i++)
        //       await engine.ExecuteAsync(script, q);
        //   sw.Stop();
        //   Assert.IsTrue(sw.ElapsedMilliseconds < 5000, "10k cached runs must finish < 5 sec");
    }
}
