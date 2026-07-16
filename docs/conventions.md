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
