# Issue Content Template

This template standardizes GitHub issue bodies for the SDC_ObjectModel repository so that both AI
assistants and humans can scan, prioritize, and execute issues with minimal extra context-gathering.
It formalizes the structure already used successfully by issues like #7, #8, and #47. Apply it to
every new issue; apply it retroactively to existing issues when they are next touched (see the
recurring `docs-hygiene` process in [`skills/DocsIssueHygiene.md`](../skills/DocsIssueHygiene.md)).

## Title

- Plain language, no unexplained jargon (spell out initialisms on first use, e.g.
  "MessagePack (MsgPack)"). Prefix with a bracketed area tag only when it aids triage and the area
  already has a convention (e.g. `[Thread-Safety]`, `[Architecture]`, `[Security]`).
- State the problem or the deliverable, not just a component name (bad: "SdcUtil issues"; good:
  "SdcUtil binding cache not thread-safe under concurrent first access").

## Body sections (in order; omit a section only if genuinely not applicable)

1. **Background** (bugs) or **Goal** (features/enhancements) — 1 short paragraph of context: what
   part of the SDC (Structured Data Capture) Object Model (OM) this touches and why it matters.
   Spell out any initialism on first use.
2. **Problem / Current behavior** (bugs only) — precise description of the defect, ideally with
   the failing scenario, error message, or divergence table.
3. **Required behavior / Goal** — the target end state in concrete, testable terms.
4. **Scope / Blast radius** — files, classes, generated-code touchpoints, and existing tests
   affected. Call out anything that must survive `xsd2code++` regeneration (see the Auto-Generated
   File Policy in `.github/copilot-instructions.md`).
5. **Plan** — a short numbered list of implementation steps, written so an AI agent could execute
   it with minimal further clarification.
6. **Relationship to other issues** — link related/overlapping/blocking issues by number
   (`#N`) both ways; update the other issue too if it doesn't already link back.
7. **Done criteria / Acceptance criteria** — a bullet checklist of objectively verifiable
   conditions (tests added and passing, no regressions, docs updated). This is what "closes" the
   issue — an AI executing it autonomously should stop and self-check against this list before
   opening a PR.

## Labels

Every issue must carry at least one **type** label (`bug`, `enhancement`, `tech-debt`,
`documentation`) and, when applicable, one or more **area** labels (`thread-safety`, `wasm`,
`area:wasm`, `area:concurrency`, `validation`, `architecture`, `security`) and a **priority**
label when it is not default priority (`low-priority`). Do not leave an issue unlabeled.

## Cross-linking requirement

Every issue that represents planned/tracked work must be linked from
[`..docs/roadmap.md`](../roadmap.md) (and vice versa — every roadmap "Planned"/"Deferred" row must
link a real open issue). This keeps the roadmap and the issue tracker as a single source of truth,
per the docs-sync responsibilities in [`skills/README.md`](../skills/README.md).

## Quick checklist for reformatting an existing issue

- [ ] Title is jargon-free and states the actual problem/deliverable.
- [ ] Body follows the section order above (add missing sections; don't reorder existing good
  content unnecessarily — minimize diff noise).
- [ ] At least one type label + relevant area/priority labels applied.
- [ ] Cross-links to related issues and to `..docs/roadmap.md` are present and correct.
- [ ] Acceptance criteria are objectively checkable (not vague).
