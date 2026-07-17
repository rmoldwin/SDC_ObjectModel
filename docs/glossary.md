# Glossary

This glossary defines every initialism, abbreviation, and short code used across the SDC.Schema
solution's documentation, code comments, commit titles, and GitHub issues. Per the project's
[No Cryptic Jargon convention](conventions.md), any new initialism or short code introduced
anywhere in this project must be added here before (or at the same time as) its first use.

| Term | Expansion | Meaning |
|---|---|---|
| API | Application Programming Interface | A defined way for one piece of software to talk to another. |
| AOT | Ahead-Of-Time (compilation) | Compiling code to native machine code before it runs, instead of interpreting/JIT-compiling it at run time. The WASM/Blazor projects in this repo currently run with AOT **disabled** (`RunAOTCompilation=false`) and use the slower interpreter instead — see [WASM/Blazor](architecture/wasm-blazor.md). |
| ASP.NET | Active Server Pages .NET | Microsoft's web application framework. |
| BP-* (e.g. BP-VAL-002) | Best Practice rule ID | Stable ID for a QA rule in `SDC.Schema.QA`, formatted `BP-<category>-<number>`. See [QA Rule ID Catalog](architecture/qa-best-practices.md) for the full list and category codes (VAL = Validation, SER = Serialization, MUT = Mutation, ADH = Ad-hoc attributes, GEN = General). |
| BSON | Binary JSON | A binary-encoded version of JSON used for compact/fast data storage. |
| CAP | College of American Pathologists | The organization whose cancer-reporting protocols/templates many SDC forms implement; some QA rules are flagged as CAP-specific rather than generic SDC best practices. |
| Cat#/Test# (e.g. Cat5/Test1, also written Cat5-Test1) | Category number / Test number | Internal shorthand for a specific test page/case in the Phase2 in-browser WASM test dashboard (`SDC.ScriptEngine.BlazorAsyncTests.Phase2`), e.g. "Category 5: Shared TopNode Tests, Test 1." Used in commit messages and bug tables; not a public/standard term. See [WASM/Blazor](architecture/wasm-blazor.md). |
| CCYY | Century-Century-Year-Year | 4-digit year format (e.g. 2026) used in date/time string patterns. |
| CI | Continuous Integration | Automated building/testing of code on every change. |
| CLR | Common Language Runtime | The .NET execution engine that runs compiled code. |
| COEP | Cross-Origin-Embedder-Policy | An HTTP response header that, together with COOP, "cross-origin isolates" a web page. Required (`require-corp`) for the browser to allow `SharedArrayBuffer`, which multi-threaded WebAssembly needs. See [MDN: Cross-Origin-Embedder-Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cross-Origin-Embedder-Policy) and [WASM/Blazor](architecture/wasm-blazor.md). |
| COOP | Cross-Origin-Opener-Policy | An HTTP response header that isolates a browser window from other origins (`same-origin`). Required, together with COEP, for `SharedArrayBuffer` and therefore for multi-threaded WebAssembly. See [MDN: Cross-Origin-Opener-Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cross-Origin-Opener-Policy) and [WASM/Blazor](architecture/wasm-blazor.md). |
| CSP | Content-Security-Policy | An HTTP header that restricts what scripts/resources a page may load or execute (e.g. blocking inline `eval`); referenced in the Phase2 WASM test harness, which avoids `eval`-based JavaScript interop specifically to prevent CSP warnings. |
| DE | Data Element | An SDC schema building block representing one piece of data. |
| DevServer | (Blazor WebAssembly Development Server) | The lightweight local web server the Blazor WebAssembly SDK spins up for `dotnet run`. It does not send the COOP/COEP headers multi-threaded WASM requires, so the Phase2 multi-threaded test harness must be run via `dotnet publish` + a real ASP.NET Core host instead. See [WASM/Blazor](architecture/wasm-blazor.md). |
| FDF | Form Design File | The SDC file format that defines a form's structure and rules. |
| FIXME | "Fix me" marker | A source-code comment tag used to mark a known problem that still needs correction. |
| GC | Garbage Collector | The .NET subsystem that automatically frees unused memory. |
| GUID | Globally Unique Identifier | A 128-bit value used to uniquely identify objects. |
| HTML | HyperText Markup Language | The standard markup language for web pages and self-contained browser-rendered reports. |
| ID | Identifier | A value used to name/reference a specific object. |
| IEEE | Institute of Electrical and Electronics Engineers | Standards body (referenced for floating-point/number formats). |
| IET | IdentifiedExtensionType | An SDC OM node type that carries a unique ID (e.g. `BP-MUT-001` requires every IdentifiedExtensionType node's ID to be unique within its tree). |
| IL | Intermediate Language | The .NET bytecode emitted by the compiler and loaded/executed by the runtime. |
| IRI | Internationalized Resource Identifier | Like a URI/URL but supports non-ASCII characters. |
| ISO 8601 | International Organization for Standardization, standard 8601 | The international standard for representing dates and times as text. |
| JSInterop | JavaScript Interop | The Blazor mechanism that lets C# code call JavaScript functions (and vice versa) in the browser. The Phase2 WASM test harness uses a named JavaScript helper function via JSInterop instead of `eval(...)`, specifically to avoid CSP (Content-Security-Policy) warnings. |
| JSON | JavaScript Object Notation | A lightweight text format for representing structured data. |
| LINQ | Language Integrated Query | A .NET feature for querying collections with SQL-like syntax. |
| MsgPack | MessagePack | A compact binary serialization format, alternative to JSON/BSON. |
| Mono | (runtime name, not an acronym) | The open-source .NET runtime implementation that Blazor WebAssembly uses to execute .NET IL (Intermediate Language) inside the browser's WebAssembly sandbox. Mono-specific limits (e.g. its worker/thread-pool size) show up directly in this repo's WASM configuration — see [WASM/Blazor](architecture/wasm-blazor.md). |
| MSTest | Microsoft Test (framework) | The unit-testing framework used across the SDC.Schema test projects. |
| .NET | .NET | Microsoft's software development platform/runtime. |
| OM | Object Model | The in-memory class hierarchy (SDC.Schema) representing an SDC form. |
| PLINQ | Parallel LINQ | A version of LINQ that runs queries across multiple CPU cores. |
| pthread | POSIX thread | The standard C threading API that native (non-browser) multi-threaded code normally uses. Mono's WebAssembly runtime emulates pthreads using browser Web Workers; several Phase2 WASM bugs and config limits (e.g. `WasmPThreadPoolSize`) trace directly back to this emulation layer — see [WASM/Blazor](architecture/wasm-blazor.md). |
| QA | Quality Assurance | Here: the SDC.Schema.QA validation/rules-checking subsystem. |
| RC | Release Candidate | A near-final build considered ready for release pending final testing. |
| README | "Read Me" | A document at the top of a folder explaining its contents/purpose. |
| RFC | Request for Comments | A formal technical standards document (e.g., for date/time formats). |
| RWLS | Reader-Writer Lock Slim | A .NET lock type allowing many readers or one writer at a time. |
| SDAC | SDC Structured Data Assessment/Capture rules | Built-in QA rule category for structured data capture behavior. |
| SDC | Structured Data Capture | The health-data-form standard this object model implements. |
| SDK | Software Development Kit | A set of tools/libraries for building on a platform. |
| SDS | SDC Structured Data Specification rules | Another built-in SDC QA rule category (schema-level rules). |
| SharedArrayBuffer | (JavaScript/browser API, not an acronym) | A JavaScript memory buffer that multiple Web Workers (browser threads) can read/write directly, forming the basis of real multi-threaded WebAssembly. Browsers only allow it on pages that are "cross-origin isolated" via the COOP and COEP headers. See [MDN: SharedArrayBuffer](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/SharedArrayBuffer) and [WASM/Blazor](architecture/wasm-blazor.md). |
| SWMR | Single-Writer, Multiple-Reader | A concurrency pattern allowing one writer or many concurrent readers. |
| TFM | Target Framework Moniker | The string identifying which .NET version a project targets (e.g. `net10.0`). |
| TODO | "To do" marker | A source-code comment tag used to mark missing follow-up work or an unfinished task. |
| TRG | (SDC) Technical Reference Guide | The official SDC standard's technical documentation. |
| TS-# (e.g. TS-6) | Thread-Safety test number | Internal numbering scheme for thread-safety investigation test cases; not a public/standard term. |
| UI | User Interface | The visual/interactive part of an application. |
| URI | Uniform Resource Identifier | A string that identifies a resource (e.g., a web address). |
| URN | Uniform Resource Name | A URI that names a resource without specifying its location. |
| UTC | Coordinated Universal Time | The primary world time standard, unaffected by time zones. |
| UTF-8 | Unicode Transformation Format – 8-bit | A common text encoding that supports all Unicode characters. |
| UX | User Experience | How easy/pleasant a tool is for a person to use. |
| WASM | WebAssembly | A binary format that lets code (e.g., C#/Blazor) run in web browsers. |
| wasm-tools | (.NET SDK workload name, not an acronym) | The optional .NET SDK "workload" (install unit) that provides the thread-enabled native WebAssembly runtime binaries needed for `WasmEnableThreads=true`. Installed via `dotnet workload install wasm-tools`; verify with `dotnet workload list`. See [WASM/Blazor](architecture/wasm-blazor.md). |
| Webcil | (portmanteau of "Web" + ".cil", not a standard acronym) | A wrapper file format .NET can use to package a compiled assembly for the browser so its file extension/content-type isn't `.dll` (some web servers/policies block serving raw `.dll` files). Controlled by the `WasmEnableWebcil` MSBuild property; the browser hosts in this repo currently disable it (`false`). |
| XML | Extensible Markup Language | A tagged text format for structured data, used by SDC forms. |
| XSD | XML Schema Definition | The rulebook format that defines what valid XML must look like. |

## Numbered example convention

Generated example files (see `SDC.Schema.QA.ExampleGenerator/Program.cs`) are numbered rather
than named descriptively in some comments, e.g. `guide/03` refers to the 3rd generated example,
`03-adhoc-attributes-mixed-namespaces.xml`. Any such shorthand reference must be spelled out
in full (file name, not just the number) wherever it appears — see
[conventions.md](conventions.md).
