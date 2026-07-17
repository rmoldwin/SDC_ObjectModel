# SDC.Schema

`SDC.Schema` is the core Structured Data Capture (SDC) Object Model (OM) class library in this
repository. It targets `.NET 10` (`net10.0`) and implements the in-memory node hierarchy used to
load, inspect, mutate, validate, compare, and re-serialize SDC documents. Much of the library is
generated from Extensible Markup Language (XML) Schema Definition (XSD) files with Xsd2Code++, then
layered with hand-written customizations, partial classes, extension methods, validation logic, and
serializer code.

This README is a project-level orientation guide for the library source folder. The repository-wide
technical knowledge base lives in [`../docs/`](../docs/), which is explicitly work in progress and
separate from the eventual project wiki; see [`../docs/summary.md`](../docs/summary.md).

## Folder layout

The top level of `SDC.Schema/` currently contains these project folders:

- `Enums/` — shared enumerations such as question and item classification helpers used across the
  object model.
- `Interfaces/` — core interfaces such as `ITopNode`, `IBaseType`, and compatibility interfaces
  used by builders and serializer helpers.
- `Notebooks/` — ad hoc exploratory notebook content currently kept with the project.
- `Partial Classes/` — consolidated partial-class extensions for generated types, including
  cross-cutting object-model behavior that should not live in the generated files themselves.
- `Properties/` — project resources and generated resource-designer output.
- `Schema Classes/` — the main schema-derived type hierarchy plus the hand-maintained overlays that
  make it usable in the real object model. Important nested convention folders here include:
  - `SDC Constructor Removed/` — generated variants where constructors and related generated code
    have been adjusted for the object model's runtime needs.
  - `SDC Unmodified Classes/` — baseline generated classes kept close to generator output.
  - `SDC Schema Files/` — the source XML Schema Definition files and related transform assets.
  - `SDC Customized Classes/` — hand-written or hand-corrected overlays such as `BaseType`,
    `IdentifiedExtensionType`, validation helpers, and the nested `SDC Serializers/` folder for the
    XML, JavaScript Object Notation (JSON), Binary JSON (BSON), and MessagePack (MsgPack)
    serializers.
- `Utility Classes/` — supporting infrastructure such as `SdcUtil`, validation/reporting helpers,
  coherence rules, compare/diff helpers, lock scopes, comparers, builders, and the extension-method
  implementations under `Utility Classes/Extensions/`.

The project file is `SDC.Schema.csproj`, which targets `.NET 10` and enables nullable reference
types and XML documentation file generation.

## Generated code plus custom layers

The library is substantially generated from SDC XML Schema Definition (XSD) files, but it is not a
pure generator dump. The working architecture depends on several added layers:

- partial classes,
- hand-customized schema classes,
- utility/extension helpers,
- validation rules and event/report plumbing,
- serializer wrappers for XML, JavaScript Object Notation (JSON), Binary JSON (BSON), and
  MessagePack (MsgPack),
- tree refresh, move, and compare infrastructure.

That split matters when reading or modifying the code: many higher-level behaviors live outside the
original generated class files on purpose.

## Architecture at a glance

The authoritative chapter index is [`../docs/architecture.md`](../docs/architecture.md). The most
important chapters for this project are:

### Serialization

[`../docs/architecture/serialization.md`](../docs/architecture/serialization.md) documents the four
supported serializer families: XML, JavaScript Object Notation (JSON), Binary JSON (BSON), and
MessagePack (MsgPack). The XML path uses `XmlSerializer`; the JSON and BSON paths use
Newtonsoft.Json with explicit polymorphism and non-public-constructor support; the MessagePack path
still has known fidelity gaps for deeply polymorphic Structured Data Capture (SDC) Object Model
(OM) trees.

### Validation

[`../docs/architecture/validation.md`](../docs/architecture/validation.md) explains the core
validation pipeline, including `DataAnnotations`, soft-reject behavior for invalid setter input,
deserialize-time validation, `SdcValidationReport`, and the bridge to the separate Quality
Assurance (QA) engine.

### Rules engine

[`../docs/architecture/rules.md`](../docs/architecture/rules.md) covers the built-in coherence-rule
extension point for Structured Data Assessment/Capture (SDAC) and Structured Data Specification
(SDS) checks, plus the registry mechanism for custom rule implementations supplied by consuming
applications.

### Tree stability and thread safety

[`../docs/architecture/tree-stability.md`](../docs/architecture/tree-stability.md) and
[`../docs/architecture/thread-safety.md`](../docs/architecture/thread-safety.md) capture the current
state of dictionary integrity, move/reparent correctness, and single-writer/multiple-reader locking.
The desktop thread-safety pass is implemented and documented; several remaining concurrency issues
for WebAssembly (WASM) multi-threaded use remain on the roadmap.

### Tree comparison

[`../docs/architecture/compare-trees.md`](../docs/architecture/compare-trees.md) documents
`CompareTrees<T>`, the helper that compares two versions of an SDC tree and reports added,
removed, moved, reparented, and attribute-changed nodes.

### `SdcUtil`

[`../docs/architecture/sdc-util.md`](../docs/architecture/sdc-util.md) describes the large static
utility class that underpins validation state, tree refresh/rebuild operations, navigation, XML
reflection, naming, and attachment helpers across the library.

## State of completion and open core-library work

The current backlog for this core library is summarized in [`../docs/roadmap.md`](../docs/roadmap.md).
For `SDC.Schema` itself, the open work items are:

- **Serialization** — migrate `timeZone` to a stronger typed offset representation (`#9`), fix or
  replace polymorphic MessagePack (MsgPack) round-tripping (`#35`), optionally add asynchronous
  file input/output overloads (`#26`), and make XML serialization friendlier to trimmed
  WebAssembly (WASM) deployments (`#18`).
- **Validation** — thread parse errors through the public datatype-builder entry points (`#6`),
  close the remaining numeric XML Schema Definition (XSD) versus `.NET` round-trip divergences
  (`#7`), and extend the soft-reject contract so no invalid value survives any setter or
  deserialization path (`#8`).
- **Thread safety / shared state** — move remaining ambient deserialization state from
  `[ThreadStatic]` to `AsyncLocal<T>` where needed (`#17`), replace the remaining non-thread-safe
  reflection caches in `SdcUtil` (`#23`), and finish the remaining WebAssembly (WASM)
  multi-threading defect work (`#20`, `#21`, `#24`, `#28`).
- **Rules engine** — decide whether the unused recursive predicate-expression model in the generated
  rules types should be removed or formally documented as unsupported (`#29`).

## Where to go next

- Start with [`../docs/architecture.md`](../docs/architecture.md) for deeper implementation detail.
- Use [`../docs/glossary.md`](../docs/glossary.md) when an SDC-specific term or short code is
  unfamiliar.
- Use [`../docs/roadmap.md`](../docs/roadmap.md) to understand what is still incomplete or under
  investigation.
