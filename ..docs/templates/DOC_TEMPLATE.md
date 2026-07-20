# Documentation Template

This template codifies the structure already used successfully across `..docs/` (see
[`architecture.md`](../architecture.md), [`roadmap.md`](../roadmap.md),
[`conventions.md`](../conventions.md), and the `architecture/*.md` chapters). Use it for every
new `.md` file added under `..docs/`, and apply it opportunistically when editing existing docs
that don't yet follow it. This is the canonical reference for the **docs-sync** responsibilities
described in [`skills/README.md`](../skills/README.md) and tracked in
[issue #38](https://github.com/rmoldwin/SDC_ObjectModel/issues/38).

## Required sections (in order)

1. **Title (`# H1`)** — one per file, human-readable, no unexplained jargon (see
   [`conventions.md`](../conventions.md), "No Cryptic Jargon Convention").
2. **Scope/purpose paragraph** — 1–3 sentences immediately under the title stating what the
   document covers and, if relevant, what it explicitly does *not* cover. Link sibling/parent
   index docs here (e.g. a chapter links back to [`architecture.md`](../architecture.md)).
3. **Body sections (`## H2`)** — organized topically, each with a clear, jargon-free heading.
   Prefer tables for any catalog/status/inventory content (rule IDs, roadmap items, glossary
   entries) over prose lists — tables are easier for both humans and AI tools to scan and diff.
4. **Cross-references** — use relative Markdown links (`[text](relative/path.md)`), never bare
   file names or absolute paths. Link to the specific anchor (`#heading`) when referencing a
   subsection. Every initialism/abbreviation on first use must be spelled out and, if reused
   project-wide, added to [`glossary.md`](../glossary.md).
5. **Status/provenance footer (when applicable)** — for living documents (roadmap, catalogs),
   end with a short note on how currency is maintained (e.g. "verified against the live issue
   tracker on `<date>`") or a pointer to the enforcing skill/process.

## Formatting conventions

- Use `**Status**`-style bold labels or a `| Status | ... |` table column for anything tracking
  progress (`Done`, `Planned`, `Deferred`, `Low priority`, `Future`).
- Every roadmap/plan item that maps to a GitHub issue must link it: `[#N](https://github.com/<owner>/<repo>/issues/N)`.
- Every table row about code (rule IDs, class/method names) must reference the actual file/type
  name — never a numeric-only shorthand (see the Multi-PR "Stage N" convention in
  [`conventions.md`](../conventions.md) for the parallel rule about PR/stage numbering).
- Keep line-oriented prose (not hard-wrapped) — this repo's existing docs use soft-wrapped
  paragraphs; match that style for diff-friendliness.

## Folder placement rules

| Content type | Location |
|---|---|
| Settled architecture/design chapters | `..docs/architecture/*.md`, indexed in [`architecture.md`](../architecture.md) |
| Dated point-in-time design/change history | `..docs/changes/YYYY-MM[-DD]_Topic.md` |
| Cross-cutting conventions/glossary | `..docs/conventions.md`, `..docs/glossary.md` |
| Planned/tracked work | `..docs/roadmap.md` (must link a GitHub issue) |
| Reusable AI skills/process docs | `..docs/skills/*.md` |
| Reusable templates (this file, issue template) | `..docs/templates/*.md` |
| Session continuity/handoff notes | `sessions/` (outside `..docs/`, see [`sessions/README.md`](../../sessions/README.md)) |
| Superseded/completed planning docs | project-local `Archived Plans/` subfolder with a provenance `README.md`, per the Document Management rule in `.github/copilot-instructions.md` |

## Checklist before committing a new/edited doc

- [ ] Title + scope paragraph present.
- [ ] No unexplained initialism/abbreviation (first-use expansion or glossary entry added).
- [ ] Tables used for catalog/status content.
- [ ] All relative links resolve (no dangling links after any file move/rename).
- [ ] Linked GitHub issue exists for any "Planned"/"Deferred" item, and the issue itself references
  this doc back if it's the primary design reference.
- [ ] If this doc supersedes another, the old doc was moved to an `Archived Plans/` folder (not
  deleted) with an updated provenance `README.md`.
