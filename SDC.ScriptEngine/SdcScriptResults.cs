// ============================================================================
// SdcScriptResults.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// This file defines the public result types returned by SdcScriptEngine:
//
//   SdcDiagnosticSeverity  — maps Roslyn DiagnosticSeverity to a public enum
//                            without exposing Roslyn types to the caller.
//
//   SdcScriptDiagnostic    — one compiler error or warning from a compilation,
//                            with line/column numbers relative to the USER's
//                            script body (thanks to the #line directive in the
//                            template).
//
//   SdcScriptCompileResult — the outcome of compiling a script, including the
//                            compiled IL bytes on success or diagnostics on
//                            failure.
//
//   SdcScriptRunResult     — the outcome of invoking a compiled script against
//                            a live SDC OM node.
//
// LINE NUMBER REPORTING
// ----------------------
// Because SdcScriptTemplate inserts `#line 1 "script"` immediately before the
// user's code, Roslyn's DiagnosticLocation.GetMappedLineSpan() returns line
// numbers relative to the user's first line of code (line 1 = user's first
// line), not the wrapper's first line (~10 lines higher).
// ConvertDiagnostics() in SdcScriptEngine calls GetMappedLineSpan(); no offset
// math is required here.  The mapped span also provides a FileName of "script"
// (from the #line directive) rather than an internal assembly name.

namespace SDC.ScriptEngine;

// ── Severity ─────────────────────────────────────────────────────────────────

/// <summary>
/// Severity level of a compiler diagnostic, mirroring Roslyn's
/// <c>DiagnosticSeverity</c> without exposing a Roslyn dependency in the
/// public API.
/// </summary>
public enum SdcDiagnosticSeverity
{
    /// <summary>A fatal compilation error; the script cannot be executed.</summary>
    Error,

    /// <summary>A non-fatal warning; the script compiles but may misbehave.</summary>
    Warning,

    /// <summary>An informational message from the compiler.</summary>
    Info,
}

// ── Diagnostic ───────────────────────────────────────────────────────────────

/// <summary>
/// Describes one compiler diagnostic (error or warning) produced during
/// C# script compilation.
/// </summary>
/// <param name="Severity">Error, warning, or info.</param>
/// <param name="Message">The human-readable compiler message text.</param>
/// <param name="Line">
/// 1-based line number within the USER's script body (not the generated
/// wrapper), because of the <c>#line 1 "script"</c> directive in the template.
/// </param>
/// <param name="Column">
/// 1-based column number within the user's line.
/// </param>
/// <param name="FileName">
/// The virtual file name from the <c>#line</c> directive — typically
/// <c>"script"</c> for errors in user code, or an empty string for
/// diagnostics that originate in wrapper-generated code.
/// </param>
public record SdcScriptDiagnostic(
    SdcDiagnosticSeverity Severity,
    string Message,
    int Line,
    int Column,
    string FileName = "");

// ── Compile result ────────────────────────────────────────────────────────────

/// <summary>
/// The result of compiling a C# script body via
/// <see cref="SdcScriptEngine.CompileAsync"/>.
/// </summary>
/// <param name="Success">
/// <see langword="true"/> if compilation succeeded and IL bytes are available.
/// </param>
/// <param name="CompiledIL">
/// Raw IL bytes produced by Roslyn, suitable for loading into a
/// <see cref="SdcScriptLoadContext"/> and for storage (Base64-encoded) in
/// SDC XML.  <see langword="null"/> when <paramref name="Success"/> is
/// <see langword="false"/>.
/// </param>
/// <param name="CanonicalHash">
/// The canonical SHA-256 hex hash of the script body computed by
/// <see cref="SdcScriptCanonicalizer.ComputeCanonicalHash"/>.
/// Store this in SDC XML alongside <paramref name="CompiledIL"/> so the
/// engine can detect whether the source was modified after compilation.
/// </param>
/// <param name="Diagnostics">
/// Compiler errors and warnings.  Non-empty even on success (warnings).
/// On failure, contains at least one <see cref="SdcDiagnosticSeverity.Error"/>
/// entry.
/// </param>
public record SdcScriptCompileResult(
    bool Success,
    byte[]? CompiledIL,
    string CanonicalHash,
    IReadOnlyList<SdcScriptDiagnostic> Diagnostics);

// ── Run result ────────────────────────────────────────────────────────────────

/// <summary>
/// The outcome of invoking a compiled script against a live SDC OM node via
/// <see cref="SdcScriptEngine.RunAsync"/> or
/// <see cref="SdcScriptEngine.ExecuteAsync"/>.
/// </summary>
/// <param name="Success">
/// <see langword="true"/> if <c>SdcScript.Execute</c> returned without
/// throwing.
/// </param>
/// <param name="Exception">
/// The exception thrown by the user script, if any.  The outer
/// <see cref="System.Reflection.TargetInvocationException"/> is unwrapped;
/// this is the script's original exception (or a runtime error such as
/// <see cref="NullReferenceException"/>).
/// </param>
/// <param name="ErrorMessage">
/// Human-readable error text, or <see langword="null"/> when
/// <paramref name="Success"/> is <see langword="true"/>.
/// </param>
public record SdcScriptRunResult(
    bool Success,
    Exception? Exception,
    string? ErrorMessage);
