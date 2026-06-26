// ============================================================================
// SdcScriptEngine.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// OVERVIEW
// --------
// SdcScriptEngine is the primary public API of this library.  It orchestrates
// the full pipeline:
//
//   (1) Canonicalize the script text and compute its hash.
//   (2) Check the in-process cache; skip compilation if the hash is known.
//   (3) Wrap the script body in the generated C# class template.
//   (4) Compile the wrapped source with Roslyn (CSharpCompilation.Create).
//   (5) Emit IL bytes to a MemoryStream.
//   (6) Load the IL bytes into a custom SdcScriptLoadContext.
//   (7) Resolve the SdcScript.Execute(BaseType) MethodInfo via reflection.
//   (8) Cache the entry (ALC + MethodInfo + IL bytes) for future invocations.
//   (9) Invoke the method, passing the live SDC OM node as argument.
//  (10) Return a structured result.
//
// PLATFORM COMPATIBILITY
// ----------------------
// This library targets net10.0 and is designed to work on:
//
//   Desktop (.NET CLR):
//     AppDomainReferenceProvider enumerates loaded assemblies.
//     AssemblyLoadContext (collectible) unloads scripts after GC.
//     All features available.
//
//   Blazor WASM (mono/wasm interpreter mode):
//     REQUIREMENT: The Blazor project must set:
//       <WasmEnableInterpreter>true</WasmEnableInterpreter>
//       <WasmEnableWebcil>false</WasmEnableWebcil>
//     A WASM-specific ISdcScriptReferenceProvider that fetches DLL bytes
//     via HttpClient is required (not included in this library — platform
//     concern belongs in the Blazor host project).
//     AssemblyLoadContext with isCollectible: true is available on mono/wasm
//     but GC unloading may not occur in practice (Phase 1 known limitation).
//
//   MSTest (desktop):
//     Use AppDomainReferenceProvider; all features work.
//
// SECURITY NOTE
// -------------
// Scripts compiled by this engine run IN-PROCESS with the SAME trust level
// as the host application.  There is NO OS-level sandbox in .NET 10.
// Scripts can access any type, field, or resource visible to the host process
// (subject only to the MetadataReferences provided by ISdcScriptReferenceProvider).
//
// Phase 1 mitigations:
//   - allowUnsafe: false in compilation options (no pointer arithmetic)
//   - MetadataReferences are controlled by the ISdcScriptReferenceProvider
//     implementation; a restrictive provider can exclude System.IO, System.Net,
//     System.Diagnostics.Process, and System.Runtime.InteropServices.
//
// Phase 2 (deferred): SdcScriptSecurityAnalyzer — a Roslyn diagnostic analyzer
// that statically rejects scripts using banned APIs before compilation,
// providing a defense-in-depth layer.
//
// DO NOT USE Thread.Abort for timeouts.  Thread.Abort was removed in .NET Core.
// Use CancellationToken for cooperative cancellation (passed to Roslyn Emit
// and checked at strategic points before/after major steps).

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SDC.Schema;

namespace SDC.ScriptEngine;

/// <summary>
/// Orchestrates canonicalization, compilation, caching, loading, and
/// execution of C# scripts that operate on the SDC Object Model.
/// </summary>
/// <remarks>
/// <para>
/// <b>Typical usage</b>:
/// <code>
/// var engine = new SdcScriptEngine(new AppDomainReferenceProvider());
/// var result = await engine.ExecuteAsync(scriptBody, myOmNode);
/// if (!result.Success) Console.WriteLine(result.ErrorMessage);
/// </code>
/// </para>
/// <para>
/// <b>Pre-compiled IL path</b> (XML round-trip):
/// <code>
/// // On save: compile and store hash + IL in the SDC XML node
/// var compile = await engine.CompileAsync(scriptBody);
/// node.SourceHash    = compile.CanonicalHash;
/// node.CompiledILBase64 = Convert.ToBase64String(compile.CompiledIL!);
///
/// // On load: run without recompiling (fast path)
/// var run = await engine.ExecutePrecompiledAsync(
///     node.Source, node.SourceHash, node.CompiledILBase64, sdcNode);
/// </code>
/// </para>
/// <para>
/// <b>Platform notes</b>:
/// <list type="bullet">
///   <item>Desktop: provide <see cref="AppDomainReferenceProvider"/>.</item>
///   <item>Blazor WASM: provide a custom <see cref="ISdcScriptReferenceProvider"/>
///         that fetches DLL bytes via <c>HttpClient</c>.  Also enable
///         <c>WasmEnableInterpreter</c> and disable <c>WasmEnableWebcil</c>
///         in the Blazor host project.</item>
/// </list>
/// </para>
/// <para>
/// <b>Security</b>: scripts run in-process with host trust level.
/// No OS sandbox exists in .NET 10.  Restrict the reference provider to
/// exclude System.IO / System.Net / System.Diagnostics.Process if needed.
/// Phase 2 will add a static security analyzer.
/// </para>
/// </remarks>
public class SdcScriptEngine
{
    private readonly ISdcScriptReferenceProvider _referenceProvider;
    private readonly SdcScriptCache _cache;
    private readonly SdcScriptEngineOptions _options;

    /// <summary>
    /// Creates a new <see cref="SdcScriptEngine"/> with the specified
    /// reference provider and optional options.
    /// </summary>
    /// <param name="referenceProvider">
    /// Platform-specific provider of Roslyn metadata references.
    /// Use <see cref="AppDomainReferenceProvider"/> on desktop.
    /// </param>
    /// <param name="options">
    /// Optional engine configuration.  If null, a default
    /// <see cref="SdcScriptEngineOptions"/> is used.
    /// </param>
    public SdcScriptEngine(
        ISdcScriptReferenceProvider referenceProvider,
        SdcScriptEngineOptions? options = null)
    {
        _referenceProvider = referenceProvider
            ?? throw new ArgumentNullException(nameof(referenceProvider));
        _cache = new SdcScriptCache();
        _options = options ?? new SdcScriptEngineOptions();
    }

    // =========================================================================
    // CompileAsync
    // =========================================================================

    /// <summary>
    /// Compiles a C# script body to IL bytes, or returns the cached result
    /// if the canonical hash is already known.
    /// </summary>
    /// <param name="scriptText">
    /// The raw C# script body (method body statements only; no class/method
    /// declaration needed — the engine wraps it automatically).
    /// </param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// A <see cref="SdcScriptCompileResult"/> with the IL bytes on success,
    /// or a list of <see cref="SdcScriptDiagnostic"/> entries on failure.
    /// </returns>
    public Task<SdcScriptCompileResult> CompileAsync(
        string scriptText,
        CancellationToken ct = default)
    {
        // Run compilation on a thread-pool thread so we don't block a
        // UI thread or Blazor's synchronization context.
        // Note: on Blazor WASM, Task.Run posts work cooperatively (single
        // thread) — this is fine; it just yields control before compiling.
        return Task.Run(() => CompileCore(scriptText, ct), ct);
    }

    private SdcScriptCompileResult CompileCore(string scriptText, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // ── Step 1: compute canonical hash ────────────────────────────────
        // The canonical hash is the cache key AND the value stored in XML.
        // We compute it from the RAW script body (before wrapping), matching
        // what callers will later compute when verifying the XML-stored hash.
        var canonicalHash = SdcScriptCanonicalizer.ComputeCanonicalHash(scriptText);

        // ── Step 2: cache hit ─────────────────────────────────────────────
        // If we've compiled this exact script before (same canonical tokens),
        // return a success result using the cached IL bytes.  Do NOT recompile.
        if (_cache.TryGet(canonicalHash, out var cached))
        {
            // The cache stores the IL bytes from the original compilation,
            // so we can return them in the result without re-emitting.
            return new SdcScriptCompileResult(
                Success: true,
                CompiledIL: cached.ILBytes,
                CanonicalHash: canonicalHash,
                Diagnostics: Array.Empty<SdcScriptDiagnostic>());
        }

        ct.ThrowIfCancellationRequested();

        // ── Step 3: wrap the user script body ─────────────────────────────
        // Insert the user code into the static class/method template.
        // The #line directive inside the template ensures compiler errors are
        // reported relative to the user's code (line 1 = user's first line).
        var wrappedSource = SdcScriptTemplate.Wrap(scriptText);

        // ── Step 4: parse ─────────────────────────────────────────────────
        // ParseText converts the source string into a syntax tree.
        // No compilation happens yet — just lexical and syntax analysis.
        var syntaxTree = CSharpSyntaxTree.ParseText(wrappedSource, cancellationToken: ct);

        // ── Step 5: get metadata references ──────────────────────────────
        // Ask the platform-specific provider for the assembly references that
        // should be available to the script (like /reference flags).
        var references = _referenceProvider.GetReferences();

        ct.ThrowIfCancellationRequested();

        // ── Step 6: configure compilation options ─────────────────────────
        //
        // DynamicallyLinkedLibrary: produce a DLL (assembly), not an EXE.
        //   The script is a library called by the host, not a standalone program.
        //
        // OptimizationLevel.Release: inline small methods, remove debug nops.
        //   Scripts are short and don't need debug symbols.
        //
        // deterministic: true — given the same source and references, Roslyn
        //   always emits identical IL bytes.  This enables future byte-level
        //   comparison to verify that stored IL is actually from the current
        //   source (not just that the hash matches, but that the bytes are
        //   literally unchanged).
        //
        // allowUnsafe: controlled by options — defaults to false.
        //   Keeping 'unsafe' blocks out of user scripts is a baseline security
        //   posture (no pointer arithmetic, no fixed/stackalloc).
        var compilationOptions = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: _options.AllowUnsafeCode)
            .WithDeterministic(true);

        // ── Step 7: create the compilation ───────────────────────────────
        // The assembly name includes the first 8 chars of the hash to make
        // it unique and identifiable in stack traces, while staying short.
        var compilation = CSharpCompilation.Create(
            assemblyName: $"SdcScript_{canonicalHash[..8]}",
            syntaxTrees: [syntaxTree],
            references: references,
            options: compilationOptions);

        // ── Step 8: emit IL ───────────────────────────────────────────────
        // Emit writes the compiled assembly as a PE/COFF byte stream.
        // We capture it in a MemoryStream so no disk I/O occurs.
        // (No disk I/O is a requirement for WASM compatibility — the WASM
        //  sandbox may not allow arbitrary file writes.)
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms, cancellationToken: ct);

        // ── Step 9: check for errors ──────────────────────────────────────
        if (!emitResult.Success || HasErrors(emitResult))
        {
            var diagnostics = ConvertDiagnostics(emitResult.Diagnostics);
            return new SdcScriptCompileResult(
                Success: false,
                CompiledIL: null,
                CanonicalHash: canonicalHash,
                Diagnostics: diagnostics);
        }

        // ── Step 10: collect warnings (non-fatal) ─────────────────────────
        var warnings = ConvertDiagnostics(emitResult.Diagnostics);

        // ── Step 11: load into a custom ALC ──────────────────────────────
        // SdcScriptLoadContext.Load() returns null for all dependencies,
        // forcing the runtime to use the host's copies of SDC.Schema etc.
        // This preserves type identity (see SdcScriptLoadContext.cs).
        var ilBytes = ms.ToArray();
        var alc = new SdcScriptLoadContext();
        var assembly = alc.LoadScript(ilBytes);

        // ── Step 12: resolve the Execute MethodInfo ───────────────────────
        // We use reflection to find SdcScript.Execute(BaseType) in the loaded
        // assembly.  The '!' null-forgiving operators are safe because the
        // template guarantees the class and method always exist if emit
        // succeeded.
        var executeMethod = assembly
            .GetType("SdcScript")!
            .GetMethod("Execute", BindingFlags.Public | BindingFlags.Static)!;

        // ── Step 13: cache the entry ──────────────────────────────────────
        // Store ALC + MethodInfo + IL bytes so subsequent calls with the same
        // hash avoid recompilation and re-loading entirely.
        var entry = new SdcScriptEntry(alc, executeMethod, ilBytes);
        _cache.Store(canonicalHash, entry);

        return new SdcScriptCompileResult(
            Success: true,
            CompiledIL: ilBytes,
            CanonicalHash: canonicalHash,
            Diagnostics: warnings);
    }

    // =========================================================================
    // RunAsync
    // =========================================================================

    /// <summary>
    /// Executes pre-compiled IL bytes against a live SDC OM node.
    /// </summary>
    /// <param name="compiledIL">
    /// IL bytes from a prior <see cref="CompileAsync"/> call or decoded from
    /// <see cref="SdcScriptNode.CompiledILBase64"/>.
    /// </param>
    /// <param name="canonicalHash">
    /// The canonical hash of the script body.  If this hash is already in
    /// the cache (from a prior compile or run), the cached MethodInfo is
    /// reused and <paramref name="compiledIL"/> is ignored.
    /// </param>
    /// <param name="sdcNode">
    /// The SDC OM node to pass as the <c>sdc</c> argument to
    /// <c>SdcScript.Execute(BaseType sdc)</c>.
    /// </param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A <see cref="SdcScriptRunResult"/> indicating success or failure.</returns>
    public Task<SdcScriptRunResult> RunAsync(
        byte[] compiledIL,
        string canonicalHash,
        BaseType sdcNode,
        CancellationToken ct = default)
    {
        return Task.Run(() => RunCore(compiledIL, canonicalHash, sdcNode, ct), ct);
    }

    private SdcScriptRunResult RunCore(
        byte[] compiledIL,
        string canonicalHash,
        BaseType sdcNode,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // ── Resolve or load the MethodInfo ────────────────────────────────
        SdcScriptEntry entry;

        if (!_cache.TryGet(canonicalHash, out var cached))
        {
            // Cache miss: load the provided IL bytes into a new ALC.
            // This happens when the caller supplies pre-compiled IL from
            // XML storage without having first called CompileAsync.
            var alc = new SdcScriptLoadContext();
            var assembly = alc.LoadScript(compiledIL);
            var method = assembly
                .GetType("SdcScript")!
                .GetMethod("Execute", BindingFlags.Public | BindingFlags.Static)!;
            entry = new SdcScriptEntry(alc, method, compiledIL);
            _cache.Store(canonicalHash, entry);
        }
        else
        {
            entry = cached;
        }

        ct.ThrowIfCancellationRequested();

        // ── Invoke the script ─────────────────────────────────────────────
        // MethodInfo.Invoke calls the static Execute(BaseType sdc) method.
        // First param null = no instance (static method).
        // Second param is the argument array: just the sdcNode.
        //
        // IMPORTANT: MethodInfo.Invoke wraps any exception thrown by the
        // script inside a TargetInvocationException.  We unwrap it to expose
        // the original exception to the caller, which is more useful for
        // diagnostics and UI error messages.
        try
        {
            entry.ExecuteMethod.Invoke(null, [sdcNode]);
            return new SdcScriptRunResult(Success: true, Exception: null, ErrorMessage: null);
        }
        catch (TargetInvocationException tie)
        {
            // Unwrap: the InnerException is the actual exception from user code.
            var inner = tie.InnerException ?? tie;
            return new SdcScriptRunResult(
                Success: false,
                Exception: inner,
                ErrorMessage: inner.Message);
        }
        catch (Exception ex)
        {
            // Unexpected runtime error (e.g., type-load failure, ALC disposed).
            return new SdcScriptRunResult(
                Success: false,
                Exception: ex,
                ErrorMessage: ex.Message);
        }
    }

    // =========================================================================
    // ExecuteAsync
    // =========================================================================

    /// <summary>
    /// Convenience method: compiles the script (or uses the cache) and
    /// immediately runs it against <paramref name="sdcNode"/>.
    /// </summary>
    /// <param name="scriptText">Raw C# script body.</param>
    /// <param name="sdcNode">The SDC OM node to pass to the script.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A <see cref="SdcScriptRunResult"/>.</returns>
    public async Task<SdcScriptRunResult> ExecuteAsync(
        string scriptText,
        BaseType sdcNode,
        CancellationToken ct = default)
    {
        var compileResult = await CompileAsync(scriptText, ct).ConfigureAwait(false);

        if (!compileResult.Success)
        {
            // Surface compilation errors as a failed run result so callers
            // only need to check one result type.
            var errorSummary = string.Join("; ",
                compileResult.Diagnostics
                    .Where(d => d.Severity == SdcDiagnosticSeverity.Error)
                    .Select(d => $"Line {d.Line}: {d.Message}"));
            return new SdcScriptRunResult(
                Success: false,
                Exception: null,
                ErrorMessage: $"Compilation failed: {errorSummary}");
        }

        return await RunAsync(
            compileResult.CompiledIL!,
            compileResult.CanonicalHash,
            sdcNode,
            ct).ConfigureAwait(false);
    }

    // =========================================================================
    // ExecutePrecompiledAsync
    // =========================================================================

    /// <summary>
    /// Executes a pre-compiled script stored in an SDC XML document,
    /// verifying the source hash before running.
    /// </summary>
    /// <param name="scriptText">
    /// The current script source text (read from the SDC XML element).
    /// Used to compute the canonical hash for comparison against
    /// <paramref name="storedSourceHash"/>.
    /// </param>
    /// <param name="storedSourceHash">
    /// The canonical hash stored in the SDC XML alongside the IL bytes.
    /// Produced by an earlier <see cref="CompileAsync"/> call.
    /// </param>
    /// <param name="compiledILBase64">
    /// Base64-encoded IL bytes stored in the SDC XML element.
    /// </param>
    /// <param name="sdcNode">The live SDC OM node to pass to the script.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// A <see cref="SdcScriptRunResult"/>.  If the hashes match, runs from
    /// the stored IL (fast path — no Roslyn involved).  If they mismatch,
    /// behaviour depends on <see cref="SdcScriptEngineOptions.OnHashMismatch"/>.
    /// </returns>
    /// <exception cref="ScriptHashMismatchException">
    /// Thrown when the hashes do not match and
    /// <see cref="SdcScriptEngineOptions.OnHashMismatch"/> is
    /// <see cref="ScriptHashMismatchBehavior.Throw"/>.
    /// </exception>
    public async Task<SdcScriptRunResult> ExecutePrecompiledAsync(
        string scriptText,
        string storedSourceHash,
        string compiledILBase64,
        BaseType sdcNode,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // ── Hash verification ─────────────────────────────────────────────
        // Compute the canonical hash of the CURRENT source and compare it
        // to the hash stored in the XML.  If they differ, the source was
        // edited after the IL was compiled and the stored IL is potentially
        // stale.
        var computed = SdcScriptCanonicalizer.ComputeCanonicalHash(scriptText);
        var hashesMatch = string.Equals(
            computed, storedSourceHash, StringComparison.OrdinalIgnoreCase);

        if (!hashesMatch)
        {
            switch (_options.OnHashMismatch)
            {
                case ScriptHashMismatchBehavior.Throw:
                    // Default: let the caller decide what to do (prompt user,
                    // log the event, or trigger a recompile workflow).
                    throw new ScriptHashMismatchException(computed, storedSourceHash);

                case ScriptHashMismatchBehavior.RecompileAndRun:
                    // Headless / automated path: recompile from current source
                    // and run immediately.  The caller is responsible for
                    // persisting the new hash and IL back to the XML document.
                    var compileResult = await CompileAsync(scriptText, ct).ConfigureAwait(false);
                    if (!compileResult.Success)
                    {
                        var errorSummary = string.Join("; ",
                            compileResult.Diagnostics
                                .Where(d => d.Severity == SdcDiagnosticSeverity.Error)
                                .Select(d => $"Line {d.Line}: {d.Message}"));
                        return new SdcScriptRunResult(
                            Success: false,
                            Exception: null,
                            ErrorMessage: $"Recompilation failed: {errorSummary}");
                    }
                    return await RunAsync(
                        compileResult.CompiledIL!,
                        compileResult.CanonicalHash,
                        sdcNode,
                        ct).ConfigureAwait(false);

                case ScriptHashMismatchBehavior.Cancel:
                    // Read-only / audit path: refuse to run anything.
                    return new SdcScriptRunResult(
                        Success: false,
                        Exception: null,
                        ErrorMessage:
                            $"Script run cancelled: source hash mismatch. " +
                            $"Computed: {computed}, Stored: {storedSourceHash}. " +
                            "Recompile required.");

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_options.OnHashMismatch),
                        _options.OnHashMismatch,
                        "Unknown ScriptHashMismatchBehavior value.");
            }
        }

        // ── Fast path: hashes match ───────────────────────────────────────
        // The stored IL corresponds to the current source.  Decode and run
        // without invoking Roslyn at all.
        var ilBytes = Convert.FromBase64String(compiledILBase64);
        return await RunAsync(ilBytes, storedSourceHash, sdcNode, ct).ConfigureAwait(false);
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    /// <summary>
    /// Returns true when the emit result contains hard errors, or when
    /// <see cref="SdcScriptEngineOptions.TreatWarningsAsErrors"/> is enabled
    /// and warnings are present.
    /// </summary>
    private bool HasErrors(Microsoft.CodeAnalysis.Emit.EmitResult emitResult)
    {
        if (!emitResult.Success) return true;

        if (_options.TreatWarningsAsErrors)
        {
            return emitResult.Diagnostics.Any(
                d => d.Severity == DiagnosticSeverity.Warning
                  && d.IsWarningAsError == false);
        }

        return false;
    }

    /// <summary>
    /// Converts Roslyn <see cref="Diagnostic"/> objects to the public
    /// <see cref="SdcScriptDiagnostic"/> record, filtering to errors and
    /// warnings only (skipping hidden/info diagnostics unless they become
    /// errors due to options).
    /// </summary>
    private IReadOnlyList<SdcScriptDiagnostic> ConvertDiagnostics(
        IEnumerable<Diagnostic> diagnostics)
    {
        var result = new List<SdcScriptDiagnostic>();

        foreach (var d in diagnostics)
        {
            // Skip Hidden severity (internal Roslyn infrastructure diagnostics).
            if (d.Severity == DiagnosticSeverity.Hidden) continue;

            // When TreatWarningsAsErrors is false, skip warnings during a
            // successful compile — only include them when they represent actual
            // problems (i.e., during a failed compile where we report everything).
            // For simplicity in Phase 1, we include all non-hidden diagnostics.

            var severity = d.Severity switch
            {
                DiagnosticSeverity.Error   => SdcDiagnosticSeverity.Error,
                DiagnosticSeverity.Warning => SdcDiagnosticSeverity.Warning,
                _                          => SdcDiagnosticSeverity.Info,
            };

            // GetLineSpan() returns positions relative to the #line directive,
            // so Line and Column are relative to the USER's script body
            // (line 1 = user's first line) rather than the wrapper source.
            // StartLinePosition is 0-based; we convert to 1-based here.
            var span = d.Location.GetLineSpan();
            var line   = span.StartLinePosition.Line + 1;
            var column = span.StartLinePosition.Character + 1;

            result.Add(new SdcScriptDiagnostic(severity, d.GetMessage(), line, column));
        }

        return result;
    }
}
