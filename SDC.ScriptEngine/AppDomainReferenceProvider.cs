// ============================================================================
// AppDomainReferenceProvider.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// DESKTOP-ONLY IMPLEMENTATION
// ----------------------------
// This class is the simplest (and most common) implementation of
// ISdcScriptReferenceProvider.  It works by:
//
//   1. Force-loading the assemblies that user scripts will typically need but
//      that may not yet have been touched at the time compilation runs.
//   2. Enumerating every assembly currently loaded in AppDomain.CurrentDomain.
//   3. Wrapping each disk-backed assembly in a MetadataReference.
//
// QUIRK: IN-MEMORY / DYNAMIC ASSEMBLIES
// ----------------------------------------
// AppDomain.GetAssemblies() returns ALL loaded assemblies, including:
//   - Assemblies generated at runtime via Reflection.Emit (e.g., by Moq,
//     Castle.DynamicProxy, or EF Core model builders).
//   - Assemblies loaded from byte arrays (no on-disk path).
//
// These assemblies have Location == "" (empty string).  Calling
// MetadataReference.CreateFromFile("") would throw an ArgumentException.
// We skip them.  Because their APIs are not part of the stable SDC OM surface,
// this omission does not affect normal script compilation.
//
// BLAZOR WASM NOTE
// -----------------
// Do NOT use this class in a Blazor WASM project.  On WASM there is no
// AppDomain file-based enumeration.  Provide a custom ISdcScriptReferenceProvider
// that fetches DLL bytes via HttpClient instead.

using Microsoft.CodeAnalysis;

namespace SDC.ScriptEngine;

/// <summary>
/// Desktop implementation of <see cref="ISdcScriptReferenceProvider"/>.
/// Enumerates all assemblies currently loaded in the CLR AppDomain and
/// wraps each file-backed assembly in a <see cref="MetadataReference"/> for
/// Roslyn to use during C# script compilation.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is suitable for desktop runtimes (WPF, WinForms, MSTest,
/// console).  It is <b>not</b> compatible with Blazor WASM, which has no
/// file-based AppDomain enumeration.
/// </para>
/// <para>
/// <b>Important</b>: Assemblies that have not yet been loaded at the moment
/// <see cref="GetReferences"/> is called will be absent from the reference
/// list and unavailable to user scripts.  The constructor force-loads the
/// assemblies that are most commonly needed.
/// </para>
/// </remarks>
public class AppDomainReferenceProvider : ISdcScriptReferenceProvider
{
    /// <inheritdoc />
    public IReadOnlyList<MetadataReference> GetReferences()
    {
        // Force-load assemblies that are commonly needed by SDC scripts but
        // may not yet have been accessed (and therefore not yet loaded into
        // the AppDomain) by the time this method is called.
        //
        // The underscore discard idiom suppresses the "unused variable"
        // compiler warning; we only care about the side-effect of loading.
        _ = typeof(SDC.Schema.BaseType).Assembly;           // SDC.Schema
        _ = typeof(Newtonsoft.Json.JsonConvert).Assembly;   // Newtonsoft.Json
        _ = typeof(System.Linq.Enumerable).Assembly;        // System.Linq / System.Core
        _ = typeof(object).Assembly;                        // System.Private.CoreLib

        var refs = new List<MetadataReference>();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Skip in-memory / dynamic assemblies that were generated at
            // runtime (e.g., by Reflection.Emit, Moq, Castle.DynamicProxy).
            // These assemblies have Location == "" and no on-disk file to read.
            // MetadataReference.CreateFromFile would throw for them.
            if (string.IsNullOrEmpty(asm.Location))
                continue;

            try
            {
                // CreateFromFile reads the PE metadata from the assembly's DLL.
                // Roslyn uses this exactly like a /reference compiler flag.
                refs.Add(MetadataReference.CreateFromFile(asm.Location));
            }
            catch
            {
                // Best-effort: skip any assembly that cannot be read (e.g.,
                // collectible ALC assemblies whose backing store is gone, or
                // assemblies in unusual locations with restricted permissions).
            }
        }

        return refs;
    }
}
