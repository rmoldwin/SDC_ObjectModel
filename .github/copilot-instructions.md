# Copilot Instructions

## Project Guidelines
- User prefers test cases to include comments explaining the rationale of the assertions, not just descriptive test names.
- Whenever code changes are made, all affected tests must be identified and run, and the task is not finished until all those tests pass.
- For branch names, use PascalCase and avoid dashes; underscores are allowed sparingly. Use branch Features/AnyAtt_Support for this work.
- Ensure round-tripping between SDC OM and XML is covered in tests to handle complex and unexpected ad-hoc attribute cases, including support for multiple mixed namespaces and inherited/default namespace usage.
- Include tests for legal and illegal ad-hoc attribute content, with proper escaping and graceful handling of illegal content.