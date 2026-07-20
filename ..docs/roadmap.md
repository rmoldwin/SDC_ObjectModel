# Roadmap

Every roadmap item below must have a linked GitHub issue, and every relevant open GitHub issue
(bug, fix, improvement, to-do) must appear here. Keeping this file and the issue tracker in sync
is one of the responsibilities of the docs-sync skill ([issue #38](https://github.com/rmoldwin/SDC_ObjectModel/issues/38),
see [skills/](skills/)).

## Documentation restructuring (this project)

| Status | Item | Issue |
|---|---|---|
| Done | Migrate the two public GitHub gists ("SDC OM Validation" and "SDC OM Rules") into `..docs/architecture/validation.md` and `..docs/architecture/rules.md`, then delete the gists | — |
| Done | Move `..docs/` from inside the `SDC.Schema` project folder to the top level of the repository, alongside `sessions/` | — |
| Done | Consolidate `SDC.Schema.Tests/Documentation` technical docs (XML Schema Definition, or XSD, vs. .NET type divergence notes; OM tree-stability notes; thread-safety investigation notes; validation-pipeline unification notes; BSON/JSON serializer bug history) into `..docs/architecture/`, archiving the 12 fully-superseded originals into `SDC.Schema.Tests/Documentation/Archived Plans/` | — |
| Done | Move session handoff/kickstart documents (13 files) out of `SDC.Schema.Tests/Documentation` into the top-level `sessions/` folder | — |
| Planned | Create every-roadmap-item-has-an-issue GitHub issues and cross-link them here (this stage) | — |
| Planned | Create a docs-sync AI skill that checks `..docs/`, `..docs/skills/`, plan documents, the wiki, and archived docs for consistency (including glossary completeness and roadmap/issue sync) | [#38](https://github.com/rmoldwin/SDC_ObjectModel/issues/38) |
| Planned | Begin populating the project wiki with settled architecture content and images/links from the SDC Technical Reference Guide (TRG) | [#36](https://github.com/rmoldwin/SDC_ObjectModel/issues/36) |

## Serialization

| Status | Item | Issue |
|---|---|---|
| Planned | Migrate date/time `timeZone` from a validated string to a strongly-typed `TimeSpan`/offset representation (see [architecture/xsd-dotnet-type-mapping.md](architecture/xsd-dotnet-type-mapping.md)) | [#9](https://github.com/rmoldwin/SDC_ObjectModel/issues/9) (see also related [#13](https://github.com/rmoldwin/SDC_ObjectModel/issues/13)) |
| Planned | Fix MessagePack (MsgPack) round-trip support for polymorphic SDC Object Model (OM) types, or replace the MsgPack-based serializer (see [architecture/serialization.md](architecture/serialization.md)) | [#35](https://github.com/rmoldwin/SDC_ObjectModel/issues/35) |
| Low priority | Add optional `async` file I/O overloads (`LoadAsync`/`SaveAsync`) to the four serializers — a responsiveness convenience only, not a correctness or thread-safety need | [#26](https://github.com/rmoldwin/SDC_ObjectModel/issues/26) |
| Low priority | Add trim-safe XML serialization support (e.g. `Microsoft.XmlSerializer.Generator`) for production WebAssembly (WASM) deployments, so `PublishTrimmed` doesn't need to stay disabled | [#18](https://github.com/rmoldwin/SDC_ObjectModel/issues/18) |

## Validation

| Status | Item | Issue |
|---|---|---|
| Planned | Thread the existing parse-error list through the public `AddDataType`/`AddQuestionResponseField` overloads, and add a consistent logging/error-collection mechanism for malformed or out-of-range numeric input (currently silently dropped in the public API) | [#6](https://github.com/rmoldwin/SDC_ObjectModel/issues/6) |
| Planned | Fix five confirmed numeric `ResponseType` round-trip/validation divergences between XML Schema Definition (XSD) and .NET/the serializers (JSON can't round-trip large whole-number decimals; BSON loses precision on high-precision decimals and rejects unsigned values above `long.MaxValue`; the digit-count and exclusive-range facet checks are slightly off at extreme values) | [#7](https://github.com/rmoldwin/SDC_ObjectModel/issues/7) |
| Planned | Ensure no code path (setter or deserialization, any format) ever stores a value that fails validation — always keep the prior/unset value and raise an event naming the offending value ("soft-reject", extending the existing contract to fully cover deserialization) | [#8](https://github.com/rmoldwin/SDC_ObjectModel/issues/8) |
| Planned | Implement a working `IVal.ValXmlString` (generic "value as text" accessor) for the ~32 of ~35 SDC data types where it currently throws instead of working, and add the missing `IVal` interface declaration to `string_Stype` (see [architecture/tree-operations.md](architecture/tree-operations.md#known-pre-existing-unrelated-gap-surfaced-by-this-work)) | [#46](https://github.com/rmoldwin/SDC_ObjectModel/issues/46) |
| Planned | Audit and complete validation logic + automated test coverage for every SDC data type (not just `IVal.ValXmlString`) — inventory current gaps, fix them, and add boundary/validation tests for every type lacking one | [#47](https://github.com/rmoldwin/SDC_ObjectModel/issues/47) |
| Bug | Fix `dateTimeStamp_Stype.val` setter, which always throws a `ValidationException` due to a broken `RegularExpressionAttribute` (blocks legal `dateTimeStamp` values) | [#50](https://github.com/rmoldwin/SDC_ObjectModel/issues/50) |
| Planned | Implement `IVal.ValXmlString` for `XML_Stype` and `HTML_Stype`, deferred from the ad hoc attribute (`AnyAtt`)/`IVal` work (companion to #46) | [#49](https://github.com/rmoldwin/SDC_ObjectModel/issues/49) |

## Tree operations (copy/paste/graft/repeat/inject)

| Status | Item | Issue |
|---|---|---|
| Done | Fix a null-target-slot bug in the shared `Move()` attach path used by `CopyPaste()`/`Graft()`, and add regression tests for it plus `CopyPaste()` deep-clone correctness (see [architecture/tree-operations.md](architecture/tree-operations.md)) | — |
| Done | Add `InjectSubtree()` (general-purpose same-tree/cross-tree subtree injection, e.g. for `InjectForm`-style composition) with `RepeatCounter`-based suffix assignment and full test coverage | — |
| Done | Add `InjectSubtreeFromTemplate()` so injected/repeated subtrees are response-free by construction (cloned from the source Form Design File, or FDF, template rather than the live instance) | — |


## Thread safety (WebAssembly, or WASM, multi-threading — separate from the completed desktop investigation)

> See the caveat banner at the top of [architecture/thread-safety.md](architecture/thread-safety.md):
> these issues reuse some of the same `TS-#` labels as the completed desktop thread-safety work for
> **different, still-open** bugs found under real WASM multi-threading.

| Status | Item | Issue |
|---|---|---|
| Planned | Migrate the ambient `[ThreadStatic]` `LastTopNode` field (and related deserialization flags) to `AsyncLocal<T>`, since `[ThreadStatic]` does not protect against an `await` occurring mid-tree-build in WASM/async scenarios | [#17](https://github.com/rmoldwin/SDC_ObjectModel/issues/17) |
| Planned | Root-cause a duplicate-key exception on `PredActionType` seen under read/write lock contention in real multi-threaded WASM | [#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20), [#25](https://github.com/rmoldwin/SDC_ObjectModel/issues/25) |
| Planned | Root-cause an array-copy exception (`Arg_LongerThanDestArray`) seen when multiple threads run tree-comparison concurrently on specific test trees under WASM | [#21](https://github.com/rmoldwin/SDC_ObjectModel/issues/21) |
| Planned | Re-test the `GuidAssignment` deadlock under 4-thread `Barrier` synchronization on WASM after the underlying static-cache fix, to confirm it's resolved (root cause suspected to be a test-design issue with `Barrier`, not an SDC OM defect — see also [#22](https://github.com/rmoldwin/SDC_ObjectModel/issues/22), already fixed by switching to `CountdownEvent`) | [#24](https://github.com/rmoldwin/SDC_ObjectModel/issues/24) |
| Planned | Replace the non-thread-safe plain `Dictionary` reflection binding cache (`dListPropInfoElements`/`dListPropInfoAttributes` in `SdcUtil.cs`) with a `ConcurrentDictionary`, to fix a reflection-cache race that produces misleading "cannot be attached" errors under concurrent node construction | [#23](https://github.com/rmoldwin/SDC_ObjectModel/issues/23) |
| Planned | Investigate intermittent, run-order-dependent test failures in `SDC.Schema.QA.Tests` traced to `[ThreadStatic]`/ambient async-local state leaking across pooled worker threads when MSTest method-level parallelism is enabled (workaround already applied: parallelism disabled for that test project) | [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28) |

## WebAssembly (WASM) / Blazor scripting

| Status | Item | Issue |
|---|---|---|
| Planned | Design and implement an adaptive multithreading optimizer for Blazor WASM clients that tunes active concurrency at runtime (device/browser capability unknown ahead of time), building on the Phase 2 findings in [architecture/wasm-blazor.md](architecture/wasm-blazor.md) | [#52](https://github.com/rmoldwin/SDC_ObjectModel/issues/52) |
| Deferred (Phase 2) | Design an Object Model (OM)-level event model so scripts attached to IdentifiedExtensionType (IET) nodes can be triggered by user interaction (selection, value change, form entry/submission) | [#14](https://github.com/rmoldwin/SDC_ObjectModel/issues/14) |
| Deferred (Phase 2) | Verify script hashes against a published, authoritative registry of approved hashes for known SDC template versions, to protect against a maliciously crafted SDC file where both the script source and its hash have been replaced together | [#15](https://github.com/rmoldwin/SDC_ObjectModel/issues/15) |
| Deferred (Phase 2) | Support in-browser C# script compilation via the Roslyn compiler platform, including a `WasmReferenceProvider` that fetches reference assemblies from the WASM boot manifest instead of relying on desktop-only assembly discovery | [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16) |

## Rules engine

| Status | Item | Issue |
|---|---|---|
| Future ("major surgery" — requires design discussion before implementation) | Decide whether to remove or formally document-as-unsupported the recursive, arbitrary-depth predicate-expression composition mechanism (`PredGuardType`'s nested AND/OR/NOT tree-building) in the auto-generated Rules schema types, none of which is currently executed anywhere in the object model | [#29](https://github.com/rmoldwin/SDC_ObjectModel/issues/29) |

## Documentation maintenance

| Status | Item | Issue |
|---|---|---|
| Planned | Finish XML documentation-comment (`<summary>`) coverage across all public SDC.Schema members (see `sessions/XmlAnnotationPlan.md`) | [#37](https://github.com/rmoldwin/SDC_ObjectModel/issues/37) |

All issue links above were verified against the live GitHub issue tracker on 2026-07-20 to avoid
duplicates; only closed issues (already resolved) were excluded from this table. See
[`templates/ISSUES_EXECUTION_PLAN.md`](templates/ISSUES_EXECUTION_PLAN.md) for a prioritized,
dependency-ordered execution plan across all currently open issues.
