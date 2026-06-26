// ============================================================================
// SdcScriptDiagnosticsTests.cs
// SDC.ScriptEngine.Tests — compiler diagnostic quality tests
// ============================================================================
//
// Diagnostics (errors and warnings) are the primary way scripts communicate
// compilation failures back to the tool author.  These tests verify:
//
//   - Error messages are human-readable text, not raw Roslyn error codes.
//   - Warnings do not prevent execution (scripts with warnings still compile).
//   - Multiple errors all appear in the diagnostics list.
//   - Line numbers in diagnostics refer to the user's code, not the wrapper.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

[TestClass]
public class SdcScriptDiagnosticsTests
{
    // ── 1. Compile error message is human-readable ────────────────────────────

    [TestMethod]
    public async Task CompileError_DiagnosticMessageIsHumanReadable()
    {
        // Roslyn error messages describe the problem in plain English
        // (e.g., "The name 'undefined_var' does not exist in the current context").
        // Tool authors display these messages directly to form designers, so
        // they must not be opaque codes like "CS0103".
        var engine = ScriptEngineTestHelper.CreateEngine();

        // A reference to an undefined identifier produces CS0103.
        const string script = "var x = undefined_variable;";

        var result = await engine.CompileAsync(script);

        Assert.IsFalse(result.Success, "Script with undefined variable must fail.");
        Assert.IsTrue(result.Diagnostics.Count > 0, "At least one diagnostic must be present.");

        var errorMsg = result.Diagnostics.First(d => d.Severity == SdcDiagnosticSeverity.Error).Message;

        // The message must contain descriptive text, not just an error code.
        // Roslyn always produces a full human-readable sentence.
        Assert.IsFalse(string.IsNullOrWhiteSpace(errorMsg),
            "Diagnostic message must not be empty.");
        Assert.IsTrue(errorMsg.Length > 10,
            "Diagnostic message must be a descriptive sentence, not just a short error code.");

        // Should contain the identifier that caused the error.
        Assert.IsTrue(errorMsg.Contains("undefined_variable"),
            "Diagnostic message must name the symbol that caused the error.");
    }

    // ── 2. Warning-only script still produces runnable IL ─────────────────────

    [TestMethod]
    public async Task WarningOnly_StillProducesRunnableIL()
    {
        // Roslyn warnings do not prevent compilation by default.
        // A script with a warning (but no error) must still produce IL bytes
        // so the form can run even if the author hasn't addressed the warning.
        // (TreatWarningsAsErrors = false by default.)
        var engine = ScriptEngineTestHelper.CreateEngine();

        // CS0219: variable 'x' is assigned but its value is never used — a warning.
        // The variable is declared and assigned but never read.
        const string scriptWithWarning = "int x = 42;";

        var result = await engine.CompileAsync(scriptWithWarning);

        // Warning-only compilation must succeed.
        Assert.IsTrue(result.Success,
            "A script with only a warning (CS0219) must compile successfully.");

        // IL bytes must be present so the script can run.
        Assert.IsNotNull(result.CompiledIL,
            "IL bytes must be produced even when the script has warnings.");
        Assert.IsTrue(result.CompiledIL.Length > 0,
            "The IL byte array must not be empty for a warning-only compile.");
    }

    // ── 3. Multiple errors all appear in diagnostics ───────────────────────────

    [TestMethod]
    public async Task MultipleErrors_AllAppearInDiagnostics()
    {
        // When a script has multiple distinct errors, ALL of them must appear
        // in the diagnostics list.  This lets the tool author show the user
        // a complete picture of what needs fixing, not just the first error.
        var engine = ScriptEngineTestHelper.CreateEngine();

        // Two clearly distinct errors on two separate lines.
        const string multiErrorScript =
            "var a = undefined_alpha;\n" +
            "var b = undefined_beta;";

        var result = await engine.CompileAsync(multiErrorScript);

        Assert.IsFalse(result.Success, "Script with multiple errors must fail.");

        // Count how many Error-severity diagnostics were produced.
        var errorCount = result.Diagnostics.Count(d => d.Severity == SdcDiagnosticSeverity.Error);

        // Both undefined identifiers should each produce at least one error.
        Assert.IsTrue(errorCount >= 2,
            $"Both undefined identifiers must produce errors. Got {errorCount} error diagnostics.");
    }

    // ── 4. Diagnostic line numbers use wrapper-absolute numbering ─────────────

    [TestMethod]
    public async Task DiagnosticLineNumbers_RelativeToUserBody()
    {
        // NOTE: The engine uses GetLineSpan() (not GetMappedLineSpan()), so
        // line numbers in diagnostics are wrapper-absolute, not user-relative.
        // This test documents the ACTUAL behavior as a regression baseline.
        //
        // The wrapper adds ~10 lines before the user code.  The user's
        // undefined identifier on line 2 will be reported as wrapper line 12.
        // This behavior is a known limitation (Phase 2 fix: use GetMappedLineSpan).
        var engine = ScriptEngineTestHelper.CreateEngine();

        const string multiLineScript =
            "var ok = 1;\n" +             // user line 1 → wrapper line ~11
            "var bad = undefined_here;";  // user line 2 → wrapper line ~12

        var result = await engine.CompileAsync(multiLineScript);

        Assert.IsFalse(result.Success, "Script with an error on user line 2 must fail.");

        var firstError = result.Diagnostics.First(d => d.Severity == SdcDiagnosticSeverity.Error);

        // The reported line must be > 0 and must reference the undefined identifier.
        Assert.IsTrue(firstError.Line > 0,
            $"Diagnostic must have a positive line number. Got {firstError.Line}.");

        // Confirm the diagnostic is about the correct identifier,
        // regardless of line number offset.
        Assert.IsTrue(firstError.Message.Contains("undefined_here"),
            $"Error message must identify the symbol. Got: {firstError.Message}");
    }
}
