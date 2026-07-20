# SDC.Schema Docs — Summary

This `..docs/` folder is the **work-in-progress** technical knowledge base for the SDC.Schema
solution (the Structured Data Capture, or SDC, Object Model, or OM). It captures architecture
decisions, ongoing design work, and a running roadmap while features are still in motion.

Once a topic becomes settled and stable, it should be promoted into the project **wiki**, which
is the durable, polished reference (including images and material drawn from the SDC Technical
Reference Guide, or TRG). See [architecture.md](architecture.md) for the current chapter
breakdown, and [roadmap.md](roadmap.md) for planned work and its linked GitHub issues.

## Folder layout

```
..docs/
  summary.md          — this file
  architecture.md      — index of architecture chapters
  roadmap.md           — planned work, each item linked to a GitHub issue
  glossary.md          — every initialism/abbreviation used in the project, spelled out
  conventions.md       — project-wide documentation/jargon conventions
  architecture/        — one file per architecture chapter (serialization, validation, etc.)
  changes/             — dated technical change logs (component-level design history)
  skills/              — AI skills specific to maintaining/using the SDC.Schema solution
  templates/           — DOC_TEMPLATE.md, ISSUE_TEMPLATE.md, and the issues execution plan
```

## Related locations

- **`sessions/`** (top level of the solution, alongside `SDC.Schema/`, `SDC.Schema.Tests/`, etc.)
  holds session continuity/handoff documents — not architecture content, so it lives outside
  `..docs/`.
- **`SDC.Schema.Tests/Documentation/Archived Plans/`** holds superseded planning documents,
  preserved for change history.
- The project **wiki** (not in this repo checkout) holds settled, polished technical reference
  material.

## Status

This structure was established to consolidate technical knowledge that had been scattered across
GitHub gists and `SDC.Schema.Tests/Documentation/`. Migration of that existing material is
tracked as separate follow-up work — see [roadmap.md](roadmap.md).
