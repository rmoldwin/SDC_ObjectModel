// ============================================================================
// SdcScriptNode.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// PHASE 1 DESIGN NOTE
// --------------------
// The real SDC OM class hierarchy does not yet have dedicated <Script> elements
// defined in the XSD schema.  Rather than block the engine on schema design,
// Phase 1 uses this standalone POCO as a stand-in for XML round-trip testing.
//
// The POCO carries the three pieces of data that any real SDC script element
// will need:
//   - Source       : the C# script body text
//   - SourceHash   : canonical SHA-256 of Source at compile time
//   - CompiledILBase64 : Roslyn-compiled IL bytes, Base64-encoded for XML storage
//
// PHASE 2 MIGRATION PATH
// -----------------------
// When the SDC schema is extended with a proper Script element type, the
// corresponding generated C# class will gain Source, SourceHash, and
// CompiledILBase64 properties (or equivalents) and this POCO will be retired.
// The engine API (CompileAsync / RunAsync / ExecutePrecompiledAsync) does NOT
// depend on SdcScriptNode; it accepts plain strings and byte arrays, so no
// engine changes are required for the migration.

namespace SDC.ScriptEngine;

/// <summary>
/// Standalone POCO representing a script node stored in an SDC XML document.
/// Used for Phase 1 testing and XML round-trip validation.
/// </summary>
/// <remarks>
/// <para>
/// In a typical round-trip:
/// <list type="number">
///   <item>
///     Set <see cref="Source"/> to the C# script body.
///   </item>
///   <item>
///     Call <see cref="SdcScriptEngine.CompileAsync"/> and store the result:
///     <code>
///     node.SourceHash = compileResult.CanonicalHash;
///     node.SetCompiledIL(compileResult.CompiledIL!);
///     </code>
///   </item>
///   <item>
///     Serialize the node to XML.  Both <see cref="SourceHash"/> and
///     <see cref="CompiledILBase64"/> travel with the document.
///   </item>
///   <item>
///     On load, call <see cref="SdcScriptEngine.ExecutePrecompiledAsync"/>:
///     <code>
///     await engine.ExecutePrecompiledAsync(
///         node.Source, node.SourceHash, node.CompiledILBase64, sdcNode);
///     </code>
///   </item>
/// </list>
/// </para>
/// </remarks>
public class SdcScriptNode
{
    /// <summary>
    /// The C# script body text as written by the form author.
    /// This is the only field the author edits directly.
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// The canonical SHA-256 hex hash of <see cref="Source"/> at the time
    /// <see cref="CompiledILBase64"/> was produced.
    /// </summary>
    /// <remarks>
    /// Computed by <see cref="SdcScriptCanonicalizer.ComputeCanonicalHash"/>.
    /// If <see cref="Source"/> is edited after compilation, the hash the
    /// engine computes at run time will not match this stored value,
    /// triggering the hash-mismatch workflow.
    /// </remarks>
    public string SourceHash { get; set; } = "";

    /// <summary>
    /// Base64-encoded IL bytes produced by Roslyn when <see cref="Source"/>
    /// was last compiled.
    /// </summary>
    /// <remarks>
    /// Empty or null means the script has never been compiled.  The engine
    /// must compile from <see cref="Source"/> before the script can run.
    /// Base64 encoding is used so the bytes are safely embeddable in XML text
    /// content without escaping or CDATA sections.
    /// </remarks>
    public string CompiledILBase64 { get; set; } = "";

    /// <summary>
    /// Decodes <see cref="CompiledILBase64"/> to a raw byte array.
    /// </summary>
    /// <returns>
    /// The IL byte array, or <see langword="null"/> if
    /// <see cref="CompiledILBase64"/> is empty or null.
    /// </returns>
    public byte[]? GetCompiledIL()
        => string.IsNullOrEmpty(CompiledILBase64)
            ? null
            : Convert.FromBase64String(CompiledILBase64);

    /// <summary>
    /// Encodes <paramref name="bytes"/> as Base64 and stores the result in
    /// <see cref="CompiledILBase64"/>.
    /// </summary>
    /// <param name="bytes">The IL bytes from a Roslyn compilation.</param>
    public void SetCompiledIL(byte[] bytes)
        => CompiledILBase64 = Convert.ToBase64String(bytes);
}
