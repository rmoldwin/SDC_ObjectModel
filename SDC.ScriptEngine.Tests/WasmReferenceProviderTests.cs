// ============================================================================
// WasmReferenceProviderTests.cs
// SDC.ScriptEngine.Tests — PreloadedReferenceProvider contract tests
// ============================================================================
//
// PURPOSE
// -------
// These tests validate the PreloadedReferenceProvider class and the
// ISdcScriptReferenceProvider interface contract it fulfils.
//
// They run entirely on desktop (no WASM runtime, no HttpClient, no IJSRuntime)
// because PreloadedReferenceProvider is a pure synchronous wrapper — the async
// loading half (WasmReferenceProvider) is tested separately in a WASM context.
//
// DESIGN
// ------
// The tests use AppDomainReferenceProvider to obtain a real set of Roslyn
// MetadataReferences (the same references available on desktop), then wrap
// them in PreloadedReferenceProvider.  This lets us:
//
//   1. Verify that GetReferences() returns exactly the list supplied to the
//      constructor (interface contract: provider must return what it was given).
//
//   2. Verify that SdcScriptEngine, constructed with PreloadedReferenceProvider,
//      can compile and run the standard SDC script against a live OM node —
//      proving the synchronous interface bridge works end-to-end.
//
// These tests directly prove the scenario used in Phase 2 Blazor WASM:
//   WasmReferenceProvider async load → PreloadedReferenceProvider → SdcScriptEngine.

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

[TestClass]
public class WasmReferenceProviderTests
{
    [TestInitialize]
    public void Init() => BaseType.ResetLastTopNode();

    [TestCleanup]
    public void Cleanup() => BaseType.ResetLastTopNode();

    // ── 1. Constructor: returns exact list supplied ────────────────────────────

    [TestMethod]
    public void PreloadedReferenceProvider_GetReferences_ReturnsExactListFromConstructor()
    {
        // Arrange — build an artificial two-element reference list.
        // We use AppDomainReferenceProvider to obtain valid references so we
        // don't need to know any DLL paths ahead of time.
        var desktopRefs = new AppDomainReferenceProvider().GetReferences();

        // Take the first two items to produce a small, deterministic list.
        // If GetReferences() returns what was supplied, these two must match.
        var subset = desktopRefs.Take(2).ToList().AsReadOnly();

        // Act
        var provider = new PreloadedReferenceProvider(subset);
        var result   = provider.GetReferences();

        // Assert: the returned list must be reference-equal (same objects, same order).
        Assert.AreEqual(subset.Count, result.Count,
            "GetReferences() must return a list with the same element count as the constructor argument.");

        for (int i = 0; i < subset.Count; i++)
        {
            // Object.ReferenceEquals ensures we got back the exact same
            // MetadataReference instances, not copies.
            Assert.IsTrue(ReferenceEquals(subset[i], result[i]),
                $"Element {i}: GetReferences() must return the same MetadataReference " +
                "instance that was supplied to the constructor.");
        }
    }

    // ── 2. Constructor: empty list is accepted ─────────────────────────────────

    [TestMethod]
    public void PreloadedReferenceProvider_EmptyList_AcceptedAndReturnedEmpty()
    {
        // An empty list is valid (though it will produce Roslyn errors at compile
        // time).  The provider must not throw or modify the list.
        var provider = new PreloadedReferenceProvider(Array.Empty<MetadataReference>());
        var result   = provider.GetReferences();

        // An empty input must produce an empty result.
        Assert.AreEqual(0, result.Count,
            "An empty MetadataReference list must be returned as-is (count == 0).");
    }

    // ── 3. Constructor: null throws ArgumentNullException ─────────────────────

    [TestMethod]
    public void PreloadedReferenceProvider_NullArgument_ThrowsArgumentNullException()
    {
        // Passing null must be rejected immediately to prevent a confusing
        // NullReferenceException at compile time instead of construction time.
        bool threw = false;
        try
        {
            _ = new PreloadedReferenceProvider(null!);
        }
        catch (ArgumentNullException)
        {
            threw = true;
        }

        Assert.IsTrue(threw,
            "Constructor must throw ArgumentNullException when references is null.");
    }

    // ── 4. GetReferences() called multiple times returns same list ─────────────

    [TestMethod]
    public void PreloadedReferenceProvider_MultipleCallsReturnSameList()
    {
        // Roslyn may call GetReferences() once per compilation.  Multiple calls
        // must return the same (or equivalent) list — specifically not null.
        var refs     = new AppDomainReferenceProvider().GetReferences();
        var provider = new PreloadedReferenceProvider(refs);

        var first  = provider.GetReferences();
        var second = provider.GetReferences();

        // The same backing list should be returned each time.
        Assert.IsTrue(ReferenceEquals(first, second),
            "Multiple GetReferences() calls must return the same list instance (no re-allocation).");
    }

    // ── 5. Full end-to-end: PreloadedReferenceProvider → SdcScriptEngine compiles and runs ──

    [TestMethod]
    public async Task PreloadedReferenceProvider_WithDesktopRefs_CompilesAndRunsStandardScript()
    {
        // Arrange: obtain the real AppDomain references, wrap in PreloadedReferenceProvider,
        // and build a SdcScriptEngine.  This exactly mirrors the Phase 2 Blazor path:
        //   WasmReferenceProvider.GetReferencesAsync() → PreloadedReferenceProvider → SdcScriptEngine
        var desktopRefs = new AppDomainReferenceProvider().GetReferences();
        var provider    = new PreloadedReferenceProvider(desktopRefs);
        var engine      = new SdcScriptEngine(provider);

        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("PreloadedStart");

        // The standard SDC script body used across tests.
        const string script =
            "var q = (SDC.Schema.QuestionItemType)sdc;\n" +
            "q.name = q.name + \"_mutated\";";

        // Act: compile
        var compileResult = await engine.CompileAsync(script);

        // Compilation with a full reference set must succeed.
        Assert.IsTrue(compileResult.Success,
            "Compilation via PreloadedReferenceProvider with desktop references must succeed. " +
            "Errors: " + string.Join("; ", compileResult.Diagnostics.Select(d => $"L{d.Line}: {d.Message}")));

        Assert.IsNotNull(compileResult.CompiledIL,
            "IL bytes must be present after successful compilation.");
        Assert.IsTrue(compileResult.CompiledIL!.Length > 0,
            "IL byte array must be non-empty.");

        // Act: run
        var runResult = await engine.RunAsync(
            compileResult.CompiledIL!,
            compileResult.CanonicalHash,
            q);

        // The run must succeed and the OM mutation must be visible on the caller side,
        // proving that the ALC type-identity bridge works correctly.
        Assert.IsTrue(runResult.Success,
            "RunAsync must succeed after compilation via PreloadedReferenceProvider. " +
            $"Error: {runResult.ErrorMessage}");

        Assert.AreEqual("PreloadedStart_mutated", q.name,
            "The script must mutate q.name by appending '_mutated'. " +
            "If this fails with InvalidCastException, check SdcScriptLoadContext.Load() returns null.");
    }

    // ── 6. Interface contract: GetReferences() count matches AppDomainReferenceProvider ──

    [TestMethod]
    public void PreloadedReferenceProvider_WithFullDesktopRefs_CountMatchesSource()
    {
        // GetReferences() on PreloadedReferenceProvider must return exactly the
        // same number of references that were supplied — not more, not fewer.
        var desktopRefs = new AppDomainReferenceProvider().GetReferences();
        var provider    = new PreloadedReferenceProvider(desktopRefs);
        var result      = provider.GetReferences();

        // The count must be preserved exactly.
        Assert.AreEqual(desktopRefs.Count, result.Count,
            "PreloadedReferenceProvider.GetReferences() count must equal the count " +
            "of the list supplied to the constructor.");

        // All references must be present.
        Assert.IsTrue(result.Count > 0,
            "AppDomainReferenceProvider should have returned at least one reference on desktop.");
    }
}
