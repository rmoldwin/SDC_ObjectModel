# SDC.Schema.QA.ExampleGenerator

## What the project is

`SDC.Schema.QA.ExampleGenerator` is a console application that generates the numbered example artifacts referenced by the Quality Assurance (QA) best-practices documentation. Instead of hand-writing sample snippets, it constructs real Structured Data Capture (SDC) object-model trees through the public library Application Programming Interface (API), serializes them, and writes example outputs that the documentation can point to without drifting away from the code.

## Basic architecture

- `Program.cs` is the entire application entry point and contains the observed generation flow.
- The program creates an output root, then writes:
  - `xml\01-construction-basic-form.xml` and `json\01-construction-basic-form.json` for a basic construction example in Extensible Markup Language (XML) and JavaScript Object Notation (JSON).
  - `xml\02-mutation-before-move.xml` and `xml\02-mutation-after-move.xml` for a move/mutation example.
  - `xml\03-adhoc-attributes-mixed-namespaces.xml` for the mixed-namespace ad-hoc attribute example described in `docs\architecture\qa-best-practices.md`.
  - sample Markdown and HyperText Markup Language (HTML) QA reports in a sibling `reports\` folder by running `SDC.Schema.QA.Rules.QaEngine`.
- The project file `SDC.Schema.QA.ExampleGenerator.csproj` references both `..\SDC.Schema\SDC.Schema.csproj` and `..\SDC.Schema.QA\SDC.Schema.QA.csproj`. It depends on the core object model plus the QA engine; no other project takes a build-time dependency on this console application.
- Although the generated artifacts live outside this project folder at runtime, the numbered-example set documented in `docs\architecture\qa-best-practices.md` is conceptually downstream of this program.

## State of completion

- Rough scale: 1 C# source file implements the full generator, and the current observed output set covers three numbered examples plus two sample QA reports in both Markdown and HTML forms.
- TODO and FIXME notes: no `TODO` or `FIXME` matches were found in this project folder during a text search.
- Open roadmap issues clearly scoped to this project: none are called out directly in `docs/roadmap.md`.
