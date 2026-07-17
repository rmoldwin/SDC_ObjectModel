# SDC.Schema Architecture

This is the index of architecture chapters for the SDC.Schema solution — the implementation of
the Structured Data Capture (SDC) Object Model (OM). Each chapter lives in `architecture/` as its
own file. See [glossary.md](glossary.md) for every initialism used below.

## Chapters

1. **Object Model Overview** — [architecture/object-model-overview.md](architecture/object-model-overview.md)
   Node hierarchy, `BaseType`, the split between xsd2code++-generated classes and customized
   classes (see the Auto-Generated File Policy in the project's contribution notes).
2. **Serialization** — [architecture/serialization.md](architecture/serialization.md)
   XML (`SdcSerializer<T>` / `XmlSerializer`, `XmlInclude`-based polymorphism), JSON
   (`SdcSerializerJson<T>` / Newtonsoft.Json, `TypeNameHandling.All`), BSON
   (`SdcSerializerBson<T>`, Base64-encoded bytes), MessagePack (`SdcSerializerMsgPack<T>`), and
   ad hoc/"any" attribute and namespace round-tripping across all formats.
3. **Validation** — [architecture/validation.md](architecture/validation.md)
   Built-in SDC / College of American Pathologists (CAP) structural validation, ad hoc and
   (de)serialization-time validation, and the QA engine / `SdcValidationReport` bridge.
4. **Rules Engine** — [architecture/rules.md](architecture/rules.md)
   Built-in SDC Structured Data Assessment/Capture (SDAC) and SDC Structured Data Specification
   (SDS) rules, ad hoc custom rules, and the roadmap for Form Design File (FDF)-embedded rules.
5. **QA Best Practices & Rule Catalog** — [architecture/qa-best-practices.md](architecture/qa-best-practices.md)
   Catalog of all Best Practice (`BP-*`) rule IDs from `SDC.Schema.QA`, their categories, and the
   numbered example set from `SDC.Schema.QA.ExampleGenerator`.
6. **Running in WASM / Blazor** — [architecture/wasm-blazor.md](architecture/wasm-blazor.md)
   WebAssembly (WASM) hosting considerations, `SDC.ScriptEngine` Blazor/WASM spikes, and
   thread-pool/concurrency caveats specific to WASM.
7. **Modeling Best Practices** — [architecture/modeling-best-practices.md](architecture/modeling-best-practices.md)
   Guidance for authoring SDC forms/Form Design Files (FDFs) against this object model.
8. **OM Tree Stability & Thread Safety** — [architecture/tree-stability.md](architecture/tree-stability.md),
   [architecture/thread-safety.md](architecture/thread-safety.md)
   Consolidated findings from the tree-stability and thread-safety investigation work
   (migrated from `SDC.Schema.Tests/Documentation`).
9. **Type Fidelity: XSD vs .NET** — [architecture/xsd-dotnet-type-mapping.md](architecture/xsd-dotnet-type-mapping.md)
   Consolidated divergence notes for `anyURI`, `dateTime`, and numeric range types between XML
   Schema Definition (XSD) and .NET (migrated from `SDC.Schema.Tests/Documentation`).
10. **CompareTrees** — [architecture/compare-trees.md](architecture/compare-trees.md)
    How `CompareTrees<T>` compares two SDC Object Model (OM) trees, how it matches
    `IdentifiedExtensionType` nodes and serialized Extensible Markup Language (XML) attributes, and
    what its diff records mean.
11. **SdcUtil** — [architecture/sdc-util.md](architecture/sdc-util.md)
    Inventory of the central `SdcUtil` helper class: validation plumbing, tree refresh/rebuild
    helpers, navigation/traversal helpers, Extensible Markup Language (XML) reflection, naming, and
    attachment support.

All chapters listed above link to existing content. Planned future chapters are tracked in [roadmap.md](roadmap.md).

## Docs vs. wiki

This `docs/` folder holds **work-in-progress** technical material. Once a topic is settled and
stable, its content should be copied into the project wiki with added explanation, images, and
cross-links, per [summary.md](summary.md). Some topics (e.g., broad conceptual overviews, SDC
Technical Reference Guide, or TRG, excerpts) may be wiki-only and never need a `docs/` chapter.
