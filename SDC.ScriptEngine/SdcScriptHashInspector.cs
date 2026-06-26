// ============================================================================
// SdcScriptHashInspector.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// PURPOSE
// -------
// When an SDC XML document stores a pre-compiled script (Base64 IL bytes and
// the canonical hash of the source at compile time), the host application must
// be able to verify whether the source text has been modified since the IL was
// generated.  If it has, the stored IL is stale and must be discarded before
// recompilation.
//
// This class provides the verification utility and a structured result type
// so UI code can display a clear status message ("Match" vs "MISMATCH —
// source changed") without embedding the hashing logic.
//
// RELATIONSHIP TO SdcScriptCanonicalizer
// ----------------------------------------
// SdcScriptHashInspector.ComputeHash delegates directly to
// SdcScriptCanonicalizer.ComputeCanonicalHash.  Both methods use the same
// algorithm.  SdcScriptHashInspector is the public-facing convenience API;
// SdcScriptCanonicalizer is the implementation detail.

namespace SDC.ScriptEngine;

// ── Verification result ────────────────────────────────────────────────────────

/// <summary>
/// The result of comparing the current script source's canonical hash against
/// a hash stored in SDC XML.
/// </summary>
/// <param name="HashesMatch">
/// <see langword="true"/> if the computed hash equals the stored hash
/// (case-insensitive), meaning the source has not changed since compilation.
/// </param>
/// <param name="ComputedHash">
/// The canonical SHA-256 hex hash of the current script source text.
/// </param>
/// <param name="StoredHash">
/// The hash read from the SDC XML document (as-is, without normalization).
/// </param>
/// <param name="StatusMessage">
/// A human-readable summary suitable for display in a UI:
/// <c>"Match — pre-compiled IL is current"</c> or
/// <c>"MISMATCH — source was edited after IL was compiled. Recompile required."</c>
/// </param>
public record SdcHashVerificationResult(
    bool HashesMatch,
    string ComputedHash,
    string StoredHash,
    string StatusMessage);

// ── Inspector ─────────────────────────────────────────────────────────────────

/// <summary>
/// Utilities for computing and verifying the canonical SHA-256 hashes that
/// are stored in SDC XML documents alongside pre-compiled script IL.
/// </summary>
/// <remarks>
/// These methods use the same hashing algorithm as
/// <see cref="SdcScriptCanonicalizer"/>.  Always use
/// <see cref="ComputeHash"/> (not a raw hash of source text) to produce
/// values that are comparable to hashes stored in XML.
/// </remarks>
public static class SdcScriptHashInspector
{
    /// <summary>
    /// Computes the canonical SHA-256 hex hash of <paramref name="scriptText"/>
    /// for display, comparison, or storage in SDC XML.
    /// </summary>
    /// <param name="scriptText">
    /// The raw C# script body as the user wrote it.
    /// </param>
    /// <returns>
    /// A 64-character lowercase hex string (the canonical hash).
    /// This is the same value that <see cref="SdcScriptEngine.CompileAsync"/>
    /// stores in <see cref="SdcScriptCompileResult.CanonicalHash"/>.
    /// </returns>
    public static string ComputeHash(string scriptText)
        => SdcScriptCanonicalizer.ComputeCanonicalHash(scriptText);

    /// <summary>
    /// Compares the canonical hash of the current source text against a
    /// hash previously stored in an SDC XML document.
    /// </summary>
    /// <param name="scriptText">
    /// The current C# script body text (may have been edited since the IL
    /// was last compiled).
    /// </param>
    /// <param name="storedHash">
    /// The hash value read from the SDC XML element (e.g.,
    /// <c>SdcScriptNode.SourceHash</c>).
    /// </param>
    /// <returns>
    /// A <see cref="SdcHashVerificationResult"/> with the match status,
    /// both hash strings, and a human-readable message.
    /// </returns>
    public static SdcHashVerificationResult Verify(string scriptText, string storedHash)
    {
        var computed = ComputeHash(scriptText);

        // Case-insensitive comparison because stored hashes may have been
        // produced with a different casing convention (all-uppercase from
        // an older tool version, for example).
        var match = string.Equals(computed, storedHash, StringComparison.OrdinalIgnoreCase);

        var msg = match
            ? "Match — pre-compiled IL is current"
            : "MISMATCH — source was edited after IL was compiled. Recompile required.";

        return new SdcHashVerificationResult(match, computed, storedHash, msg);
    }
}
