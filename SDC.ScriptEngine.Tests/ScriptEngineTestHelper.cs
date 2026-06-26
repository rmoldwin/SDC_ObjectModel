// ============================================================================
// ScriptEngineTestHelper.cs
// SDC.ScriptEngine.Tests — shared test infrastructure
// ============================================================================
//
// This is NOT a test class.  It is shared infrastructure used by every test
// class in this project.  Key responsibilities:
//
//   CreateEngine()            — builds a default SdcScriptEngine with the
//                               AppDomainReferenceProvider (desktop path).
//
//   CreateTestOmTree()        — builds a minimal SDC OM node tree that tests
//                               can target with scripts.  Returns a
//                               QuestionItemType because that is the most
//                               common script target in clinical form design.
//
//   CreateEngineWithCompileCounter() — wraps the reference provider with a
//                               thin spy that counts GetReferences() calls.
//                               Because GetReferences() is called exactly once
//                               per Roslyn compilation, the call count is a
//                               reliable proxy for the number of compilations
//                               performed (and therefore whether the cache was
//                               bypassed or hit).

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

/// <summary>
/// Shared helper utilities for <c>SDC.ScriptEngine.Tests</c>.
/// Not a test class — no <c>[TestClass]</c> attribute.
/// </summary>
internal static class ScriptEngineTestHelper
{
    // ── Engine factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a pre-configured <see cref="SdcScriptEngine"/> backed by
    /// <see cref="AppDomainReferenceProvider"/>, which enumerates all
    /// assemblies currently loaded in the CLR AppDomain.
    /// </summary>
    /// <param name="options">
    /// Optional engine options.  If null, defaults are used (hash-mismatch
    /// behavior = Throw, unsafe code disabled, warnings not errors).
    /// </param>
    public static SdcScriptEngine CreateEngine(SdcScriptEngineOptions? options = null)
        => new SdcScriptEngine(new AppDomainReferenceProvider(), options);

    // ── OM tree factory ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal SDC OM node tree suitable for script targeting.
    /// </summary>
    /// <remarks>
    /// SDC OM nodes use constructor-with-parent registration — each node must
    /// declare its parent at construction time so it is registered in the OM's
    /// internal dictionaries.  A standalone <see cref="DataElementType"/> with
    /// <c>null</c> parent is a valid unit-test root (no full form required).
    /// </remarks>
    /// <param name="questionName">
    /// The <c>name</c> property to set on the returned question.
    /// Defaults to <c>"TestQuestion"</c>.
    /// </param>
    /// <returns>
    /// A tuple of the top-level <see cref="DataElementType"/> and the
    /// <see cref="QuestionItemType"/> nested inside it.  Tests typically
    /// pass the question as the <c>sdcNode</c> argument to the engine.
    /// </returns>
    public static (DataElementType de, QuestionItemType question) CreateTestOmTree(
        string questionName = "TestQuestion")
    {
        // DataElementType with null parent = standalone unit-test root.
        // This is the established pattern in SDC.Schema.Tests.
        var de = new DataElementType(null);

        // QuestionItemType is registered as a child of 'de' during construction.
        var q = new QuestionItemType(de, "q-test");
        q.name = questionName;

        return (de, q);
    }

    // ── Compile-counting engine factory ──────────────────────────────────────

    /// <summary>
    /// Creates an <see cref="SdcScriptEngine"/> whose underlying reference
    /// provider is wrapped with a spy that counts <c>GetReferences()</c>
    /// invocations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="ISdcScriptReferenceProvider.GetReferences"/> is called
    /// exactly once per Roslyn compilation.  Counting those calls therefore
    /// tells us how many times Roslyn actually ran, which is the correct
    /// observable signal for "was the cache hit or missed?".
    /// </para>
    /// <para>
    /// The <c>getCompileCount</c> delegate returned here captures the spy
    /// instance; call it any time to retrieve the current count.
    /// </para>
    /// </remarks>
    /// <returns>
    /// A tuple of the engine and a <c>Func&lt;int&gt;</c> that returns the
    /// number of compilations performed so far.
    /// </returns>
    public static (SdcScriptEngine engine, Func<int> getCompileCount)
        CreateEngineWithCompileCounter()
    {
        var counter = new CompileCountingProvider(new AppDomainReferenceProvider());
        return (new SdcScriptEngine(counter), () => counter.CompileCount);
    }

    // ── Private spy ──────────────────────────────────────────────────────────

    /// <summary>
    /// Transparent wrapper around an <see cref="ISdcScriptReferenceProvider"/>
    /// that increments a counter each time <see cref="GetReferences"/> is
    /// called.  Thread-safe via <see cref="Interlocked.Increment"/>.
    /// </summary>
    private sealed class CompileCountingProvider : ISdcScriptReferenceProvider
    {
        private int _count;
        private readonly ISdcScriptReferenceProvider _inner;

        public CompileCountingProvider(ISdcScriptReferenceProvider inner)
            => _inner = inner;

        /// <summary>Returns the number of times GetReferences was called.</summary>
        public int CompileCount => _count;

        /// <inheritdoc />
        public IReadOnlyList<MetadataReference> GetReferences()
        {
            // Interlocked.Increment is used rather than lock() so this spy
            // is safe for parallel test runs without blocking.
            Interlocked.Increment(ref _count);
            return _inner.GetReferences();
        }
    }
}
