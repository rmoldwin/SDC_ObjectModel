# SDC.ScriptEngine.WpfTest

## What this project is

`SDC.ScriptEngine.WpfTest` is the desktop comparison host for the scripting work. It is a Windows desktop test harness that lets a developer compile scripts, run cached compiled bytes, exercise the pre-compiled-document path, and inspect canonical-hash behavior against live Structured Data Capture (SDC) Object Model (OM) nodes.

## Basic architecture

- `SDC.ScriptEngine.WpfTest.csproj` targets `net10.0-windows` and references both `..\SDC.ScriptEngine\SDC.ScriptEngine.csproj` and `..\SDC.Schema\SDC.Schema.csproj`.
- `MainWindow.xaml` defines the user interface for the script editor, hash inspector, result panel, and security notes.
- `MainWindow.xaml.cs` wires the buttons to `CompileAsync`, `RunAsync`, and `ExecutePrecompiledAsync`, creates fresh `QuestionItemType` test nodes, and displays diagnostics and before/after mutation results.
- `App.xaml` / `App.xaml.cs` provide the usual Windows desktop application startup shell.
- Relationship to other projects:
  - it is the easiest direct desktop host for `SDC.ScriptEngine`,
  - it provides the non-browser comparison point for the WebAssembly hosts documented in [../docs/architecture/wasm-blazor.md](../docs/architecture/wasm-blazor.md).

## State of completion

- Rough scope: one-window desktop harness plus standard application shell files.
- The project already exercises the three main paths the engine exposes to hosts: compile only, run cached compiled bytes, and run the pre-compiled-document path.
- No project-authored `TODO` or `FIXME` comments were found in the current source files.
- No Windows-desktop-specific open roadmap issue was clearly identified; this project mainly serves as the desktop comparison host for the browser-side work.
