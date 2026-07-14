// ============================================================================
// MainWindow.xaml.cs
// SDC.ScriptEngine.WpfTest — interactive WPF test harness (.NET 10, Windows)
// ============================================================================
//
// PURPOSE
// -------
// This file drives the SDC.ScriptEngine library on a full desktop JIT runtime,
// proving that the IL compile → load → run → mutate-OM-node pipeline works
// correctly outside a browser context.
//
// ASYNC NOTES
// -----------
// WPF dispatches all UI events on the UI thread.  The engine's async methods
// (CompileAsync, RunAsync, ExecutePrecompiledAsync) use Task.Run internally so
// Roslyn work happens off the UI thread.  We await them from async void event
// handlers (the only safe place to use async void in WPF) and update controls
// after the await — by that point we are back on the UI thread, so no
// Dispatcher.Invoke is needed.
//
// WHY AppDomainReferenceProvider
// --------------------------------
// AppDomainReferenceProvider walks AppDomain.CurrentDomain.GetAssemblies() and
// returns MetadataReferences for every loaded assembly.  This ensures Roslyn can
// resolve SDC.Schema types (QuestionItemType, BaseType, etc.) inside the compiled
// script.  A Blazor WASM host would use a different provider that fetches DLL bytes
// via HttpClient instead.
//
// FIELD SUMMARY
// -------------
//   _engine            — single SdcScriptEngine instance, reused across all
//                        button clicks (Roslyn loads lazily on first compile).
//   _lastCompileResult — result from the most recent CompileAsync call; needed
//                        so [Run] can use the IL without recompiling.
//   _lastScriptNode    — SdcScriptNode POCO built after each compile; holds
//                        Source, SourceHash, and CompiledILBase64 for the
//                        pre-compiled-IL path test.

using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using SDC.Schema;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.WpfTest;

/// <summary>
/// Interactive test harness for SDC.ScriptEngine on the desktop JIT runtime.
/// </summary>
public partial class MainWindow : Window
{
    // ── Engine and state fields ───────────────────────────────────────────────

    /// <summary>
    /// The single engine instance shared across all button handlers.
    /// Creating it is cheap; Roslyn loads lazily on the first CompileAsync call.
    /// </summary>
    private readonly SdcScriptEngine _engine;

    /// <summary>
    /// The result of the last successful CompileAsync call.
    /// Null if the script has never been compiled or the last compile failed.
    /// </summary>
    private SdcScriptCompileResult? _lastCompileResult;

    /// <summary>
    /// A SdcScriptNode POCO populated after each successful compile.
    /// Used by [Test Pre-Compiled IL Path] to simulate the WASM load-from-XML path.
    /// </summary>
    private SdcScriptNode? _lastScriptNode;

    // ── Constructor ───────────────────────────────────────────────────────────

    public MainWindow()
    {
        InitializeComponent();

        // AppDomainReferenceProvider uses AppDomain.CurrentDomain.GetAssemblies()
        // so Roslyn can resolve SDC.Schema types inside the script.
        // On first compile, Roslyn JITs the compiler itself — expect ~1–3 s.
        // Subsequent compiles reuse cached Roslyn state and are typically <100 ms.
        _engine = new SdcScriptEngine(new AppDomainReferenceProvider());

        // Compute the initial hash of the pre-filled demo script so the Hash
        // Inspector section is populated as soon as the window opens.
        RefreshComputedHash();
    }

    // ── Helper: create a fresh SDC OM node for each run ───────────────────────

    /// <summary>
    /// Builds a new <see cref="QuestionItemType"/> node with a well-known initial
    /// <c>name</c> value so the before/after diff in the Results panel is clear.
    /// </summary>
    /// <remarks>
    /// A new node is created for every Run so each test starts clean and
    /// mutations from a previous run don't bleed into the next one.
    /// <para>
    /// Constructor signature: QuestionItemType(BaseType? parentNode, string id = "")
    /// DataElementType acts as a minimal top-level parent so the node registers
    /// itself in the OM's internal node registry.
    /// </para>
    /// </remarks>
    private static QuestionItemType CreateFreshNode()
    {
        // DataElementType is a top-level SDC container; passing null makes it
        // a standalone root (no XML package parent required for testing).
        var de = new DataElementType(null, "de-wpf-test");
        var q  = new QuestionItemType(de, "q-wpf-test");
        q.name = "Original Name";
        return q;
    }

    // ── Button: [Compile] ─────────────────────────────────────────────────────

    /// <summary>
    /// Compiles the current script text using Roslyn and caches the IL bytes.
    /// </summary>
    /// <remarks>
    /// On success, populates _lastCompileResult and _lastScriptNode so the
    /// [Run] and [Test Pre-Compiled IL Path] buttons have IL to work with.
    /// </remarks>
    private async void BtnCompile_Click(object sender, RoutedEventArgs e)
    {
        // Disable all action buttons while async work is in flight so the
        // developer cannot queue overlapping engine operations.
        SetButtonsEnabled(false);
        try
        {
            ClearResults();
            var scriptText = TxtScript.Text;

            var sw = Stopwatch.StartNew();
            _lastCompileResult = await _engine.CompileAsync(scriptText);
            sw.Stop();

            DisplayCompileResult(_lastCompileResult, sw.ElapsedMilliseconds);

            if (_lastCompileResult.Success)
            {
                // Build the SdcScriptNode POCO for the pre-compiled-IL path test.
                // We do this here (not in [Run]) because the node is the "stored XML"
                // equivalent — source + hash + IL — that would be serialised to disk.
                _lastScriptNode = new SdcScriptNode
                {
                    Source = scriptText,
                    SourceHash = _lastCompileResult.CanonicalHash,
                };
                _lastScriptNode.SetCompiledIL(_lastCompileResult.CompiledIL!);

                // Reflect the stored hash in the Hash Inspector.
                TxtStoredHash.Text = _lastScriptNode.SourceHash;
                RefreshHashStatus();
            }
        }
        catch (Exception ex)
        {
            DisplayError("Compile error (unexpected exception)", ex);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    // ── Button: [Run (use cached IL)] ────────────────────────────────────────

    /// <summary>
    /// Runs the most recently compiled IL against a fresh SDC OM node.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="SdcScriptEngine.RunAsync"/> which accepts raw IL bytes
    /// and the canonical hash; no recompilation occurs.
    /// </remarks>
    private async void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        if (_lastCompileResult is null || !_lastCompileResult.Success)
        {
            ShowMessage("No cached IL — click [Compile] first.", isError: true);
            return;
        }

        SetButtonsEnabled(false);
        try
        {
            ClearResults();

            // Create a fresh node so each run starts with "Original Name".
            var node = CreateFreshNode();
            TxtBefore.Text = $"name = \"{node.name}\"";

            var sw = Stopwatch.StartNew();
            var runResult = await _engine.RunAsync(
                _lastCompileResult.CompiledIL!,
                _lastCompileResult.CanonicalHash,
                node);
            sw.Stop();

            TxtCompileTime.Text = "0 ms (used cached IL)";
            DisplayRunResult(runResult, node, sw.ElapsedMilliseconds, _lastCompileResult);
        }
        catch (Exception ex)
        {
            DisplayError("Run error (unexpected exception)", ex);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    // ── Button: [Compile + Run] ───────────────────────────────────────────────

    /// <summary>
    /// Compiles the script and immediately runs it — a convenience shortcut.
    /// </summary>
    private async void BtnCompileAndRun_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        try
        {
            ClearResults();
            var scriptText = TxtScript.Text;

            // ── Compile phase ────────────────────────────────────────────────
            var compileSw = Stopwatch.StartNew();
            _lastCompileResult = await _engine.CompileAsync(scriptText);
            compileSw.Stop();

            DisplayCompileResult(_lastCompileResult, compileSw.ElapsedMilliseconds);

            if (!_lastCompileResult.Success)
                return; // diagnostics already shown; no point running

            // Update SdcScriptNode for the pre-compiled-IL path test.
            _lastScriptNode = new SdcScriptNode
            {
                Source = scriptText,
                SourceHash = _lastCompileResult.CanonicalHash,
            };
            _lastScriptNode.SetCompiledIL(_lastCompileResult.CompiledIL!);
            TxtStoredHash.Text = _lastScriptNode.SourceHash;
            RefreshHashStatus();

            // ── Run phase ────────────────────────────────────────────────────
            var node = CreateFreshNode();
            TxtBefore.Text = $"name = \"{node.name}\"";

            var runSw = Stopwatch.StartNew();
            var runResult = await _engine.RunAsync(
                _lastCompileResult.CompiledIL!,
                _lastCompileResult.CanonicalHash,
                node);
            runSw.Stop();

            DisplayRunResult(runResult, node, runSw.ElapsedMilliseconds, _lastCompileResult);
        }
        catch (Exception ex)
        {
            DisplayError("Compile+Run error (unexpected exception)", ex);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    // ── Button: [Test Pre-Compiled IL Path] ───────────────────────────────────

    /// <summary>
    /// Simulates the Blazor WASM scenario: a pre-compiled script node loaded
    /// from XML is executed without triggering a recompile.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In a WASM context, compiling with Roslyn is expensive (Roslyn must be
    /// deployed as WASM modules) so the SDC document stores the compiled IL in
    /// Base64 alongside the source hash.  On load, the engine calls
    /// <see cref="SdcScriptEngine.ExecutePrecompiledAsync"/>, which:
    ///   1. Recomputes the canonical hash of the source text.
    ///   2. Compares it to the stored hash (from SdcScriptNode.SourceHash).
    ///   3. If they match, loads the stored IL directly — zero Roslyn work.
    ///   4. If they mismatch, applies the OnHashMismatch policy (default: Throw).
    /// </para>
    /// <para>
    /// When the source text in the editor matches what was compiled, compile time
    /// should be 0 ms.  Edit the script after compiling, hit this button, and
    /// watch it detect a MISMATCH.
    /// </para>
    /// </remarks>
    private async void BtnTestPrecompiled_Click(object sender, RoutedEventArgs e)
    {
        if (_lastScriptNode is null)
        {
            ShowMessage("Compile first to generate IL.", isError: true);
            return;
        }

        SetButtonsEnabled(false);
        try
        {
            ClearResults();

            var node = CreateFreshNode();
            TxtBefore.Text = $"name = \"{node.name}\"";

            // ExecutePrecompiledAsync: uses stored IL if source hash matches;
            // throws ScriptHashMismatchException if source was edited after compile.
            var sw = Stopwatch.StartNew();
            var runResult = await _engine.ExecutePrecompiledAsync(
                _lastScriptNode.Source,           // source text at compile time
                _lastScriptNode.SourceHash,       // stored hash (from compile)
                _lastScriptNode.CompiledILBase64, // stored IL (Base64)
                node);
            sw.Stop();

            // 0 ms compile because we used the stored IL path.
            TxtCompileTime.Text = "0 ms (pre-compiled IL loaded from SdcScriptNode)";
            DisplayRunResult(runResult, node, sw.ElapsedMilliseconds, _lastCompileResult);
        }
        catch (ScriptHashMismatchException hmEx)
        {
            // This is the expected error when the developer edits the script
            // after compiling and then clicks this button.
            TxtStatus.Text = "⚠️ Hash Mismatch";
            TxtStatus.Foreground = Brushes.DarkOrange;
            TxtDiagnostics.Text =
                $"ScriptHashMismatchException:\n{hmEx.Message}\n\n" +
                $"The script source was edited after the IL was compiled.\n" +
                $"Recompile to generate fresh IL, then retry.";
            RefreshHashStatus();
        }
        catch (Exception ex)
        {
            DisplayError("Pre-compiled IL path error (unexpected exception)", ex);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    // ── Button: [Re-check] (Hash Inspector) ──────────────────────────────────

    /// <summary>
    /// Recomputes the hash of the current script text without invoking Roslyn,
    /// then compares it against the stored hash from the last compile.
    /// </summary>
    private void BtnRecheck_Click(object sender, RoutedEventArgs e)
    {
        RefreshComputedHash();
        RefreshHashStatus();
    }

    // ── Copy buttons ──────────────────────────────────────────────────────────

    private void BtnCopyComputedHash_Click(object sender, RoutedEventArgs e)
        => TryCopy(TxtComputedHash.Text);

    private void BtnCopyStoredHash_Click(object sender, RoutedEventArgs e)
        => TryCopy(TxtStoredHash.Text);

    private static void TryCopy(string text)
    {
        if (!string.IsNullOrEmpty(text))
            Clipboard.SetText(text);
    }

    // ── Display helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Populates the Results panel with compile outcome data.
    /// Called after every CompileAsync invocation.
    /// </summary>
    private void DisplayCompileResult(SdcScriptCompileResult result, long compileMs)
    {
        TxtCompileTime.Text = $"{compileMs} ms";
        TxtCanonicalHash.Text = result.CanonicalHash;
        TxtComputedHash.Text = result.CanonicalHash;

        if (result.Success)
        {
            TxtILSize.Text = $"{result.CompiledIL!.Length:N0} bytes";
            TxtDiagnostics.Text = result.Diagnostics.Count == 0
                ? "(none)"
                : string.Join("\n", result.Diagnostics.Select(d => $"[{d.Severity}] L{d.Line}:{d.Column} {d.Message}"));
        }
        else
        {
            TxtStatus.Text = "❌ Compile Failed";
            TxtStatus.Foreground = Brushes.Red;
            TxtDiagnostics.Text = string.Join("\n",
                result.Diagnostics.Select(d => $"[{d.Severity}] L{d.Line}:{d.Column} {d.Message}"));
        }
    }

    /// <summary>
    /// Populates the Results panel with run outcome data.
    /// </summary>
    private void DisplayRunResult(
        SdcScriptRunResult result,
        QuestionItemType node,
        long runMs,
        SdcScriptCompileResult? compileResult)
    {
        TxtRunTime.Text = $"{runMs} ms";

        if (compileResult is not null)
        {
            TxtILSize.Text = $"{compileResult.CompiledIL?.Length ?? 0:N0} bytes";
            TxtCanonicalHash.Text = compileResult.CanonicalHash;
        }

        if (result.Success)
        {
            TxtStatus.Text = "✅ Success";
            TxtStatus.Foreground = Brushes.DarkGreen;
            TxtAfter.Text = $"name = \"{node.name}\"";
            TxtDiagnostics.Text = "(none)";
        }
        else
        {
            TxtStatus.Text = "❌ Script Runtime Error";
            TxtStatus.Foreground = Brushes.Red;
            TxtAfter.Text = $"name = \"{node.name}\"  (unchanged — script threw)";
            TxtDiagnostics.Text = result.ErrorMessage ?? result.Exception?.ToString() ?? "Unknown error";
        }
    }

    /// <summary>Displays an unexpected (non-script) exception.</summary>
    private void DisplayError(string context, Exception ex)
    {
        TxtStatus.Text = "❌ Error";
        TxtStatus.Foreground = Brushes.Red;
        TxtDiagnostics.Text = $"{context}:\n{ex}";
    }

    /// <summary>Shows a short status message (e.g., "Compile first").</summary>
    private void ShowMessage(string message, bool isError)
    {
        TxtStatus.Text = message;
        TxtStatus.Foreground = isError ? Brushes.OrangeRed : Brushes.DarkBlue;
    }

    /// <summary>Resets all result fields to their blank/initial state.</summary>
    private void ClearResults()
    {
        TxtStatus.Text = "—";
        TxtStatus.Foreground = Brushes.Black;
        TxtBefore.Text = "";
        TxtAfter.Text = "";
        TxtCompileTime.Text = "";
        TxtRunTime.Text = "";
        TxtILSize.Text = "";
        TxtCanonicalHash.Text = "";
        TxtDiagnostics.Text = "(none)";
    }

    // ── Hash Inspector helpers ────────────────────────────────────────────────

    /// <summary>
    /// Computes the canonical hash of the current script text and updates
    /// the Computed Hash field in the Hash Inspector section.
    /// </summary>
    /// <remarks>
    /// This is a lightweight operation (just SHA-256 + normalisation) —
    /// it does NOT invoke Roslyn.
    /// </remarks>
    private void RefreshComputedHash()
    {
        var hash = SdcScriptHashInspector.ComputeHash(TxtScript.Text);
        TxtComputedHash.Text = hash;
    }

    /// <summary>
    /// Compares the computed hash against the stored hash and updates the
    /// status label in the Hash Inspector section.
    /// </summary>
    private void RefreshHashStatus()
    {
        var stored = TxtStoredHash.Text;

        if (string.IsNullOrEmpty(stored))
        {
            TxtHashStatus.Text       = "(compile to populate stored hash)";
            TxtHashStatus.Foreground = Brushes.Gray;
            return;
        }

        // SdcScriptHashInspector.Verify performs a case-insensitive comparison
        // and returns a human-readable StatusMessage.
        var result = SdcScriptHashInspector.Verify(TxtScript.Text, stored);

        if (result.HashesMatch)
        {
            TxtHashStatus.Text       = "✅ MATCH — pre-compiled IL is current";
            TxtHashStatus.Foreground = Brushes.DarkGreen;
        }
        else
        {
            TxtHashStatus.Text       = "⚠️ MISMATCH — source was edited after IL was compiled";
            TxtHashStatus.Foreground = Brushes.DarkOrange;
        }
    }

    // ── UI state helper ───────────────────────────────────────────────────────

    /// <summary>
    /// Enables or disables all four action buttons simultaneously.
    /// Used to prevent overlapping async engine operations.
    /// </summary>
    private void SetButtonsEnabled(bool enabled)
    {
        BtnCompile.IsEnabled         = enabled;
        BtnRun.IsEnabled             = enabled;
        BtnCompileAndRun.IsEnabled   = enabled;
        BtnTestPrecompiled.IsEnabled = enabled;
    }
}