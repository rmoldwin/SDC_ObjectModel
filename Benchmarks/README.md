# Benchmarks

## What this project is

`Benchmarks` is a BenchmarkDotNet console project for measuring selected `SDC.Schema` operations. It is not part of the scripting subsystem; it loads representative Structured Data Capture (SDC) data and times/refines hot paths in the core Structured Data Capture (SDC) Object Model (OM) code.

## Basic architecture

- `Benchmarks.csproj` targets `net10.0`, references `..\SDC.Schema\SDC.Schema.csproj`, and adds `BenchmarkDotNet`.
- `Program.cs` is both the benchmark entry point and the benchmark class definition. The active benchmark runner currently calls `BenchmarkRunner.Run<SdcTests>()`.
- The main benchmark methods currently focus on:
  - `ReflectNodes()` — calls `SdcUtil.ReflectRefreshTree(...)`,
  - `CompareVersions()` — loads two sample form versions and performs parallel attribute-comparison work.
- `Setup.cs` loads the sample XML data used by the benchmarks and exposes shared helper/state methods.
- `Test files\` contains the XML inputs consumed by `Setup.cs` and `Program.cs`.
- Relationship to other projects:
  - direct project reference: `SDC.Schema`,
  - no direct dependency on `SDC.ScriptEngine`.

## State of completion

- Rough scope: one benchmark runner file, one setup/helper file, and a folder of sample XML inputs.
- This project is clearly a working harness, but it is still somewhat exploratory: the benchmark code mixes active benchmarks, commented-out experiments, and diagnostic helper logic in one file.
- `Program.cs` contains several project-authored `TODO` notes worth knowing about:
  - investigate a thread-safe non-static previous-sibling lookup to avoid locking,
  - reduce some LINQ-based lookups in the comparison path,
  - consider cheaper direct value comparison instead of string conversion in attribute comparison,
  - decide whether removed-node entries should be recorded explicitly in `CompareVersions()`.
- No clearly matching open roadmap issue was identified for this folder alone.
