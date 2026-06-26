// ============================================================================
// SdcScriptLoadContext.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// WHY A CUSTOM AssemblyLoadContext?
// ----------------------------------
// When the .NET runtime loads an assembly, it resolves the assembly's
// dependencies (other assemblies it references).  By default, that resolution
// happens in the DEFAULT AssemblyLoadContext (ALC).
//
// If we naively called Assembly.Load(compiledIL) without a custom ALC, the
// runtime would load the script assembly into the default ALC and everything
// would share the same type objects.  So far so good.
//
// The problem arises when we later want to UNLOAD a script (to free memory
// after a re-compilation).  Only COLLECTIBLE ALCs can be unloaded.  The
// default ALC is NOT collectible.  So we need a custom collectible ALC to
// host each script assembly.
//
// THE TYPE-IDENTITY PROBLEM
// --------------------------
// Suppose our custom ALC loads the script AND also loads a fresh copy of
// SDC.Schema.dll to satisfy the script's dependency on BaseType.
//
// Now there are TWO versions of the BaseType class in the process:
//   - The host's BaseType  (from the default ALC)
//   - The script's BaseType (from our custom ALC)
//
// These are DIFFERENT Type objects even though they have the same name and
// come from the same DLL on disk.  .NET's type system considers them
// incompatible.  When the host calls Execute(hostBaseTypeInstance), the
// runtime throws InvalidCastException because hostBaseTypeInstance is of
// "host BaseType", not "script BaseType".
//
// THE FIX: Override Load() to return null
// -----------------------------------------
// When Load() returns null, the runtime falls back to the DEFAULT ALC to
// resolve the dependency.  The default ALC returns the host's already-loaded
// copy of SDC.Schema.  Script and host now share the SAME BaseType Type
// object.  Casts work.  MethodInfo.Invoke works.  Problem solved.
//
// This pattern is described in the .NET documentation as
// "Resolving dependencies via the parent ALC" and is the canonical solution
// for plugin/scripting scenarios where the plugin must interoperate with host
// objects by reference, not by value copy.
//
// WASM NOTE
// ----------
// On Blazor WASM (mono/wasm runtime), AssemblyLoadContext with
// isCollectible: true is supported in principle, but the mono GC may not
// actually unload the ALC when there are no live references — treat WASM
// deployments as accumulating-only for Phase 1.  This does NOT affect
// correctness, only memory growth over many recompilations.

using System.Reflection;
using System.Runtime.Loader;

namespace SDC.ScriptEngine;

/// <summary>
/// A custom, collectible <see cref="AssemblyLoadContext"/> that hosts exactly
/// one compiled script assembly and delegates ALL dependency resolution to the
/// default ALC, preserving type identity between host and script objects.
/// </summary>
/// <remarks>
/// <para>
/// <b>Critical design decision</b>: <see cref="Load"/> always returns
/// <see langword="null"/>, forcing the runtime to resolve every dependency
/// (SDC.Schema, Newtonsoft.Json, System.*, etc.) through the default ALC.
/// This means the host's live instances of <c>BaseType</c> (and all other
/// SDC OM types) are the exact same <see cref="Type"/> objects seen by the
/// script, so casting and method invocation work correctly.
/// </para>
/// <para>
/// <b>Lifecycle</b>: keep at least one reference to the
/// <see cref="SdcScriptEntry"/> that owns this context alive for as long as
/// script invocations may occur.  When all references are released the GC
/// can collect this ALC and unload the script assembly (desktop only).
/// </para>
/// </remarks>
internal sealed class SdcScriptLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// Creates a new collectible load context for one script assembly.
    /// </summary>
    public SdcScriptLoadContext()
        : base(isCollectible: true)
    {
        // isCollectible: true — this ALC (and the assembly it contains) can
        // be garbage-collected once no live references remain.
        //
        // On desktop .NET this enables memory recovery after re-compilation
        // (the old script ALC is collected while the new one becomes active).
        //
        // On Blazor WASM / mono the collector may not fully unload the ALC in
        // Phase 1, so treat WASM as accumulating; this is acceptable for the
        // expected low number of distinct scripts per session.
    }

    /// <summary>
    /// Loads a compiled script assembly from IL bytes into this context.
    /// </summary>
    /// <param name="compiledIL">
    /// The raw IL bytes produced by <c>CSharpCompilation.Emit()</c>.
    /// </param>
    /// <returns>The loaded <see cref="Assembly"/>.</returns>
    /// <remarks>
    /// We use <see cref="AssemblyLoadContext.LoadFromStream"/> rather than
    /// <c>LoadFromAssemblyPath</c> because the script assembly is never
    /// written to disk — it exists only as an in-memory byte array.
    /// </remarks>
    public Assembly LoadScript(byte[] compiledIL)
        => LoadFromStream(new MemoryStream(compiledIL));
    // MemoryStream wraps the array without copying; the stream is readable
    // by LoadFromStream which reads the PE header from it.  The stream
    // instance can be GC'd immediately after LoadFromStream returns;
    // the assembly is now fully resident in managed memory via this ALC.

    /// <summary>
    /// Intentionally returns <see langword="null"/> for ALL assembly name
    /// lookups, forcing the runtime to fall back to the default ALC.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to resolve.</param>
    /// <returns>Always <see langword="null"/>.</returns>
    /// <remarks>
    /// Returning null here is the correct and intentional behavior.
    /// See the class-level documentation for the full explanation of WHY
    /// this is needed to prevent type-identity mismatches between the host
    /// and script contexts.
    /// </remarks>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Deliberately do NOT attempt to load any assembly.
        // Returning null signals: "I don't know how to load this; let the
        // runtime ask the default ALC."  The default ALC will find and
        // return the host's already-loaded copy, preserving type identity.
        return null;
    }
}
