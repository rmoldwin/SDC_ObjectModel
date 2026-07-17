namespace SDC.ScriptEngine.BlazorAsyncTests.TestEngine;

public enum TestStatus { NotRun, Running, Passed, Failed, Skipped }

/// <summary>
/// Describes a single test case. <see cref="RequiresThreads"/> = true marks Phase 2-only tests
/// that are automatically skipped when <see cref="WasmTestRunner.WasmThreadsEnabled"/> is false.
/// </summary>
public record WasmTestCase(
    string Name,
    string Category,
    Func<Task> TestBody,
    bool RequiresThreads = false  // true = Phase 2 only, auto-skipped in Phase 1
);

/// <summary>
/// Immutable result record for a completed test.
/// </summary>
public record WasmTestResult(
    string Name,
    string Category,
    TestStatus Status,
    string Message,
    TimeSpan Duration
);
