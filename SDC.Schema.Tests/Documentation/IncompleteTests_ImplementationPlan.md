# Incomplete Test Implementation Plan

_Original branch: Refactor/TestFolderOrganization — merged into Features/Net11Upgrade (HEAD: eb452a8)_

---

## Section 1 — Full Audit: Incomplete Test Methods

### Category A — Timer-shell stubs (no test logic whatsoever)

| Folder | File | Method | Issue |
|--------|------|--------|-------|
| `Functional\Serialization` | `SdcSerializationTests.cs` | `JsonToXML()` | Body is only `Setup.TimerStart` / `TimerPrintSeconds`; no conversion logic, no assertions |
| `Functional\Serialization` | `SdcSerializationTests.cs` | `SdcToJson()` | Body is only `Setup.TimerStart` / `TimerPrintSeconds`; no serialization logic, no assertions |

---

### Category B — Methods with no assertions (execute code but never verify a result)

| Folder | File | Method | Issue |
|--------|------|--------|-------|
| `Functional\Serialization` | `SdcSerializationTests.cs` | `DeserializeDEFromPath()` | Deserializes DE, calls `GetXml()` and `GetJson()`, but contains **zero assertions** |
| `Functional\Serialization` | `SdcSerializationTests.cs` | `DeserializeDEFromXml()` | Deserializes DE from raw XML string, calls `GetXml()`, but contains **zero assertions** |
| `Functional\Serialization` | `SdcSerializationTests.cs` | `DeserializeDemogFormDesignFromPath()` | Deserializes DemogFormDesign, serialises to JSON/XML via Newtonsoft, but contains **zero assertions** |
| `Functional\Serialization` | `SdcSerializationTests.cs` | `DeserializeFormDesignFromPathSimple()` | Deserializes and re-serializes FormDesign as XML and JSON, but contains **zero assertions** |
| `Functional\Serialization` | `SdcSerializationTests.cs` | `DeserializePkgFromPath()` | Deserializes a package, adds a node, reads it back — **zero assertions** |
| `Functional\Serialization` | `SdcSerializationTests.cs` | `DeserializePkgFromPath_AddName()` | Deserializes package with name-method, serializes — **zero assertions** |
| `Functional\Serialization` | `FormDesignSerializerUtilTests.cs` | `DeserializeTest()` | Deserializes XML, prints counts/types, **zero assertions** |
| `Functional` | `MiscTests.cs` | `Fibonacci()` | Computes Fibonacci; `.ToTuple()` called; **zero assertions** |
| `Functional` | `MiscTests.cs` | `GetHtmlItems()` | Iterates IET nodes via large switch; **zero assertions** |
| `Functional` | `MiscTests.cs` | `Test()` | Instantiates Setup and reads two version XML properties; **zero assertions** |
| `UtilityClasses\AttrMetadata` | `CompareTreesTests.cs` | `GetIETnodesRemovedInNewTest()` | Assigns `_comparer.GetIETnodesRemovedInNew` to local `C`; **zero assertions** |
| `UtilityClasses\AttrMetadata` | `CompareTreesTests.cs` | `ChangeSummaryTest()` | ~100 lines of `Console.WriteLine` output; **zero assertions** |

---

### Category C — Methods with trivially-passing assertions (assert things that are always true)

| Folder | File | Method | Issue |
|--------|------|--------|-------|
| `Functional\TreeOperations` | `ChangeTypeTests.cs` | `ChangeTypeTest()` | `new ChangeType()` then `Assert.IsNotNull` — construction cannot return null; no ChangeType behavior tested |
| `Functional\TreeOperations` | `ChangeTypeTests.cs` | `TargetItemIDPropertyTest()` | Assigns via `Activator.CreateInstance`, asserts `IsNotNull` — just assigned, trivially non-null; does not verify value retention |
| `Functional\TreeOperations` | `ChangeTypeTests.cs` | `TargetItemNamePropertyTest()` | Same pattern — trivially non-null |
| `Functional\TreeOperations` | `ChangeTypeTests.cs` | `TargetItemXPathPropertyTest()` | Same pattern — trivially non-null |
| `Functional\TreeOperations` | `ChangeTypeTests.cs` | `NewValuePropertyTest()` | Same pattern — trivially non-null |

---

## Section 2 — Empty Files: Coverage Analysis

| Folder | File | Covered By | Status |
|--------|------|------------|--------|
| `OMTests\` | `BaseTypeTests.cs` | `OM\BaseTypeTests.cs` | ✅ Fully covered — no new tests needed |
| `OMTests\` | `BaseTypeThreadSafetyTests.cs` | `OM\ThreadSafety\BaseTypeThreadSafetyTests.cs` | ✅ Fully covered — no new tests needed |
| `OMTests\` | `ListItemTypeTests.cs` | `OM\ListItemTypeTests.cs` | ✅ Fully covered — no new tests needed |
| `OMTests\` | `QuestionItemTypeTests.cs` | `OM\QuestionItemTypeTests.cs` | ✅ Fully covered — no new tests needed |
| `OMTests\` | `SectionItemTypeTests.cs` | `OM\SectionItemTypeTests.cs` | ✅ Fully covered — no new tests needed |
| `OMTests\` | `ThreadSafetyReproTests.cs` | `OM\ThreadSafety\ThreadSafetyReproTests.cs` | ✅ Fully covered — no new tests needed |
| `Functional\TreeOperations\` | `_MoveTests.cs` | `Functional\TreeOperations\MoveTests.cs` | ✅ Stub file by convention (underscore prefix); no new tests needed |
| `UtilityClasses\AttributeInfo\` | `CompareTreesTests.cs` | `UtilityClasses\AttrMetadata\CompareTreesTests.cs` | ✅ Fully covered — no new tests needed |
| `Utility Classes\Extensions\` | `BaseTypeExtensionsTests.cs` | `UtilityClasses\Extensions\BaseTypeExtensionsTests.cs` | ✅ Fully covered — no new tests needed |
| `Utility Classes\Extensions\` | `BaseTypeExtensions_ForManualReview.cs` | `UtilityClasses\Extensions\BaseTypeExtensions_ForManualReview.cs` | ✅ Fully covered — no new tests needed |
| `Utility Classes\Extensions\` | `ITopNodeExtensionsTests.cs` | `UtilityClasses\Extensions\ITopNodeExtensionsTests.cs` | ✅ Fully covered — no new tests needed |

---

## Section 3 — Implementation Plan

### 3.1 `Functional\Serialization\SdcSerializationTests.cs`

**`DeserializeDEFromPath`** — add:
- `Assert.IsNotNull(DE)`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXML))`
- `Assert.IsTrue(myXML.Contains("<DataElement"))` (DE XML root)
- Assert JSON is non-empty

**`DeserializeDEFromXml`** — add:
- `Assert.IsNotNull(DE)`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXML))`

**`DeserializeDemogFormDesignFromPath`** — add:
- `Assert.IsNotNull(FD)`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXML))`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(json))`
- Assert round-tripped XML doc is non-null

**`DeserializeFormDesignFromPathSimple`** — add:
- `Assert.IsNotNull(FD)`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXml))`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myJson))`

**`DeserializePkgFromPath`** — add:
- `Assert.IsNotNull(Pkg)`
- `Assert.IsNotNull(DFD)`
- `Assert.IsNotNull(FD)` (the non-DemogFormDesign FormDesign)
- `Assert.IsNotNull(Q)`
- `Assert.IsNotNull(DI2)` (added DI is first in ChildItemsNode)
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXML))`

**`DeserializePkgFromPath_AddName`** — add:
- `Assert.IsNotNull(Pkg)`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXML))`
- Assert at least one node has a non-empty name

**`JsonToXML`** — implement:
- Deserialize FormDesign from XML
- Convert to JSON via `GetJson()`
- Reconstruct XML from JSON via `JsonConvert.DeserializeXmlNode()`
- Assert round-tripped XML is non-null and non-empty

**`SdcToJson`** — implement:
- Deserialize FormDesign from XML
- Serialize to JSON via `GetJson()` 
- Assert JSON is non-null and non-empty
- Verify at least one known field (e.g. `"ID"`) appears in the JSON string

---

### 3.2 `Functional\Serialization\FormDesignSerializerUtilTests.cs`

**`DeserializeTest`** — add:
- `Assert.IsNotNull(FD)`
- `Assert.IsTrue(FD.Nodes.Count > 0, ...)`
- `Assert.IsFalse(string.IsNullOrWhiteSpace(myXML))`

---

### 3.3 `Functional\MiscTests.cs`

**`Fibonacci`** — add:
- Assert `a.curr == 55` (Fib(9) per the inline recurrence: 1,1,2,3,5,8,13,21,34,55)
- Assert `a.prev == 34`

**`GetHtmlItems`** — add after the loop:
- Assert `fd.IETnodes` is non-empty (proves deserialization succeeded and loop ran)
- Assert the `sGuid` lookup for question "53309.100004300" returned a non-null node

**`Test`** — add:
- `Assert.IsNotNull(bstV1)`
- `Assert.IsNotNull(bstV2)`

---

### 3.4 `Functional\TreeOperations\ChangeTypeTests.cs`

**`ChangeTypeTest`** — strengthen:
- Add `Assert.AreEqual("ChangeType", sut.GetType().Name)` (verifies concrete type)
- Verify `ShouldSerializeTargetItemID()` returns false on a fresh instance (pre-condition)

**`TargetItemIDPropertyTest`** — strengthen:
- After assignment, call `Assert.AreSame(sut.TargetItemID, sut.TargetItemID)` (identity stable) — trivial but proves round-trip; better: assert the property round-trips through `ShouldSerializeTargetItemID()` → `true`

**`TargetItemNamePropertyTest`** — strengthen same way via `ShouldSerializeTargetItemName()`

**`TargetItemXPathPropertyTest`** — strengthen via `ShouldSerializeTargetItemXPath()`

**`NewValuePropertyTest`** — strengthen via `ShouldSerializeNewValue()`

---

### 3.5 `UtilityClasses\AttrMetadata\CompareTreesTests.cs`

**`GetIETnodesRemovedInNewTest`** — add:
- `Assert.IsNotNull(C)` 
- `Assert.AreEqual(0, C.Count)` (V1→V2 removes 0 IET nodes; the removed sub-node is non-IET, consistent with `GetNodesRemovedInNewTest`)

**`ChangeSummaryTest`** — add after the `foreach` loop:
- Assert `dDifNodeIET` count > 0 (proves comparer produced results for V1→V5)
- Assert at least one node has `isAttListChanged || isMoved || isNew`

---

## Section 4 — User Decisions (Resolved)

1. **`GetIETnodesRemovedInNewTest`**: The implementation (asserting 0 removed IET nodes for V1→V2) is **correct and sufficient**. No further changes needed.

2. **`ChangeSummaryTest`**: The implemented assertions (non-null, non-empty diff list, at least one meaningful change flag) are **sufficient**. Tests like this serve for both regression and manual bug review — no specific node identity assertion required.

3. **`GetHtmlItems`**: The hard-coded question ID `"53309.100004300"` was documented in a `FRAGILITY NOTE` comment directly above the assertion. The assertion itself was left unchanged (hard fail if not found). This guards against silent fixture drift while preserving the original intent.

4. **Empty stub files in `OMTests\`**: **Deleted** — all 6 files (`BaseTypeTests.cs`, `BaseTypeThreadSafetyTests.cs`, `ListItemTypeTests.cs`, `QuestionItemTypeTests.cs`, `SectionItemTypeTests.cs`, `ThreadSafetyReproTests.cs`) were untracked empty files; canonical coverage exists in `OM\`.

5. **`_MoveTests.cs`**: **Deleted** — the file was empty and untracked. `MoveTests.cs` provides the canonical coverage.

---

## Section 5 — Completed / Remaining Tracker

| Method | File | Status |
|--------|------|--------|
| `DeserializeDEFromPath` | SdcSerializationTests.cs | ✅ Done |
| `DeserializeDEFromXml` | SdcSerializationTests.cs | ✅ Done |
| `DeserializeDemogFormDesignFromPath` | SdcSerializationTests.cs | ✅ Done |
| `DeserializeFormDesignFromPathSimple` | SdcSerializationTests.cs | ✅ Done |
| `DeserializePkgFromPath` | SdcSerializationTests.cs | ✅ Done |
| `DeserializePkgFromPath_AddName` | SdcSerializationTests.cs | ✅ Done |
| `JsonToXML` | SdcSerializationTests.cs | ✅ Done |
| `SdcToJson` | SdcSerializationTests.cs | ✅ Done |
| `DeserializeTest` | FormDesignSerializerUtilTests.cs | ✅ Done |
| `Fibonacci` | MiscTests.cs | ✅ Done |
| `GetHtmlItems` | MiscTests.cs | ✅ Done (fragility note added; assertion unchanged) |
| `Test` | MiscTests.cs | ✅ Done |
| `ChangeTypeTest` | ChangeTypeTests.cs | ✅ Done |
| `TargetItemIDPropertyTest` | ChangeTypeTests.cs | ✅ Done |
| `TargetItemNamePropertyTest` | ChangeTypeTests.cs | ✅ Done |
| `TargetItemXPathPropertyTest` | ChangeTypeTests.cs | ✅ Done |
| `NewValuePropertyTest` | ChangeTypeTests.cs | ✅ Done |
| `GetIETnodesRemovedInNewTest` | CompareTreesTests.cs | ✅ Done |
| `ChangeSummaryTest` | CompareTreesTests.cs | ✅ Done |

**All 19 targeted methods completed. Build passes with zero errors.**

> **Session update (SdcSerializationTests.cs review):** The file shown above already
> contains all implemented assertions from the prior pass. No further changes were needed
> to `SdcSerializationTests.cs` in this session. See Section 7 for the round-trip
> fidelity coverage audit prompted by this review.

---

## Section 6 — Cleanup Actions (Post-Implementation)

| Action | Detail | Status |
|--------|--------|--------|
| Fragility note in `GetHtmlItems` | Added `FRAGILITY NOTE` comment above hard-coded ID assertion in `MiscTests.cs` | ✅ Done |
| Delete `OMTests\BaseTypeTests.cs` | Empty, untracked; covered by `OM\BaseTypeTests.cs` | ✅ Deleted |
| Delete `OMTests\BaseTypeThreadSafetyTests.cs` | Empty, untracked; covered by `OM\ThreadSafety\BaseTypeThreadSafetyTests.cs` | ✅ Deleted |
| Delete `OMTests\ListItemTypeTests.cs` | Empty, untracked; covered by `OM\ListItemTypeTests.cs` | ✅ Deleted |
| Delete `OMTests\QuestionItemTypeTests.cs` | Empty, untracked; covered by `OM\QuestionItemTypeTests.cs` | ✅ Deleted |
| Delete `OMTests\SectionItemTypeTests.cs` | Empty, untracked; covered by `OM\SectionItemTypeTests.cs` | ✅ Deleted |
| Delete `OMTests\ThreadSafetyReproTests.cs` | Empty, untracked; covered by `OM\ThreadSafety\ThreadSafetyReproTests.cs` | ✅ Deleted |
| Delete `Utility Classes\Extensions\BaseTypeExtensionsTests.cs` | Empty, untracked; covered by `UtilityClasses\Extensions\BaseTypeExtensionsTests.cs` | ✅ Deleted |
| Delete `Utility Classes\Extensions\ITopNodeExtensionsTests.cs` | Empty, untracked; covered by `UtilityClasses\Extensions\ITopNodeExtensionsTests.cs` | ✅ Deleted |
| Delete `Utility Classes\Extensions\BaseTypeExtensions_ForManualReview.cs` | Empty, untracked; covered by `UtilityClasses\Extensions\BaseTypeExtensions_ForManualReview.cs` | ✅ Deleted |
| Delete `UtilityClasses\AttributeInfo\CompareTreesTests.cs` | Empty, untracked; covered by `UtilityClasses\AttrMetadata\CompareTreesTests.cs` | ✅ Deleted |
| Delete `Functional\TreeOperations\_MoveTests.cs` | Empty, untracked stub; covered by `MoveTests.cs` | ✅ Deleted |
| Final build verification | Build succeeded after all deletions | ✅ Clean |

---

## Section 7 — Round-Trip Fidelity: Serializer Fixes and CompareTrees Tests

### 7.1 User requirements (this session)

1. The MsgPack XML-tunnel workaround must be reverted — if MsgPack.Cli cannot handle the
   SDC object model natively, the test must **fail**.
2. All four serialization formats (XML, JSON, BSON, MsgPack) must have 100% round-trip
   fidelity tests verified by `CompareTrees`. No workarounds; failures must be visible.

---

### 7.2 Serializer fixes applied

| Serializer | File | Fix |
|-----------|------|-----|
| `SdcSerializerJson<T>` | `SDC Serializers/SdcSerializerJson.cs` | Added `TypeNameHandling.All` to `SerializeJson` and `DeserializeJson`. `ConstructorHandling.AllowNonPublicDefaultConstructor` was already present in `DeserializeJson` and is retained. |
| `SdcSerializerBson<T>` | `SDC Serializers/SdcSerializerBson.cs` | Added `TypeNameHandling.All` and `ConstructorHandling.AllowNonPublicDefaultConstructor` to the `SerializerBson` property initializer (was `new JsonSerializer()`). |
| `SdcSerializerMsgPack<T>` | `SDC Serializers/SdcSerializerMsgPack.cs` | **Reverted** the XML-as-UTF8-bytes tunnel. Restored `MessagePackSerializer<T>` field type and `Pack`/`Unpack` calls. This serializer now uses the true MsgPack.Cli binary protocol. |

> **Security note on `TypeNameHandling.All`:** Writing `"$type"` discriminators is safe for
> internal/trusted round-trips. When accepting JSON or BSON from untrusted sources, supply a
> custom `SerializationBinder` that whitelists only types in the `SDC.Schema` assembly.

---

### 7.3 Round-trip fidelity tests

Four new tests were added to `SdcSerializationTests.cs`. Each test:
- Loads `Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml` (large complex SDC fixture)
- Serializes the tree to the target format
- Deserializes back to a new `FormDesignType` instance
- Calls `AssertRoundTripFidelity<T>`, a shared private helper that uses `CompareTrees<T>`
  to verify zero differences across all four comparison dimensions

The `AssertRoundTripFidelity` helper asserts:
- `GetIETnodesAddedInNew.Count == 0`
- `GetIETnodesRemovedInNew.Count == 0`
- `GetNodesAddedInNew.Count == 0`
- `GetNodesRemovedInNew.Count == 0`
- `GetIETattDiffs` contains zero nodes with any of: `isAttListChanged`, `isMoved`,
  `isNew`, `isParChanged`, `hasAddedSubNodes`, `hasRemovedSubNodes`

| Test | Format | Location | Expected outcome |
|------|--------|----------|-----------------|
| `XmlRoundTripFidelityTest` | XML | `SdcSerializationTests.cs` | ✅ Should pass — XML is the canonical SDC format |
| `JsonRoundTripFidelityTest` | JSON | `SdcSerializationTests.cs` | ✅ Should pass after `TypeNameHandling.All` fix |
| `BsonRoundTripFidelityTest` | BSON | `SdcSerializationTests.cs` | ✅ Should pass after both BSON fixes |
| `MsgPackRoundTripFidelityTest` | MsgPack | `SdcSerializationTests.cs` | ❓ Will fail if `MsgPack.Cli` cannot handle `XmlElement`/`XmlAttribute` fields — **correct and expected** |

The existing three `Clone*` tests in `MoveTests.cs` (`CloneSdcSubtreeBsonTest`,
`CloneSdcSubtreeJsonTest`, `CloneSdcSubtreeMpackTest`) were also updated with corrected
comments reflecting the serializer fixes and the MsgPack workaround revert.

---

### 7.4 Action items

| Item | Priority | Status |
|------|----------|--------|
| Fix `SdcSerializerJson<T>` — `TypeNameHandling.All` | High | ✅ Done |
| Fix `SdcSerializerBson<T>` — `TypeNameHandling.All` + `ConstructorHandling` | High | ✅ Done |
| Revert MsgPack XML-tunnel workaround | High | ✅ Done |
| Add `AssertRoundTripFidelity` helper + 4 fidelity tests | High | ✅ Done |
| Update `CloneSdcSubtree*` comments in `MoveTests.cs` | Low | ✅ Done |
| Add round-trip fidelity tests for `DataElementType`, `DemogFormDesignType`, `RetrieveFormPackageType` | Low | ⚠️ Open — not yet prioritized |
