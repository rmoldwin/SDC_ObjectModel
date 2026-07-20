# AI Skills for SDC.Schema

This folder holds AI skills scoped specifically to working on the SDC.Schema solution. Unlike
the wiki (which may also host published skills for broader consumption), skills stored here are
the working/source copies used during active development.

## Implemented skills

- **docs-sync / DocsIssueHygiene** — see [`DocsIssueHygiene.md`](DocsIssueHygiene.md). Verifies
  that `..docs/`, `..docs/skills/`, plan documents, the project wiki, and archived docs (e.g.
  `SDC.Schema.Tests/Documentation/Archived Plans/`) remain mutually consistent, and that open
  GitHub issues stay formatted, labeled, and cross-linked with the roadmap. Runs in two modes:
  selectively before every PR, and comprehensively every 5 merged PRs (see
  `.github/workflows/docs-issue-hygiene-reminder.yml` and
  `.github/copilot-instructions.md`). Responsibilities include:
  - Every [roadmap.md](../roadmap.md) item has a linked GitHub issue, and vice versa.
  - Every initialism/abbreviation/short code (e.g. `BP-VAL-002`, `TS-6`) used in code, commits,
    issues, or docs has a corresponding entry in [glossary.md](../glossary.md).
  - No cryptic jargon appears unexplained in commit titles, issue titles, or doc pages, per
    [conventions.md](../conventions.md).
  - Archived-docs `README.md` files list matches the actual archived file set.
  - Wiki cross-links referenced from `..docs/` still resolve.
  - Open issues follow [`templates/ISSUE_TEMPLATE.md`](../templates/ISSUE_TEMPLATE.md) and stay
    reflected in [`templates/ISSUES_EXECUTION_PLAN.md`](../templates/ISSUES_EXECUTION_PLAN.md).

Tracked by [issue #38](https://github.com/rmoldwin/SDC_ObjectModel/issues/38) (left open until the
skill has been exercised across a real before-PR cycle with no false positives).
