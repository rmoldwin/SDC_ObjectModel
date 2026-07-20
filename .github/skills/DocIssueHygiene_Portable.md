# Portable Skill: Docs & Issue Hygiene for AI Coding Agents

**Portability note:** this file is written to be repo-agnostic. It is the reusable source for the
SDC_ObjectModel-specific procedure in
[`..docs/skills/DocsIssueHygiene.md`](../../..docs/skills/DocsIssueHygiene.md). To use it in
another repository, copy this file there (or install it as a personal, user-level Copilot
instruction so it applies automatically across all repos you work in — a repo-local
`.github/copilot-instructions.md` only auto-loads for that one repo). Replace the bracketed
`[placeholders]` with the target repo's actual paths/labels.

## Purpose

Keep a repository's Markdown documentation and issue tracker internally consistent, cross-linked,
and non-stale — with minimal supervision — by running two kinds of passes:

1. A **selective** pass before every pull request (PR), scoped to what that PR touched.
2. A **comprehensive** pass every 5 merged PRs (or on explicit request), scoped to the whole repo.

## Preconditions to adapt per repo

- `[DOCS_DIR]` — the repo's primary docs folder (e.g. `..docs/`, `docs/`, `Documentation/`).
- `[GLOSSARY_FILE]` — file defining abbreviations/initialisms, if one exists.
- `[ROADMAP_FILE]` — file tracking planned work + issue links, if one exists.
- `[ARCHIVE_DIR_PATTERN]` — convention for archiving superseded planning docs, if one exists.
- `[ISSUE_LABELS]` — the repo's actual label taxonomy (type/area/priority).

## Selective (before-PR) checklist

1. For every doc file touched by this PR's diff, confirm it still has: a clear title, a short
   scope/purpose statement near the top, tables (not prose) for any catalog/status content, and
   working relative links.
2. If the PR changes behavior described in an existing architecture/design doc, update that doc in
   the same PR — do not let docs drift from code in the same change.
3. If the PR closes, partially addresses, or contradicts an open issue, update that issue
   (comment/status/labels) and its row in `[ROADMAP_FILE]`, if present.
4. If the PR introduces a new abbreviation/initialism the project spells out on first use, add it
   to `[GLOSSARY_FILE]`, if present, in the same PR.
5. If the PR moves/renames/deletes any doc file, search the whole repo (docs, READMEs, code
   comments) for the old path/name and fix every reference in the same PR. A reliable technique:
   grep all Markdown files for the literal old folder/file name (excluding build output
   directories), then fix each hit.
6. If the PR archives a superseded plan/design doc, move it (preserving version-control history,
   e.g. `git mv`) into the repo's archive convention and update that archive folder's index/README
   with why it was archived and what replaced it.

## Comprehensive (every-5-PR) checklist

Run all Selective checks repo-wide, plus:

1. **Roadmap/backlog ↔ issue tracker sync** — every roadmap/backlog row links a real, still-open
   issue (or is marked done); every open issue representing planned/tracked work appears in the
   roadmap/backlog.
2. **Glossary/jargon completeness** — spot-check recent commit titles, issue titles/bodies, and doc
   pages for unexplained abbreviations/short codes missing from the glossary.
3. **Archive provenance** — every archive-folder index lists exactly the files currently present,
   with a reason and pointer to the superseding doc.
4. **Dangling cross-references** — spot-check that relative links between docs still resolve,
   prioritizing files touched in the last 5 PRs.
5. **Issue formatting/labeling drift** — check recently opened/updated issues against the project's
   issue template and label taxonomy; fix drift (missing labels, missing sections, missing
   cross-links to related issues).
6. **Execution-plan currency** — if the repo maintains a prioritized/dependency-ordered execution
   plan across open issues, remove closed issues, add newly opened ones in the right tier, and
   re-verify dependency notes still hold.
7. Log the pass (date, trigger, summary of fixes) in the repo-specific doc that hosts this skill,
   so future passes know what was already covered.

## Recurring triggers (how to wire this up in a new repo)

- **Before-PR:** state this as a required step in the repo's own AI instructions file (e.g.
  `.github/copilot-instructions.md`) so it always loads automatically.
- **Every-5-PR:** add a lightweight CI workflow that counts merged PRs since the last comprehensive
  pass (e.g. a counter file or a GitHub label applied to the triggering issue) and opens/updates a
  tracking issue tagged for an AI agent to pick up — see this repo's
  `.github/workflows/docs-issue-hygiene-reminder.yml` for a concrete implementation.
