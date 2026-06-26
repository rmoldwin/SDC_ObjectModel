// ============================================================================
// ISdcScriptReferenceProvider.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// WHY THIS INTERFACE EXISTS
// --------------------------
// Roslyn needs assembly metadata references (like a compiler's /reference
// flags) to resolve types used in a script.  How those references are obtained
// differs radically between platforms:
//
//   Desktop (.NET 10 CLR): Assemblies are loaded into an AppDomain.
//     We can enumerate AppDomain.CurrentDomain.GetAssemblies() and wrap each
//     one in MetadataReference.CreateFromFile(asm.Location).
//
//   Blazor WASM (mono/wasm runtime): There is no AppDomain.GetAssemblies()
//     that returns on-disk paths.  The WASM host must fetch DLL bytes via
//     HttpClient and create MetadataReference objects from those byte arrays.
//
// By hiding this platform-specific concern behind an interface, SDC.ScriptEngine
// itself stays 100% platform-agnostic.  The caller (test app, WPF app, or Blazor
// page) creates the appropriate provider and passes it to SdcScriptEngine.

using Microsoft.CodeAnalysis;

namespace SDC.ScriptEngine;

/// <summary>
/// Abstracts the mechanism by which Roslyn obtains assembly metadata references
/// for C# script compilation.
/// </summary>
/// <remarks>
/// <para>
/// Different platforms supply references differently:
/// <list type="bullet">
///   <item><b>Desktop</b>: reads from assemblies already loaded in the AppDomain.</item>
///   <item><b>Blazor WASM</b>: fetches DLL bytes via <c>HttpClient</c> and creates
///         <see cref="MetadataReference"/> from the image bytes.</item>
/// </list>
/// </para>
/// <para>
/// This interface is the primary seam that makes <c>SDC.ScriptEngine</c>
/// platform-agnostic.  Provide a platform-specific implementation when
/// constructing <see cref="SdcScriptEngine"/>.
/// </para>
/// </remarks>
public interface ISdcScriptReferenceProvider
{
    /// <summary>
    /// Returns the set of <see cref="MetadataReference"/> objects that represent
    /// every assembly whose public API the user script is allowed to call.
    /// </summary>
    /// <returns>
    /// A read-only list of metadata references.  Roslyn treats each entry like a
    /// <c>/reference</c> flag on the command-line compiler — the script can only
    /// see and call into assemblies that appear here.
    /// </returns>
    /// <remarks>
    /// The implementation may be called once per compilation.  Avoid heavyweight
    /// I/O or async work inside this method; cache the result internally if needed.
    /// </remarks>
    IReadOnlyList<MetadataReference> GetReferences();
}
