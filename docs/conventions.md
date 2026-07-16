# No Cryptic Jargon Convention

**Rule:** No commit title, code comment, issue title, issue body, or documentation page in this
project may use an unexplained initialism, abbreviation, or short code. This applies everywhere,
including short internal IDs like `BP-VAL-002` or shorthand references like `guide/03`.

## Requirements

1. **First use, full expansion.** The first time an initialism/code appears in a document, issue,
   or commit message, spell it out in full, with the short form in parentheses.
   Example: "Best Practice validation rule 002 (BP-VAL-002)" rather than just "BP-VAL-002".
2. **Titles are not exempt.** Issue titles and commit titles must also avoid unexplained jargon —
   expand initialisms inline in the title itself, not only in the body.
3. **Glossary is mandatory, not optional.** Every initialism/code used anywhere in the project
   (code, comments, commits, issues, docs) must have an entry in [glossary.md](glossary.md).
   If you introduce a new one, add the glossary entry in the same change.
4. **Numbered/coded shorthand must be spelled out.** References like `guide/03` or `TS-6` must
   name the actual thing they refer to (e.g., the file name or the human-readable topic), not
   just a number.
5. **Enforcement.** The docs-sync skill (see [skills/](skills/)) checks new commits, issues, and
   docs for unexplained initialisms/codes and flags anything missing from the glossary.

## Rationale

Cryptic internal jargon (rule IDs, ticket numbers, ad hoc abbreviations) is fine as a *stable
identifier* for cross-referencing, but it must never be the only label attached to something.
Anyone reading a commit title, an issue, or a doc page for the first time should be able to
understand what it's about without having to go spelunking through the codebase.

---

# Multi-PR Work Stage Naming Convention

**Rule:** When a piece of work is planned as a sequence of staged GitHub pull requests, refer to
each stage as **"Stage 1", "Stage 2", ...** — never "PR1", "PR2", etc. GitHub assigns its own
pull request/issue numbers automatically (a single global counter across the whole repository,
shared with issues), so a plan's "Stage N" almost never lines up with the actual GitHub `#number`
that stage ends up as. Calling a stage "PR2" when it might become GitHub PR #33 (or get folded
into another PR, or split across two) creates confusion between the plan's internal sequencing
and GitHub's real numbering.

- Always write **"Stage 1"**, **"Stage 2"**, etc. when discussing the plan itself.
- Always write **"PR #<actual number>"** (with the `#`) when referring to a real, opened GitHub
  pull request — never abbreviate or predict its number in advance.
- If a stage's work later gets folded into a different PR than originally planned (as happened
  during this docs-restructuring project, where Stage 3 landed inside PR #34 instead of its own
  PR), say so explicitly rather than letting the stage label imply a 1:1 mapping.
