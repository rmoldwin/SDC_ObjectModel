# Copilot Instructions

## Project Guidelines
- User prefers test cases to include comments explaining the rationale of the assertions, not just descriptive test names.
- Whenever code changes are made, all affected tests must be identified and run, and the task is not finished until all those tests pass.
- For branch names, use PascalCase and avoid dashes; underscores are allowed sparingly. Use branch Features/AnyAtt_Support for this work.
- Ensure round-tripping between SDC OM and XML is covered in tests to handle complex and unexpected ad-hoc attribute cases, including support for multiple mixed namespaces and inherited/default namespace usage.
- Include tests for legal and illegal ad-hoc attribute content, with proper escaping and graceful handling of illegal content.
- Unit tests running > 1 second and functional/integration tests running > 10 seconds are considered failed and must be aborted; root cause must be identified and a fix plan presented; tests must never be allowed to enter an infinite loop.

## Test File/Method Organization Rules
- Stub method = [TestMethod] with no body or only a trivial lone Assert.IsNotNull(sut) with no other behavioral logic — must have _ prefix on method name.
- Stub file = any .cs test file containing at least one stub method — must have _ prefix on filename.
- Remove _ prefix from method when it has real behavioral assertions; remove _ from filename when all methods are complete.
- True shared helper methods used across multiple test files go in a file named `*_Helper.cs` or `*Helpers.cs`.
- Output-heavy diagnostic/manual-review test methods (methods that print extensive output for human inspection, with or without formal assertions) belong in files named ending with `_ForManualReview.cs`. Such methods must also catch and re-throw (or Assert.Fail on) unexpected exceptions so they still report failure properly.
- Old `*Test.cs` files (without the 's') that duplicate newer `*Tests.cs` files should be consolidated: unique tests extracted to the newer file, old file deleted.
- Empty stub .cs files (0 bytes) should be deleted.
- Duplicate test methods should be merged, preferring the file with the better naming convention (INavigateExtensionsTests.cs preferred over BaseTypeTests.cs for navigation coverage).

## Document Management
- When .md plan/design documents (e.g., concurrency/async plans) are superseded or were produced by weaker/earlier model passes, archive them into an "Archived Plans" subfolder (preserve git history via `git mv`), add a README in that subfolder explaining provenance and listing the canonical replacements, repoint any cross-references in the keeper docs, and keep only the current/accurate docs in the active folder going forward. Keep summary/index docs like the Archived Plans README.md up to date whenever docs are moved in or out.

## Session Continuity
- After completing each meaningful chunk of work, proactively produce and keep up-to-date: (a) an updated conversation/work summary, (b) updated on-disk context/handoff documents, and (c) a kickstart prompt suitable for resuming in a freshly restarted session. Generate and maintain an organized plan document at the start of multi-step work, update it as each sub-activity completes, and use it as the kickstart prompt base.


## Git Workflow
- When doing a git amend, always check the commit message and adjust it as needed before amending the commit.
- Ensure all .md documents are included in git, including those archived under an "Archived Plans" subfolder.