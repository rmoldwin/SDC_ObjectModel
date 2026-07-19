# SDC.Schema.Tests

## What the project is

`SDC.Schema.Tests` is the main Microsoft Test (MSTest) test project for the Structured Data Capture (SDC) object model library in `SDC.Schema`. It exercises serialization, validation, tree construction, mutation, ad-hoc attribute handling, and thread-safety behavior by building real object trees, loading sample Extensible Markup Language (XML) fixtures, and asserting on the public library behavior.

## Basic architecture

- `Functional/` contains broader end-to-end tests, including serialization, tree operations, tree stability, and validation flows.
- `OM/` contains object-model-focused tests, including node behavior, setter validation, numeric and date boundary checks, and thread-safety investigations.
- `TopNode/` contains tests for top-level SDC document types such as `FormDesignType`, `MappingType`, `PackageListType`, and `XMLPackageType`.
- `UtilityClasses/` contains tests for helper and extension APIs, attribute metadata, and ad-hoc attribute scenarios.
- `Validation/` contains validation-specific tests around coherence rules, event/report behavior, and validation extension methods.
- `Helpers/` plus `Setup.cs` provide shared test helpers and fixture-loading utilities.
- `Documentation/` is supporting project documentation, including active planning notes plus an `Archived Plans/` subfolder for superseded write-ups; it is not production source code.
- `Test Files/` is a fixture library of sample XML instances and related support files used by the tests; it is supporting content, not source code.
- `TestArtifacts/` holds generated or captured test outputs used during investigation and comparison work.
- The project file `SDC.Schema.Tests.csproj` references `..\SDC.Schema\SDC.Schema.csproj`. This project tests `SDC.Schema`; the other solution projects do not take a build-time dependency on this test project.

## State of completion

- Rough scale: this folder currently contains 51 C# source files and at least 457 `[TestMethod]` tests, with coverage spread across serialization, validation, ad-hoc attributes, tree mutation, and thread-safety scenarios.
- TODO and FIXME notes worth calling out:
  - `Functional\TreeOperations\MoveTests.cs` has a TODO comment about debug-output behavior in browser-hosted test runs.
  - `UtilityClasses\AttrMetadata\CompareTreesTests.cs` has TODO comments about rolled-up changed-attribute reporting and additional changed-attribute coverage.
  - `Documentation\Archived Plans\` also contains historical TODO text, but those notes are archived background material rather than active source code gaps.
- Open roadmap issues clearly relevant to work exercised here:
  - [#35](https://github.com/rmoldwin/SDC_ObjectModel/issues/35) — MessagePack round-trip support for polymorphic SDC object model types.
  - [#7](https://github.com/rmoldwin/SDC_ObjectModel/issues/7) and [#8](https://github.com/rmoldwin/SDC_ObjectModel/issues/8) — numeric validation/round-trip divergences and full soft-reject validation coverage.
  - [#17](https://github.com/rmoldwin/SDC_ObjectModel/issues/17), [#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20), [#21](https://github.com/rmoldwin/SDC_ObjectModel/issues/21), [#23](https://github.com/rmoldwin/SDC_ObjectModel/issues/23), and [#24](https://github.com/rmoldwin/SDC_ObjectModel/issues/24) — ongoing thread-safety and WebAssembly investigations that map directly to the thread-safety and comparison tests in this project.
