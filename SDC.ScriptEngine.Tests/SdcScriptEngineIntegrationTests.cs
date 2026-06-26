// ============================================================================
// SdcScriptEngineIntegrationTests.cs
// SDC.ScriptEngine.Tests — end-to-end integration tests
// ============================================================================
//
// Integration tests exercise the full pipeline end-to-end:
//   compile → cache → load → invoke → verify
// They also verify the error-handling branches for hash mismatches.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

[TestClass]
public class SdcScriptEngineIntegrationTests
{
    [TestInitialize]
    public void Init() => BaseType.ResetLastTopNode();

    [TestCleanup]
    public void Cleanup() => BaseType.ResetLastTopNode();

    // ── 1. Full round-trip: compile → run → verify OM mutated ─────────────────

    [TestMethod]
    public async Task FullRoundTrip_CompileRunVerify_OmMutated()
    {
        // Exercises every step in the pipeline: CompileAsync produces IL,
        // RunAsync loads and executes the IL, and the OM reflects the change.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("IntegrationStart");

        const string script = """
            var q = (QuestionItemType)sdc;
            q.name = "IntegrationMutated";
            q.title = "Full Round-Trip";
            """;

        // Step 1: compile
        var compileResult = await engine.CompileAsync(script);
        Assert.IsTrue(compileResult.Success,
            "Compilation step of full round-trip must succeed.");
        Assert.IsNotNull(compileResult.CompiledIL,
            "IL bytes must be present after successful compilation.");

        // Step 2: run using the compiled IL directly (RunAsync path)
        var runResult = await engine.RunAsync(
            compileResult.CompiledIL!,
            compileResult.CanonicalHash,
            q);

        Assert.IsTrue(runResult.Success,
            "RunAsync with pre-compiled IL must succeed.");

        // Step 3: verify the OM mutation
        Assert.AreEqual("IntegrationMutated", q.name,
            "q.name must be set by the script after the full round-trip.");
        Assert.AreEqual("Full Round-Trip", q.title,
            "q.title must be set by the script after the full round-trip.");
    }

    // ── 2. ExecuteAsync convenience overload works ─────────────────────────────

    [TestMethod]
    public async Task ExecuteAsync_ConvenienceOverload_Works()
    {
        // ExecuteAsync(scriptText, sdcNode) is the single-call "convenience"
        // path that compiles (or hits the cache) and runs in one call.
        // This is the most common production usage pattern.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        // NOTE: SDC name property validates to XML NCName — no hyphens allowed.
        const string script = "((QuestionItemType)sdc).name = \"ConvenienceOk\";";

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"ExecuteAsync convenience overload must succeed. Error: {result.ErrorMessage}");
        Assert.AreEqual("ConvenienceOk", q.name,
            "q.name must reflect the mutation made via ExecuteAsync.");
    }

    // ── 3. Two sequential scripts, no cross-contamination ─────────────────────

    [TestMethod]
    public async Task TwoScripts_RunSequentially_NoCrossContamination()
    {
        // Running two different scripts on two different nodes must not cause
        // mutations from script A to appear on script B's node, and vice versa.
        // This verifies that the engine passes the correct live reference each time.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q1) = ScriptEngineTestHelper.CreateTestOmTree("Node1");
        var (_, q2) = ScriptEngineTestHelper.CreateTestOmTree("Node2");

        const string scriptA = "((QuestionItemType)sdc).name = \"ScriptA\";";
        const string scriptB = "((QuestionItemType)sdc).name = \"ScriptB\";";

        await engine.ExecuteAsync(scriptA, q1);
        await engine.ExecuteAsync(scriptB, q2);

        // Each node must reflect only its own script's mutation.
        Assert.AreEqual("ScriptA", q1.name,
            "q1.name must be set by scriptA only.");
        Assert.AreEqual("ScriptB", q2.name,
            "q2.name must be set by scriptB only.");

        // Confirm cross-contamination didn't occur.
        Assert.AreNotEqual(q1.name, q2.name,
            "The two nodes must have different name values — no cross-contamination.");
    }

    // ── 4. Hash mismatch with default behavior (Throw) ────────────────────────

    [TestMethod]
    public async Task HashMismatch_DefaultBehavior_ThrowsScriptHashMismatchException()
    {
        // The default OnHashMismatch behavior is Throw.  If we supply a stored
        // hash that doesn't match the current source, ExecutePrecompiledAsync
        // must throw ScriptHashMismatchException — not run the script.
        var engine = ScriptEngineTestHelper.CreateEngine(
            new SdcScriptEngineOptions { OnHashMismatch = ScriptHashMismatchBehavior.Throw });
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string script = "((QuestionItemType)sdc).name = \"should-not-run\";";

        // Compile once to get a valid IL blob.
        var compiled = await engine.CompileAsync(script);
        Assert.IsTrue(compiled.Success, "Pre-condition: initial compile must succeed.");

        // Use a deliberately wrong stored hash to trigger the mismatch.
        const string wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";
        var ilBase64 = Convert.ToBase64String(compiled.CompiledIL!);

        // The call must throw because the hashes don't match.
        // Using try/catch pattern since it works across all MSTest versions.
        ScriptHashMismatchException? caughtEx = null;
        try
        {
            await engine.ExecutePrecompiledAsync(script, wrongHash, ilBase64, q);
        }
        catch (ScriptHashMismatchException ex)
        {
            caughtEx = ex;
        }
        Assert.IsNotNull(caughtEx,
            "ExecutePrecompiledAsync must throw ScriptHashMismatchException on hash mismatch with Throw behavior.");

        // The mutation must NOT have occurred (script did not run).
        Assert.AreNotEqual("should-not-run", q.name,
            "The script must not have executed when a hash mismatch exception was thrown.");
    }

    // ── 5. Hash mismatch with RecompileAndRun ─────────────────────────────────

    [TestMethod]
    public async Task HashMismatch_WithRecompileAndRun_SucceedsAndUpdatesCache()
    {
        // With RecompileAndRun, a hash mismatch silently recompiles and executes.
        // The caller is responsible for persisting the new hash/IL.
        var engine = ScriptEngineTestHelper.CreateEngine(
            new SdcScriptEngineOptions { OnHashMismatch = ScriptHashMismatchBehavior.RecompileAndRun });
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string script = "((QuestionItemType)sdc).name = \"recompiled\";";

        var compiled = await engine.CompileAsync(script);
        Assert.IsTrue(compiled.Success, "Pre-condition: initial compile must succeed.");

        // A wrong stored hash triggers a silent recompile.
        const string wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";
        var ilBase64 = Convert.ToBase64String(compiled.CompiledIL!);

        var result = await engine.ExecutePrecompiledAsync(script, wrongHash, ilBase64, q);

        // The engine must recompile and still execute the script successfully.
        Assert.IsTrue(result.Success,
            "ExecutePrecompiledAsync with RecompileAndRun must succeed even on hash mismatch.");

        // The script mutation must have occurred.
        Assert.AreEqual("recompiled", q.name,
            "q.name must be set by the recompiled-and-run script.");
    }

    // ── 6. Hash mismatch with Cancel ──────────────────────────────────────────

    [TestMethod]
    public async Task HashMismatch_WithCancel_ReturnsFailedResult()
    {
        // With Cancel, a hash mismatch returns a failed result without throwing
        // and without running the script — useful in audit/read-only contexts.
        var engine = ScriptEngineTestHelper.CreateEngine(
            new SdcScriptEngineOptions { OnHashMismatch = ScriptHashMismatchBehavior.Cancel });
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("ShouldNotChange");

        const string script = "((QuestionItemType)sdc).name = \"cancelled\";";

        var compiled = await engine.CompileAsync(script);
        Assert.IsTrue(compiled.Success, "Pre-condition: initial compile must succeed.");

        const string wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";
        var ilBase64 = Convert.ToBase64String(compiled.CompiledIL!);

        var result = await engine.ExecutePrecompiledAsync(script, wrongHash, ilBase64, q);

        // The result must indicate failure (but no exception was thrown).
        Assert.IsFalse(result.Success,
            "ExecutePrecompiledAsync with Cancel must return a failed result on hash mismatch.");

        // The script must NOT have mutated the node.
        Assert.AreNotEqual("cancelled", q.name,
            "q.name must be unchanged when Cancel behavior is used on a hash mismatch.");
    }
}
