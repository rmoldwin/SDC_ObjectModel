# Roadmap

Every roadmap item below must have a linked GitHub issue, and every relevant open GitHub issue
(bug, fix, improvement, to-do) must appear here. Keeping this file and the issue tracker in sync
is one of the responsibilities of the docs-sync skill (see [skills/](skills/)).

| Status | Item | Issue |
|---|---|---|
| Done | Migrate the two public GitHub gists ("SDC OM Validation" and "SDC OM Rules") into `docs/architecture/validation.md` and `docs/architecture/rules.md`, then delete the gists | TBD |
| Done | Move `docs/` from inside the `SDC.Schema` project folder to the top level of the repository, alongside `sessions/` | TBD |
| Done | Consolidate `SDC.Schema.Tests/Documentation` technical docs (XML Schema Definition, or XSD, vs. .NET type divergence notes; OM tree-stability notes; thread-safety investigation notes; validation-pipeline unification notes; BSON/JSON serializer bug history) into `docs/architecture/`, archiving the 12 fully-superseded originals into `SDC.Schema.Tests/Documentation/Archived Plans/` | TBD |
| Done | Move session handoff/kickstart documents (13 files) out of `SDC.Schema.Tests/Documentation` into the top-level `sessions/` folder | TBD |
| Planned | Create a docs-sync AI skill that checks `docs/`, `docs/skills/`, plan documents, the wiki, and archived docs for consistency (including glossary completeness and roadmap/issue sync) | TBD |
| Planned | Begin populating the project wiki with settled architecture content and images/links from the SDC Technical Reference Guide (TRG) | TBD |
| Planned | Migrate a `Migrate timeZone from string to TimeSpan` improvement for `date`/`dateTime`/`time` `ResponseType`s (see [architecture/xsd-dotnet-type-mapping.md](architecture/xsd-dotnet-type-mapping.md)) | TBD |
| Planned | Fix MessagePack (MsgPack) round-trip support for polymorphic SDC Object Model (OM) types, or replace the MsgPack.Cli-based serializer (see [architecture/serialization.md](architecture/serialization.md)) | TBD |
| Planned | Finish XML documentation-comment (`<summary>`) coverage across all public SDC.Schema members (see `sessions/XmlAnnotationPlan.md`) | TBD |

This table will be updated with real issue numbers once PR4 (GitHub issue creation) runs.
