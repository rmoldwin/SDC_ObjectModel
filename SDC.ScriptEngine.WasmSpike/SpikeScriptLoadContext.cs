using System.Reflection;
using System.Runtime.Loader;

namespace SDC.ScriptEngine.WasmSpike;

/// <summary>
/// A minimal, collectible AssemblyLoadContext that hosts one Roslyn-compiled script assembly.
///
/// KEY DESIGN DECISION — Why Load() returns null for everything:
/// ------------------------------------------------------------------
/// When Roslyn compiles the script, it embeds symbolic references to every type the script uses
/// (e.g. BaseType, QuestionItemType). Those references are resolved at runtime by the ALC's
/// Load() callback.
///
/// If Load() returned a FRESH copy of SDC.Schema for this child ALC, the runtime would have
/// two separate Type objects for BaseType — one in the host's Default ALC and one here.
/// Even though they have the same fully-qualified name, the CLR treats them as different types.
/// The cast "(QuestionItemType)sdc" in the script would then throw InvalidCastException because
/// the host passes a Default-ALC BaseType into a method whose parameter is child-ALC BaseType.
///
/// By returning null here, every assembly resolution falls through to the Default ALC, which
/// means the script and the host share the EXACT SAME Type objects. The cast succeeds, and
/// mutations to the object are immediately visible on the calling side.
///
/// For more background see:
///   https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext
/// </summary>
internal sealed class SpikeScriptLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// Creates a collectible (GC-able) ALC. Collectible means the loaded assemblies
    /// can be unloaded when this context is no longer referenced — important for a script
    /// engine that may compile many scripts over its lifetime.
    /// </summary>
    public SpikeScriptLoadContext() : base(isCollectible: true) { }

    /// <summary>
    /// Load compiled IL bytes into this ALC and return the resulting Assembly.
    /// The assembly is "owned" by this ALC, but all its type dependencies resolve
    /// through the Default ALC (because Load() returns null below).
    /// </summary>
    public Assembly LoadScript(byte[] compiledIL)
        => LoadFromStream(new MemoryStream(compiledIL));

    /// <summary>
    /// CRITICAL: Return null for EVERYTHING.
    ///
    /// When the runtime needs to resolve a dependency of the script assembly
    /// (e.g. SDC.Schema, System.Runtime, etc.) it calls this method first.
    /// Returning null tells the runtime "I don't handle this; use the Default ALC".
    /// The Default ALC already has all these assemblies loaded, so resolution succeeds
    /// and type identity is preserved across the ALC boundary.
    /// </summary>
    protected override Assembly? Load(AssemblyName assemblyName) => null;
}
