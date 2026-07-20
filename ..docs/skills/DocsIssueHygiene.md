# DocsIssueHygiene Skill (docs-sync)

Implements the "docs-sync" skill described below and tracked as
[issue #38](https://github.com/rmoldwin/SDC_ObjectModel/issues/38). This is the canonical,
repo-specific procedure an AI assistant should run (a) selectively before opening any PR, and
(b) comprehensively every 5 merged PRs. See
[`.github/copilot-instructions.md`](../../.github/copilot-instructions.md) for the always-loaded
pointer that triggers this skill, and
[`../../.github/skills/DocIssueHygiene_Portable.md`](../../.github/skills/DocIssueHygiene_Portable.md)
for the repo-agnostic version of this procedure suitable for reuse in other repositories.

## When to run

| Trigger | Mode | What to check |
|---|---|---|
| Before opening a PR | **Selective** | Only the docs/issues directly touched or made stale by this PR's diff. |
| Every 5th merged PR (see `.github/workflows/docs-issue-hygiene-reminder.yml`) | **Comprehensive** | Everything below, across the whole repo. |
| On explicit user request ("refresh docs", "check issues", etc.) | Either, per the request | As scoped by the user. |

## Selective (before-PR) checklist

1. For every `..docs/*.md` file touched by this PR's diff, confirm it still matches
   [`..docs/templates/DOC_TEMPLATE.md`](../templates/DOC_TEMPLATE.md) (title, scope paragraph,
   tables for catalog content, working relative links).
2. If the PR changes behavior described in an existing `..docs/architecture/*.md` chapter, update
   that chapter in the same PR (do not let docs drift from code in the same change).
3. If the PR closes, partially addresses, or contradicts an open GitHub issue, update that issue
   (status/comment) and its `..docs/roadmap.md` row.
4. If the PR introduces a new initialism/abbreviation, add it to
   [`..docs/glossary.md`](../glossary.md) in the same PR.
5. If the PR moves/renames/deletes any `.md` file, grep the whole repo for old-path references
   (see the one-time repo-wide fix in this PR for the exact technique: search for the literal old
   path/name across `*.md`, excluding `bin/obj/node_modules`, and fix every hit) and fix them in
   the same PR.
6. If the PR archives a plan/design doc as superseded, follow the Document Management rule in
   `.github/copilot-instructions.md` (move via `git mv` into an `Archived Plans/` folder, update
   its provenance `README.md`).

## Comprehensive (every-5-PR) checklist

Run all of the Selective checks above repo-wide, plus:

1. **Roadmap ↔ issue sync** — every [`..docs/roadmap.md`](../roadmap.md) row links a real, still-open
   GitHub issue (or is marked `Done`); every open GitHub issue that represents planned/tracked
   work appears somewhere in the roadmap. Use `gh issue list --state open --json number,title,labels`
   to enumerate, and cross-reference against the roadmap tables.
2. **Glossary completeness** — spot-check recent commit titles, issue titles/bodies, and doc pages
   (e.g. `git log --oneline -50`, the last 10 opened/updated issues) for any initialism/short code
   without a [`glossary.md`](../glossary.md) entry.
3. **Archive provenance** — every `Archived Plans/README.md` (or equivalent) file lists exactly the
   files currently present in that folder, with a one-line reason and pointer to the superseding
   doc.
4. **Dangling cross-references** — grep all `.md` files for relative links and spot-check that
   linked files exist (a full automated link-checker is out of scope for a manual pass; prioritize
   any file touched in the last 5 PRs).
5. **Issue formatting drift** — for issues opened/updated since the last comprehensive pass, check
   against [`..docs/templates/ISSUE_TEMPLATE.md`](../templates/ISSUE_TEMPLATE.md) (sections,
   labels, cross-links) and fix drift.
6. **Execution plan currency** — update
   [`..docs/templates/ISSUES_EXECUTION_PLAN.md`](../templates/ISSUES_EXECUTION_PLAN.md): remove
   closed issues, add newly opened ones into the right dependency tier, re-verify the "Depends on"
   column still holds.
7. **Solution/project file consistency** (tracked in
   [issue #57](https://github.com/rmoldwin/SDC_ObjectModel/issues/57)) — `SDC.Schema.sln` and
   `.csproj` files can silently drift from the actual git-tracked file/folder structure (observed
   directly during the PR #54 docs restructure: a stale solution-folder display name, an entirely
   missing solution folder for a new `..docs` subfolder, and two tracked `.csproj` files with no
   `.sln` entry). Each comprehensive pass, check:
   - Every solution folder in `SDC.Schema.sln` that mirrors an on-disk folder (currently `..docs` and
     its `architecture`, `changes`, `skills`, `templates` subfolders) has SolutionItems entries
     matching `git ls-files` for that folder — add missing files, drop stale ones, fix renamed paths.
   - `git ls-files "*.csproj"` matches every `Project(...) = "...", "...csproj", "{GUID}"` line in
     `SDC.Schema.sln` — flag (don't silently add) any tracked `.csproj` with no `.sln` entry; confirm
     with the repo owner whether it's abandoned before changing anything.
   - Every project GUID has a complete, non-duplicated set of
     `GlobalSection(ProjectConfigurationPlatforms)` entries, and no GUID appears twice.
8. Record the pass in this file's **Run log** below (date, PR-count trigger or manual, summary of
   fixes made) so future passes know what was already covered.

## Run log

| Date | Trigger | Summary |
|---|---|---|
| 2026-07-20 | Manual (this session) | Renamed `docs/` → `..docs/` repo-wide and fixed all resulting links; created `..docs/templates/DOC_TEMPLATE.md` and `ISSUE_TEMPLATE.md`; added missing labels to issues #13,#17,#23,#24,#25,#29,#35,#36,#37,#38,#52; added #49,#50,#52 to `roadmap.md`; created `ISSUES_EXECUTION_PLAN.md` covering all 28 open issues; created this skill and the portable version under `.github/skills/`. |
| 2026-07-20 | Manual (PR #54 review follow-up) | Addressed 5 Copilot bot review comments on PR #54: renamed `docs-issue-refresh` → `docs-hygiene` in `ISSUE_TEMPLATE.md` for naming consistency; converted 3 backslash code-span doc references to real relative Markdown links in `SDC.Schema.QA/README.md` and `SDC.Schema.QA.ExampleGenerator/README.md`; renamed the `SDC.Schema.sln` solution folder node from `"docs"` to `"..docs"`. Replied to all 5 review threads confirming fixes. |
| 2026-07-20 | Manual (solution-file audit) | Audited `SDC.Schema.sln` for drift against git-tracked files: added the missing `templates` solution folder (3 files) and the 2 missing `skills` folder entries; verified all 15 real projects' `.csproj` paths, build configs, and GUIDs are consistent with no duplicates. Opened #55 (verify docs-hygiene workflow after a real merge), #56 (install portable skill as a user-level Copilot instruction), and #57 (recurring solution/project-file consistency check, folded into this checklist as step 7). |
