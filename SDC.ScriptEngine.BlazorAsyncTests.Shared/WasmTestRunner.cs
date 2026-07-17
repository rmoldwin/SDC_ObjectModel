using System.Diagnostics;

namespace SDC.ScriptEngine.BlazorAsyncTests.Shared;

/// <summary>
/// Runs <see cref="WasmTestCase"/> objects sequentially (never concurrently) to avoid
/// <c>LastTopNode</c> corruption that would result from interleaved tree builds.
/// </summary>
public class WasmTestRunner
{
    public List<WasmTestResult> Results { get; } = new();

    /// <summary>Fires after each test completes (passed, failed, or skipped).</summary>
    public event Action<WasmTestResult>? TestCompleted;

    /// <summary>
    /// True when the WASM runtime has multiple logical processors (i.e., WasmEnableThreads=true).
    /// False in Phase 1 (single-threaded mode).
    /// </summary>
    public bool WasmThreadsEnabled => Environment.ProcessorCount > 1;

    /// <summary>
    /// Runs every test in <paramref name="tests"/> sequentially.
    /// Tests marked <see cref="WasmTestCase.RequiresThreads"/> are skipped when
    /// <see cref="WasmThreadsEnabled"/> is false.
    /// </summary>
    public async Task RunAllAsync(IReadOnlyList<WasmTestCase> tests, CancellationToken ct = default)
    {
        Results.Clear();
        foreach (var test in tests)
        {
            if (ct.IsCancellationRequested) break;
            var result = await RunOneAsync(test);
            Results.Add(result);
            TestCompleted?.Invoke(result);
            // Yield to the UI thread between tests so the browser stays responsive.
            await Task.Yield();
        }
    }

    /// <summary>
    /// Runs all tests in the specified category sequentially.
    /// </summary>
    public async Task RunCategoryAsync(string category, IReadOnlyList<WasmTestCase> tests, CancellationToken ct = default)
    {
        var subset = tests.Where(t => t.Category == category).ToList();
        await RunAllAsync(subset, ct);
    }

    private async Task<WasmTestResult> RunOneAsync(WasmTestCase test)
    {
        if (test.RequiresThreads && !WasmThreadsEnabled)
        {
            return new WasmTestResult(
                test.Name, test.Category,
                TestStatus.Skipped,
                "Requires Phase 2 (WasmEnableThreads=true). Skipped in single-threaded mode.",
                TimeSpan.Zero);
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await test.TestBody();
            sw.Stop();
            return new WasmTestResult(test.Name, test.Category, TestStatus.Passed, "OK", sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new WasmTestResult(test.Name, test.Category, TestStatus.Failed, ex.ToString(), sw.Elapsed);
        }
    }
}
