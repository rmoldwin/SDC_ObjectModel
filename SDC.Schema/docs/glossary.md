# Glossary

This glossary defines every initialism, abbreviation, and short code used across the SDC.Schema
solution's documentation, code comments, commit titles, and GitHub issues. Per the project's
[No Cryptic Jargon convention](conventions.md), any new initialism or short code introduced
anywhere in this project must be added here before (or at the same time as) its first use.

| Term | Expansion | Meaning |
|---|---|---|
| API | Application Programming Interface | A defined way for one piece of software to talk to another. |
| ASP.NET | Active Server Pages .NET | Microsoft's web application framework. |
| BP-* (e.g. BP-VAL-002) | Best Practice rule ID | Stable ID for a QA rule in `SDC.Schema.QA`, formatted `BP-<category>-<number>`. See [QA Rule ID Catalog](architecture/qa-best-practices.md) for the full list and category codes (VAL = Validation, SER = Serialization, MUT = Mutation, ADH = Ad-hoc attributes, GEN = General). |
| BSON | Binary JSON | A binary-encoded version of JSON used for compact/fast data storage. |
| CAP | College of American Pathologists | The organization whose cancer-reporting protocols/templates many SDC forms implement; some QA rules are flagged as CAP-specific rather than generic SDC best practices. |
| CCYY | Century-Century-Year-Year | 4-digit year format (e.g. 2026) used in date/time string patterns. |
| CI | Continuous Integration | Automated building/testing of code on every change. |
| CLR | Common Language Runtime | The .NET execution engine that runs compiled code. |
| DE | Data Element | An SDC schema building block representing one piece of data. |
| FDF | Form Design File | The SDC file format that defines a form's structure and rules. |
| GC | Garbage Collector | The .NET subsystem that automatically frees unused memory. |
| GUID | Globally Unique Identifier | A 128-bit value used to uniquely identify objects. |
| ID | Identifier | A value used to name/reference a specific object. |
| IEEE | Institute of Electrical and Electronics Engineers | Standards body (referenced for floating-point/number formats). |
| IET | IdentifiedExtensionType | An SDC OM node type that carries a unique ID (e.g. `BP-MUT-001` requires every IdentifiedExtensionType node's ID to be unique within its tree). |
| IRI | Internationalized Resource Identifier | Like a URI/URL but supports non-ASCII characters. |
| ISO 8601 | International Organization for Standardization, standard 8601 | The international standard for representing dates and times as text. |
| JSON | JavaScript Object Notation | A lightweight text format for representing structured data. |
| LINQ | Language Integrated Query | A .NET feature for querying collections with SQL-like syntax. |
| MsgPack | MessagePack | A compact binary serialization format, alternative to JSON/BSON. |
| .NET | .NET | Microsoft's software development platform/runtime. |
| OM | Object Model | The in-memory class hierarchy (SDC.Schema) representing an SDC form. |
| PLINQ | Parallel LINQ | A version of LINQ that runs queries across multiple CPU cores. |
| QA | Quality Assurance | Here: the SDC.Schema.QA validation/rules-checking subsystem. |
| RC | Release Candidate | A near-final build considered ready for release pending final testing. |
| README | "Read Me" | A document at the top of a folder explaining its contents/purpose. |
| RFC | Request for Comments | A formal technical standards document (e.g., for date/time formats). |
| RWLS | Reader-Writer Lock Slim | A .NET lock type allowing many readers or one writer at a time. |
| SDAC | SDC Structured Data Assessment/Capture rules | Built-in QA rule category for structured data capture behavior. |
| SDC | Structured Data Capture | The health-data-form standard this object model implements. |
| SDK | Software Development Kit | A set of tools/libraries for building on a platform. |
| SDS | SDC Structured Data Specification rules | Another built-in SDC QA rule category (schema-level rules). |
| SWMR | Single-Writer, Multiple-Reader | A concurrency pattern allowing one writer or many concurrent readers. |
| TFM | Target Framework Moniker | The string identifying which .NET version a project targets (e.g. `net10.0`). |
| TRG | (SDC) Technical Reference Guide | The official SDC standard's technical documentation. |
| TS-# (e.g. TS-6) | Thread-Safety test number | Internal numbering scheme for thread-safety investigation test cases; not a public/standard term. |
| UI | User Interface | The visual/interactive part of an application. |
| URI | Uniform Resource Identifier | A string that identifies a resource (e.g., a web address). |
| URN | Uniform Resource Name | A URI that names a resource without specifying its location. |
| UTC | Coordinated Universal Time | The primary world time standard, unaffected by time zones. |
| UTF-8 | Unicode Transformation Format – 8-bit | A common text encoding that supports all Unicode characters. |
| UX | User Experience | How easy/pleasant a tool is for a person to use. |
| WASM | WebAssembly | A binary format that lets code (e.g., C#/Blazor) run in web browsers. |
| XML | Extensible Markup Language | A tagged text format for structured data, used by SDC forms. |
| XSD | XML Schema Definition | The rulebook format that defines what valid XML must look like. |

## Numbered example convention

Generated example files (see `SDC.Schema.QA.ExampleGenerator/Program.cs`) are numbered rather
than named descriptively in some comments, e.g. `guide/03` refers to the 3rd generated example,
`03-adhoc-attributes-mixed-namespaces.xml`. Any such shorthand reference must be spelled out
in full (file name, not just the number) wherever it appears — see
[conventions.md](conventions.md).
