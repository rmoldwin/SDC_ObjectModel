// ============================================================================
// SdcScriptTemplate.cs
// SDC.ScriptEngine — platform-agnostic Roslyn script engine for the SDC OM
// ============================================================================
//
// USER-FACING CONTRACT
// ---------------------
// The user writes only the BODY of the Execute method.  The engine wraps it
// in a complete, compilable C# file by substituting the body into the
// template below.  This means:
//
//   1. The user does not need to write class or method declarations.
//   2. The user gets the SDC OM types in scope via the `using` directives.
//   3. The user's first line of code is always line 1 in compiler diagnostics
//      (thanks to the #line directive — see below).
//
// THE #line PREPROCESSOR DIRECTIVE
// ----------------------------------
// Without any adjustment, a compile error on the user's first line would be
// reported as occurring at line 10+ (wherever that line falls inside the
// generated wrapper).  This is confusing for tool authors displaying errors.
//
// Inserting `#line 1 "script"` immediately before the user's code resets
// Roslyn's internal line counter.  After the reset:
//   - Line 1 in Roslyn's eyes = the user's first line of code.
//   - Error messages are reported relative to the user's code, not the wrapper.
//   - The virtual file name "script" appears in diagnostic messages instead of
//     a generated assembly name, making the origin obvious.
//
// `#line default` after the user's code restores normal line numbering for
// any code that might follow (there is none currently, but this is good hygiene).
//
// PLACEHOLDER
// -----------
// The string literal "{USER_SCRIPT}" is replaced by Wrap().  This simple
// substitution is intentional; a more complex templating engine would add a
// dependency and complexity that is not warranted here.

namespace SDC.ScriptEngine;

/// <summary>
/// Provides the C# source code wrapper that turns a user-supplied script body
/// into a compilable assembly containing a static <c>SdcScript.Execute</c> method.
/// </summary>
internal static class SdcScriptTemplate
{
    /// <summary>
    /// The C# source template.  The user's script body is inserted at the
    /// <c>{USER_SCRIPT}</c> placeholder, which is surrounded by
    /// <c>#line</c> directives that map compiler errors back to the user's
    /// line numbers.
    /// </summary>
    /// <remarks>
    /// The <c>#line 1 "script"</c> directive immediately precedes the user
    /// code, so any Roslyn error at the user's first line is reported as
    /// "line 1, file script" — matching what the user sees in an editor.
    /// </remarks>
    public const string Template =
        "using System;\n" +
        "using System.Linq;\n" +
        "using System.Collections.Generic;\n" +
        "using SDC.Schema;\n" +
        "\n" +
        "public static class SdcScript\n" +
        "{\n" +
        "    public static void Execute(BaseType sdc)\n" +
        "    {\n" +
        "#line 1 \"script\"\n" +
        "        {USER_SCRIPT}\n" +
        "#line default\n" +
        "    }\n" +
        "}\n";

    /// <summary>
    /// Substitutes the user's script body into the template and returns a
    /// complete, compilable C# source string.
    /// </summary>
    /// <param name="userScriptBody">
    /// The raw C# statements written by the user (the method body only,
    /// no class or method declaration).
    /// </param>
    /// <returns>
    /// A complete C# source file as a string, ready to be parsed by Roslyn
    /// via <c>CSharpSyntaxTree.ParseText()</c>.
    /// </returns>
    public static string Wrap(string userScriptBody)
        => Template.Replace("{USER_SCRIPT}", userScriptBody);
}
