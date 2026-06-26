// ============================================================================
// ScriptHashMismatchException.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// This exception is thrown by SdcScriptEngine.ExecutePrecompiledAsync when
// it detects that the canonical hash of the current script source text does
// NOT match the hash stored in the SDC XML alongside the pre-compiled IL,
// and SdcScriptEngineOptions.OnHashMismatch is set to
// ScriptHashMismatchBehavior.Throw (the default).
//
// The two hash properties allow the caller to display a diff-like summary:
//   "Stored: abc123..., Computed: def456..."
// and decide whether to recompile or abort.

namespace SDC.ScriptEngine;

/// <summary>
/// Thrown when <see cref="SdcScriptEngine.ExecutePrecompiledAsync"/> detects
/// that the canonical hash of the current script source differs from the hash
/// stored in the SDC XML document alongside the pre-compiled IL bytes.
/// </summary>
/// <remarks>
/// A mismatch means the script source was edited after the IL was compiled.
/// The stored IL may no longer correspond to the current source text.
/// The caller should either recompile from source or abort the operation.
/// </remarks>
public class ScriptHashMismatchException : Exception
{
    /// <summary>
    /// The canonical SHA-256 hex hash computed from the current script source.
    /// </summary>
    public string ComputedHash { get; }

    /// <summary>
    /// The canonical SHA-256 hex hash that was stored in the SDC XML document
    /// (i.e., the hash at the time the IL was last compiled).
    /// </summary>
    public string StoredHash { get; }

    /// <summary>
    /// Creates a new <see cref="ScriptHashMismatchException"/> with both hash
    /// values embedded in the message.
    /// </summary>
    /// <param name="computedHash">Hash of the current script source text.</param>
    /// <param name="storedHash">Hash stored in the SDC XML document.</param>
    public ScriptHashMismatchException(string computedHash, string storedHash)
        : base(
            $"Script source hash mismatch. " +
            $"Computed: {computedHash}, Stored: {storedHash}. " +
            "The source was edited after the pre-compiled IL was generated. " +
            "Recompile required.")
    {
        ComputedHash = computedHash;
        StoredHash = storedHash;
    }
}
