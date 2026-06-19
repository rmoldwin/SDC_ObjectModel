# Copilot Instructions

## Project Guidelines
- User prefers test cases to include comments explaining the rationale of the assertions, not just descriptive test names.
- Whenever code changes are made, all affected tests must be identified and run, and the task is not finished until all those tests pass.
- For branch names, use PascalCase and avoid dashes; underscores are allowed sparingly. Use branch Features/AnyAtt_Support for this work.
- Ensure round-tripping between SDC OM and XML is covered in tests to handle complex and unexpected ad-hoc attribute cases, including support for multiple mixed namespaces and inherited/default namespace usage.
- Include tests for legal and illegal ad-hoc attribute content, with proper escaping and graceful handling of illegal content.
- When test stub files are created with a leading underscore, remove the underscore from the file name once the stubs are completed, including retroactively for files already completed. All incomplete test methods (stubs) must have a `_` prefix on the method name. All .cs test files containing any stub methods must also have a `_` prefix on the filename. When a stub is completed with real assertions verifying behavior (not just a trivial lone Assert.IsNotNull on the constructed object), remove its `_` prefix. When all methods in a file are complete, remove the `_` prefix from the filename. A method with only a lone trivial assertion with no other meaningful behavioral test logic is still a stub.
- Unit tests running > 1 second and functional/integration tests running > 10 seconds are considered failed and must be aborted; root cause must be identified and a fix plan presented; tests must never be allowed to enter an infinite loop.
- Test file/method organization rules for SDC.Schema (and all projects):
  - True shared helper methods used across multiple test files go in a file named `*_Helper.cs` or `*Helpers.cs`.
  - Output-heavy diagnostic/manual-review test methods (methods that print extensive output for human inspection, with or without formal assertions) belong in files named ending with `_ForManualReview.cs`. Such methods must also catch and re-throw (or Assert.Fail on) unexpected exceptions so they still report failure properly.
  - Old `*Test.cs` files (without the 's') that duplicate newer `*Tests.cs` files should be consolidated: unique tests extracted to the newer file, old file deleted.
  - Empty stub .cs files (0 bytes) should be deleted.
  - Duplicate test methods should be merged, preferring the file with the better naming convention (INavigateExtensionsTests.cs preferred over BaseTypeTests.cs for navigation coverage).

## Document Management
- When .md plan/design documents (e.g., concurrency/async plans) are superseded or were produced by weaker/earlier model passes, archive them into an "Archived Plans" subfolder (preserve git history via `git mv`), add a README in that subfolder explaining provenance and listing the canonical replacements, repoint any cross-references in the keeper docs, and keep only the current/accurate docs in the active folder going forward. Keep summary/index docs like the Archived Plans README.md up to date whenever docs are moved in or out.

## Session Continuity
- After completing each meaningful chunk of work, proactively produce and keep up-to-date: (a) an updated conversation/work summary, (b) updated on-disk context/handoff documents, and (c) a kickstart prompt suitable for resuming in a freshly restarted session. This guards against crashes and disconnects. Generate and maintain an organized plan document at the start of multi-step work, update it as each sub-activity completes, and use it as the kickstart prompt base.

## Git Workflow
- When doing a git amend, always check the commit message and adjust it as needed before amending the commit.
- Ensure all .md documents are included in git, including those archived under an "Archived Plans" subfolder.