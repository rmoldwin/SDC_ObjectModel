// ============================================================================
// SdcScriptEngineOptions.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================

namespace SDC.ScriptEngine;

// ── Hash-mismatch behavior ────────────────────────────────────────────────────

/// <summary>
/// Controls what <see cref="SdcScriptEngine.ExecutePrecompiledAsync"/> does
/// when it detects that the current script source has a different canonical
/// hash than the one stored alongside the pre-compiled IL in SDC XML.
/// </summary>
/// <remarks>
/// A mismatch means the user (or a tool) modified the script source after the
/// IL bytes were generated.  The stored IL is therefore potentially stale and
/// may not reflect the current intent of the script.
/// </remarks>
public enum ScriptHashMismatchBehavior
{
    /// <summary>
    /// Throw a <see cref="ScriptHashMismatchException"/>.  The caller must
    /// catch it and decide what to do (e.g., prompt the user to approve a
    /// recompile before proceeding).
    /// <para>
    /// This is the default behavior and is recommended for interactive UI
    /// contexts where a human should review and confirm the discrepancy.
    /// </para>
    /// </summary>
    Throw,

    /// <summary>
    /// Silently recompile the script from the current source text and run it,
    /// ignoring the stored IL.  The old IL bytes and hash are not updated by
    /// the engine; the caller is responsible for persisting the new values.
    /// <para>
    /// Suitable for automated / headless processing pipelines where re-running
    /// Roslyn on a mismatched script is acceptable overhead.
    /// </para>
    /// </summary>
    RecompileAndRun,

    /// <summary>
    /// Return a failed <see cref="SdcScriptRunResult"/> without throwing and
    /// without executing any script code.  The <c>ErrorMessage</c> in the
    /// result describes the mismatch.
    /// <para>
    /// Suitable for read-only / audit contexts where it is important that no
    /// script runs until the hash is verified externally.
    /// </para>
    /// </summary>
    Cancel,
}

// ── Engine options ─────────────────────────────────────────────────────────────

/// <summary>
/// Configuration options for <see cref="SdcScriptEngine"/>.
/// Pass an instance to the constructor to override defaults.
/// </summary>
public class SdcScriptEngineOptions
{
    /// <summary>
    /// Defines what <see cref="SdcScriptEngine.ExecutePrecompiledAsync"/>
    /// does when the current source hash does not match the hash stored in
    /// the SDC XML alongside the pre-compiled IL.
    /// </summary>
    /// <remarks>
    /// Default: <see cref="ScriptHashMismatchBehavior.Throw"/>.
    /// Change to <see cref="ScriptHashMismatchBehavior.RecompileAndRun"/>
    /// for automated pipelines or
    /// <see cref="ScriptHashMismatchBehavior.Cancel"/> for audit/read-only
    /// contexts.
    /// </remarks>
    public ScriptHashMismatchBehavior OnHashMismatch { get; set; }
        = ScriptHashMismatchBehavior.Throw;

    /// <summary>
    /// When <see langword="true"/>, Roslyn warnings are treated as errors
    /// and compilation fails if any warning is emitted.
    /// </summary>
    /// <remarks>
    /// Default: <see langword="false"/>.  Warnings never abort compilation
    /// by default — only hard errors do.
    /// </remarks>
    public bool TreatWarningsAsErrors { get; set; } = false;

    /// <summary>
    /// When <see langword="true"/>, the Roslyn compiler allows
    /// <c>unsafe</c> code blocks (pointer arithmetic, <c>fixed</c>
    /// statements) inside user scripts.
    /// </summary>
    /// <remarks>
    /// Default: <see langword="false"/>.  Keeping pointer arithmetic out of
    /// user scripts is a good baseline security posture; enable only if the
    /// use case genuinely requires it.
    /// </remarks>
    public bool AllowUnsafeCode { get; set; } = false;
}
