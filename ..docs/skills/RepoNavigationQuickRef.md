# Repo Navigation & Context Quick Reference

Purpose: cut down repeated file-system exploration in future AI sessions. Read this first for
"where is X" and "how do I run Y" questions before globbing/grepping the repo. Keep this file
updated whenever the layout below changes materially (part of the docs-sync
[before-PR checklist](DocsIssueHygiene.md)).

## Solution layout

- `SDC.Schema.sln` — the solution file at repo root.
- `SDC.Schema/` — core Object Model (OM) class library + serializers (see
  `SDC Customized Classes/`, `Utility Classes/`, `Extensions/`, `Interfaces/`, `SDC Serializers/`,
  `Partial Classes/PartialClasses.cs` for where hand-written/customization code belongs — the
  Auto-Generated File Policy in `.github/copilot-instructions.md` governs everything else here).
- `SDC.Schema.Tests/` — MSTest unit/integration tests for `SDC.Schema`; `Documentation/` holds
  design notes (current) and `Documentation/Archived Plans/` holds superseded ones.
- `SDC.Schema.QA/` + `SDC.Schema.QA.Tests/` + `SDC.Schema.QA.ExampleGenerator/` — the Best
  Practice (`BP-*`) rule-based QA engine, its tests, and its numbered XML example generator (see
  `..docs/architecture/qa-best-practices.md`).
- `SDC.ScriptEngine*` projects — Blazor/WASM script-hosting spikes and tests (Blazor, WASM,
  WPF comparison point, async variants including a `Phase2` follow-up). See
  `..docs/architecture/wasm-blazor.md`.
- `Benchmarks/` — BenchmarkDotNet performance benchmarks.
- `..docs/` — work-in-progress technical knowledge base (see `..docs/summary.md`).
- `sessions/` — AI session continuity/handoff/kickstart documents (outside `..docs/` by design).
- `TestArtifacts/` — generated test output artifacts.

## Where to look for things

| Question | Look here |
|---|---|
| "How does serialization/validation/rules/tree-ops work?" | `..docs/architecture/*.md`, indexed by `..docs/architecture.md` |
| "What does this abbreviation/initialism mean?" | `..docs/glossary.md` |
| "What's still planned/open?" | `..docs/roadmap.md` and `..docs/templates/ISSUES_EXECUTION_PLAN.md` |
| "What doc/issue structure should I follow?" | `..docs/templates/DOC_TEMPLATE.md`, `..docs/templates/ISSUE_TEMPLATE.md` |
| "What happened in a past AI session?" | `sessions/README.md` (index) |
| "Why was this design doc archived / what replaced it?" | `SDC.Schema.Tests/Documentation/Archived Plans/README.md` |
| "What are the repo-wide behavioral rules for AI assistants?" | `.github/copilot-instructions.md` (auto-loaded every session) |

## Common commands

```powershell
# Build the whole solution
dotnet build SDC.Schema.sln

# Run all tests in a specific test project (fast, targeted — prefer this over full-suite runs)
dotnet test SDC.Schema.Tests/SDC.Schema.Tests.csproj --filter "FullyQualifiedName~<Namespace.ClassName>"

# Run the QA engine tests
dotnet test SDC.Schema.QA.Tests/SDC.Schema.QA.Tests.csproj

# List open issues with labels (for docs-sync / roadmap cross-checks)
gh issue list --state open --json number,title,labels
```

## Reminders baked into `.github/copilot-instructions.md` (do not duplicate here — go read it)

Test file/method naming and stub conventions, auto-generated file policy, branch naming/folder
rules, serializer architecture summary, and the Document Management archiving rule all live there
and are loaded automatically at the start of every session in this repo — this file only adds
*navigation* context that instructions file intentionally keeps out to stay concise.
