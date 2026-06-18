# Copilot Instructions

## Project Guidelines
- User prefers test cases to include comments explaining the rationale of the assertions, not just descriptive test names.
- Whenever code changes are made, all affected tests must be identified and run, and the task is not finished until all those tests pass.
- For branch names, use PascalCase and avoid dashes; underscores are allowed sparingly. Use branch Features/AnyAtt_Support for this work.
- Ensure round-tripping between SDC OM and XML is covered in tests to handle complex and unexpected ad-hoc attribute cases, including support for multiple mixed namespaces and inherited/default namespace usage.
- Include tests for legal and illegal ad-hoc attribute content, with proper escaping and graceful handling of illegal content.
- When test stub files are created with a leading underscore, remove the underscore from the file name once the stubs are completed, including retroactively for files already completed.
- Unit tests running > 1 second and functional/integration tests running > 10 seconds are considered failed and must be aborted; root cause must be identified and a fix plan presented; tests must never be allowed to enter an infinite loop.

## Document Management
- When .md plan/design documents (e.g., concurrency/async plans) are superseded or were produced by weaker/earlier model passes, archive them into an "Archived Plans" subfolder (preserve git history via `git mv`), add a README in that subfolder explaining provenance and listing the canonical replacements, repoint any cross-references in the keeper docs, and keep only the current/accurate docs in the active folder going forward. Keep summary/index docs like the Archived Plans README.md up to date whenever docs are moved in or out.

## Git Workflow
- When doing a git amend, always check the commit message and adjust it as needed before amending the commit.
- Ensure all .md documents are included in git, including those archived under an "Archived Plans" subfolder.