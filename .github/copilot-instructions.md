# Copilot Instructions

## Project Guidelines
- User prefers test cases to include comments explaining the rationale of the assertions, not just descriptive test names.
- Whenever code changes are made, all affected tests must be identified and run, and the task is not finished until all those tests pass.
- For branch names, use PascalCase and avoid dashes; underscores are allowed sparingly. Use branch Features/AnyAtt_Support for this work.

## Tooling / UX Preferences
- Do NOT use the `ask_user` QA panel (multiple-choice question UI) in ANY of this user's repos or other work in this tool. It is too sensitive to stray clicks and questions disappear from view. Always ask questions as a regular chat message instead, and explain each option in enough detail to be understood without re-reading the code.

## Branch Management & Naming
- Do NOT add new branches at the same Git folder level as `master`/`main`. Use Git folders to create semantic branch groups like `/Features`, `/Features/NET10`, `/BugFixes`, etc. Place new working branches inside the appropriate group folder.
- When unsure where to place a new working branch, ASK — do not guess the folder.
- Do NOT create branches with random/auto-generated names — ASK for a branch name.
- Do NOT use dashes in branch names. Follow the user's file/folder naming rules (PascalCase; underscores sparingly).
- When many branches are in use, append the date as a `yyyymmdd` suffix (e.g. `20260613`) to the end of the branch name.
- Keep branch names concise.
- NOTE: the `rename_branch` tool forces kebab-case and flattens slashes; to set a true grouped name like `Features/NET10/DateTimeValidation`, rename via git in the appropriate checkout instead.
- Ensure round-tripping between SDC OM and XML is covered in tests to handle complex and unexpected ad-hoc attribute cases, including support for multiple mixed namespaces and inherited/default namespace usage.
- Include tests for legal and illegal ad-hoc attribute content, with proper escaping and graceful handling of illegal content.
- Unit tests running > 1 second and functional/integration tests running > 10 seconds are considered failed and must be aborted; root cause must be identified and a fix plan presented; tests must never be allowed to enter an infinite loop.

## SDC OM Serializer Architecture
- The SDC OM serializer architecture is contained within the namespace SDC.Schema and includes the following components:
  - `SdcSerializer<T>` uses `XmlSerializer` (System.Xml.Serialization) and handles polymorphism via approximately 150 `[XmlInclude]` attributes on `BaseType`.
  - `SdcSerializerJson<T>` utilizes `Newtonsoft.Json`'s `JsonConvert` with `TypeNameHandling.All` and `ConstructorHandling.AllowNonPublicDefaultConstructor` for polymorphic round-trips, diverging from stock generated JSON serializers which use empty `new JsonSerializerSettings()`.
  - `SdcSerializerBson<T>` employs `Newtonsoft.Json.JsonSerializer` with `BsonDataWriter/BsonDataReader`, which are subclasses of `JsonWriter/JsonReader`, using the same two settings as the JSON serializer but with `DefaultSerialiser = BSonSerializer` for BSON.
  - `SdcSerializerMsgPack<T>` leverages `MsgPack.Cli`'s `MessagePackSerializer<T>.Pack/Unpack` and uses `DefaultSerialiser = MessagePackSerializer` in xsd2code++ options. The generated code follows the same template as JSON/BSON: `SaveToFile`, `LoadFromFile`, `Serialize (returns byte[])`, `Deserialize(byte[])` methods — all encapsulated in the generated class. No special settings are documented for MsgPack.
- All SDC node classes must have a public parameterized constructor (which requires a non-null `parentNode`) and a protected/internal parameterless constructor for deserialization use only.
- BSON bytes are stored as Base64 strings, as confirmed by official documentation. Advanced JSON settings available include DateFormatHandling, DateFormatString, DateParseHandling, DateTimeZoneHandling, DefaultValueHandling, FloatFormatHandling, FloatParseHandling, MissingMemberHandling, NullValueHandling, and StringEscapeHandling.

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

## Auto-Generated File Policy (xsd2code++)
- Files that begin with a comment block containing `<auto-generated>` (in the form `//  <auto-generated>`) are xsd2code++-generated files. **Do NOT edit these files without explicit user permission**, regardless of which folder they reside in.
- Files under `SDC.Schema/SDC.Schema/SDC Constructor Removed/`, `SDC.Schema/SDC.Schema/SDC Unmodified Classes/`, and `SDC.Schema/SDC.Schema/SDC Schema Files/` require special processing during regeneration. During any planning phase where a proposed change would touch these folders, **stop and explicitly notify the user** before proceeding. List the exact files and describe why the change is needed. Wait for approval.
- Prefer alternatives that keep generated files untouched: (a) add the `partial` keyword to the generated class (with explicit permission) and place all custom additions in `SDC.Schema/SDC.Schema/Partial Classes/PartialClasses.cs`, (b) extension methods in `SDC Customized Classes/`, or (c) derived attributes in `SDC Customized Classes/`.
- Files in `SDC Customized Classes/`, `Utility Classes/`, `Extensions/`, `Interfaces/`, and `SDC Serializers/` are the correct places for customizations and may be edited freely.
- `SDC.Schema/SDC.Schema/Partial Classes/PartialClasses.cs` is the canonical file for all consolidated partial-class extensions of generated types.


- When doing a git amend, always check the commit message and adjust it as needed before amending the commit.
- `GIT_TERMINAL_PROMPT=0` is set in the PowerShell profile and must remain set; this suppresses Git's interactive locked-folder deletion prompts that would otherwise silently block agent terminal commands. `core.fscache=true` and `checkout.workers=1` are also set in this repo's local config for the same reason. See `SDC.Schema.Tests/Documentation/GitWorkflow_LockedFolderPrevention.md` for full details and the post-merge cleanup script.
- Always close the VS solution (File → Close Solution) before performing branch checkouts or merges to prevent Windows file-handle locks on project files.
- Ensure all .md documents are included in git, including those archived under an "Archived Plans" subfolder.