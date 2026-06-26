// ============================================================================
// SdcScriptCanonicalizer.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// PURPOSE
// -------
// Before hashing a C# script body (to produce a cache key or an XML-stored
// artifact hash), we MUST normalize the text so that semantically identical
// scripts always produce the same hash.  Without normalization, trivial edits
// — a blank line, a reformatting, a comment — would change the hash and cause
// a spurious "source changed, please recompile" warning in every medical form
// that stores the script hash in its SDC XML.
//
// WHY TOKEN STREAM, NOT NormalizeWhitespace()
// -------------------------------------------
// Roslyn's SyntaxNode.NormalizeWhitespace() re-formats the tree using its
// own internal formatting rules.  Those rules have changed between Roslyn
// releases (e.g., new-line handling around attributes, trailing comma style)
// and may change again.  If the output of NormalizeWhitespace() changes
// between package upgrades, every pre-stored hash becomes stale instantly —
// a mass false-positive event that cannot be distinguished from real edits.
//
// The TOKEN STREAM approach is stable because Roslyn guarantees that the
// lexical value of a token (its ValueText) is semantically determined and
// does not change across releases.  We simply collect all non-EOF tokens in
// document order, drop all trivia (whitespace, comments), and join with
// single spaces.  The result is unambiguous, minimal, and version-stable.
//
// GOLDEN-ANSWER TEST NOTE (for maintainers)
// ------------------------------------------
// Whenever the Microsoft.CodeAnalysis.CSharp NuGet package is upgraded,
// run the project's golden-answer unit test that pins the exact canonical
// output (and SHA-256 hash) of a known script snippet.  If the test fails,
// the canonicalization algorithm has changed and EVERY pre-stored hash in
// production SDC XML files is now stale.  Investigate before shipping.

using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SDC.ScriptEngine;

/// <summary>
/// Normalizes C# script text into a canonical, whitespace- and comment-free
/// form before computing its SHA-256 hash.
/// </summary>
/// <remarks>
/// The canonical hash is a PERSISTED artifact stored inside SDC XML documents.
/// The algorithm must remain stable across Roslyn package upgrades; token-value
/// semantics are Roslyn's public contract and will not change.
/// </remarks>
public static class SdcScriptCanonicalizer
{
    /// <summary>
    /// Produces a canonical string from <paramref name="scriptText"/> by
    /// extracting every non-EOF syntax token and joining their text values
    /// with single spaces, discarding all trivia (whitespace and comments).
    /// </summary>
    /// <param name="scriptText">
    /// The raw C# script body as the user wrote it (not yet wrapped in the
    /// engine template).
    /// </param>
    /// <returns>
    /// A single line of space-separated token values.  Two scripts that are
    /// semantically identical (same tokens, different whitespace/comments)
    /// will always produce the same canonical string.
    /// </returns>
    public static string Canonicalize(string scriptText)
    {
        // Parse the text into a syntax tree.  We do NOT need a full
        // compilation here; ParseText alone is enough to tokenize.
        var tree = CSharpSyntaxTree.ParseText(scriptText);

        // DescendantTokens() walks all tokens in document order, including
        // tokens inside trivia (e.g., tokens inside #if blocks).
        // We skip EndOfFileToken because it carries no textual content.
        // WithoutTrivia() strips all leading/trailing whitespace and comments
        // from each token, leaving only the raw token text.
        var tokens = tree.GetRoot()
            .DescendantTokens()
            .Where(t => !t.IsKind(SyntaxKind.EndOfFileToken))
            .Select(t => t.WithoutTrivia().ValueText);

        // Join with a single space to produce the canonical form.
        // This string is suitable for hashing but is NOT valid C#;
        // it is purely an internal representation.
        return string.Join(" ", tokens);
    }

    /// <summary>
    /// Computes the lowercase hex-encoded SHA-256 hash of the canonical form
    /// of <paramref name="scriptText"/>.
    /// </summary>
    /// <param name="scriptText">Raw C# script body text.</param>
    /// <returns>
    /// A 64-character lowercase hex string, e.g.
    /// <c>"a3f2...8e1c"</c>, suitable for storage in SDC XML.
    /// </returns>
    /// <remarks>
    /// <para>
    /// ALWAYS call this method (not a raw SHA-256 of the source text) for
    /// every cache key and every value written to SDC XML.  Calling raw
    /// SHA-256 on the source text would make the hash sensitive to
    /// whitespace changes, defeating the purpose.
    /// </para>
    /// <para>
    /// <b>Maintainer note</b>: add a golden-answer test that pins the exact
    /// output of this method for a known input.  If the pinned hash changes
    /// after a Roslyn upgrade, the canonicalization algorithm has drifted and
    /// all pre-stored XML hashes are potentially stale.
    /// </para>
    /// </remarks>
    public static string ComputeCanonicalHash(string scriptText)
    {
        var canonical = Canonicalize(scriptText);

        // SHA256.HashData is a .NET 5+ convenience that hashes in one call
        // without allocating a disposable SHA256 instance.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));

        // Convert.ToHexString produces uppercase hex; ToLowerInvariant for
        // readability and convention (canonical hash is always lowercase).
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
