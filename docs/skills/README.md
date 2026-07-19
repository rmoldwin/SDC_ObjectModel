# AI Skills for SDC.Schema

This folder holds AI skills scoped specifically to working on the SDC.Schema solution. Unlike
the wiki (which may also host published skills for broader consumption), skills stored here are
the working/source copies used during active development.

## Planned skills

- **docs-sync** — verifies that `docs/`, `docs/skills/`, plan documents, the project wiki, and
  archived docs (e.g. `SDC.Schema.Tests/Documentation/Archived Plans/`) remain mutually
  consistent. Responsibilities include:
  - Every [roadmap.md](../roadmap.md) item has a linked GitHub issue, and vice versa.
  - Every initialism/abbreviation/short code (e.g. `BP-VAL-002`, `TS-6`) used in code, commits,
    issues, or docs has a corresponding entry in [glossary.md](../glossary.md).
  - No cryptic jargon appears unexplained in commit titles, issue titles, or doc pages, per
    [conventions.md](../conventions.md).
  - Archived-docs `README.md` files list matches the actual archived file set.
  - Wiki cross-links referenced from `docs/` still resolve.

This skill is tracked as a roadmap item and will be implemented in a later, separate PR.
