# Changes to AttInfoDif, DifNodeIET and DifNodeIET2 (June–July 2026)

## ⚠️ Correction Notice

**A prior version of this document incorrectly flagged several long-standing struct members as "ADDED" in June 2026.** Git history confirms that `AttInfoDif`, `DifNodeIET`, and `DifNodeIET2` (including most of their members) already existed as of commit `835454d` (**Feb 28, 2023**, `Fred_NugetTest` branch — the lineage that fed into master). This corrected version distinguishes:
- **PRE-EXISTING (2023)** — present since at least Feb 2023, unchanged in name/type since then
- **TYPE CHANGED (Jun 12, 2026)** — member existed since 2023, but its declared type changed
- **ADDED (date)** — genuinely new member introduced during the June 2026 work on this branch

No pre-existing members have been removed from this record — only the provenance labels have been corrected.

---

## Overview

This document summarizes API changes to the attribute difference tracking record structs in the SDC.Schema project, covering both their 2023 origins and the June 2026 enhancements made on the **Features/NET10/Net10Main** branch (via `Features/AnyAtt_Support` and `Features/CompareTrees`). These structs track XML attribute and IET (Internal Element Type) node changes across different versions of SDC trees.

---

## `AttInfoDif` Record Struct – API Changes

Represents metadata for a single XML attribute comparison between old and new SDC node versions.

| # | Member | Type | Status | Notes |
|---|--------|------|--------|-------|
| 1 | `sGuidSubnode` | `string` | 🟦 **PRE-EXISTING (2023)** | ShortGuid identifying the compared node in both versions |
| 2 | `aiPrev` | `AttributeInfo?` | 🟦 **PRE-EXISTING (2023)** | Attribute info for the previous (older) version |
| 3 | `aiNew` | `AttributeInfo?` | 🟦 **PRE-EXISTING (2023)** | Attribute info for the new (current) version |
| 4 | `elementName` | `string?` | 🟦 **PRE-EXISTING (2023)** | XML element name for the compared node |
| 5 | `propertyName` | `string?` | 🟦 **PRE-EXISTING (2023)** | PropertyType name (if applicable) |
| 6 | `displayName` | `string?` | 🟦 **PRE-EXISTING (2023)** | Display name used in reporting |
| 7 | `addedInNew` | `bool?` | 🟦 **PRE-EXISTING (before Jun 1, 2026)** | **Computed property**: `true` if in `aiNew` but not in `aiPrev`; `null` if `aiNew` is absent. Confirmed present by May 26, 2026 (commit `533cc48`); not present in the 2023 baseline, so it was added sometime between 2023 and May 2026 — but predates the June 1, 2026 window. |
| 8 | `removedInNew` | `bool?` | 🟦 **PRE-EXISTING (before Jun 1, 2026)** | **Computed property**: `true` if in `aiPrev` but not in `aiNew`; `null` if `aiPrev` is absent. Same provenance as `addedInNew`. |

### `AttInfoDif` Constructor

**Signature (unchanged since 2023):**
```csharp
AttInfoDif(string sGuidSubnode, AttributeInfo? aiPrev, AttributeInfo? aiNew, 
		   string elementName, string propertyName, string displayName)
```

**Behavior:** Constructor computes derived properties (`addedInNew`, `removedInNew`) based on presence and validity of `aiPrev` and `aiNew` parameters. This computation logic predates June 2026.

---

## `DifNodeIET2` Record Struct – API

🟦 **This entire struct is PRE-EXISTING (2023)** — it was NOT introduced in June 2026 as previously stated. It appears in the Feb 28, 2023 commit (`835454d`) with the same shape it has today.

| # | Member | Type | Constructor Param | Status | Notes |
|---|--------|------|-------------------|--------|-------|
| 1 | `sGuidIET` | `string` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | IET node identifier |
| 2 | `isParChanged` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | Parent node changed |
| 3 | `isMoved` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | Previous sibling changed |
| 4 | `isNew` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | Node in V2 only |
| 5 | `isRemoved` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | Node in V1 only |
| 6 | `isAttListChanged` | `bool` | ✅ Yes (default=`true`) | 🟦 **PRE-EXISTING (2023)** | Attribute list changed |
| 7 | `dlaiDif` | `Dictionary<string, List<AttInfoDif>>?` | ✅ Yes (default=`null`) | 🟦 **PRE-EXISTING (2023)** | Attribute diffs keyed by subnode sGuid |

### `DifNodeIET2` Constructor

**Signature (unchanged since 2023):**
```csharp
DifNodeIET2(string sGuidIET, bool isParChanged, bool isMoved, bool isNew, 
			bool isRemoved, bool isAttListChanged = true, 
			Dictionary<string, List<AttInfoDif>>? dlaiDif = null)
```

**Note:** In the codebase this struct still appears alongside a commented-out self-test line (`//readonly DifNodeIET2 test = new(...)`), a debugging artifact retained since 2023.

---

## `DifNodeIET` Record Struct – API Changes

Comprehensive IET node difference tracking with detailed attribute and sub-node deltas. This struct has the most genuine June 2026 additions of the three.

| # | Member | Type | Init-Only? | Status | Commit | Notes |
|---|--------|------|-----------|--------|--------|-------|
| 1 | `sGuidIET` | `string` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | IET node identifier |
| 2 | `isParChanged` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Parent node changed |
| 3 | `isMoved` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Previous sibling changed |
| 4 | `isNew` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Node in V2 only |
| 5 | `isRemoved` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Node in V1 only |
| 6 | `isAttListChanged` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Attribute list changed |
| 7 | `hasAddedSubNodes` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Node has gained new sub-nodes (was previously misreported as "ADDED Jun 12") |
| 8 | `hasRemovedSubNodes` | `bool` | ✅ Yes | 🟦 **PRE-EXISTING (2023)** | — | Node has removed sub-nodes (was previously misreported as "ADDED Jun 12") |
| 9 | `AddedAttributes` | `IReadOnlyDictionary<string, AttributeInfo>?` | ✅ Yes | 🟩 **GENUINELY ADDED (Jun 12, 2026)** | `6d66406` | Attributes added in new version (key = attr name). Confirmed as only a `//TODO` comment as late as `09ab12c` (Jun 10, 2026); implemented in `6d66406` |
| 10 | `RemovedAttributes` | `IReadOnlyDictionary<string, AttributeInfo>?` | ✅ Yes | 🟩 **GENUINELY ADDED (Jun 12, 2026)** | `6d66406` | Attributes removed in new version (key = attr name) |
| 11 | `ChangedAttributes` | `IReadOnlyDictionary<string, AttributeInfo>?` | ✅ Yes | 🟩 **GENUINELY ADDED (Jun 12, 2026)** | `6d66406` | Attributes with changed values (key = attr name) |
| 12 | `isChanged` | `bool` | ✅ Yes | 🟩 **GENUINELY ADDED (Jun 12, 2026)** | `6d66406` | Node itself changed (includes attributes) |
| 13 | `addedSubNodes` | `IReadOnlyList<BaseType>?` | ✅ Yes | 🟨 **TYPE CHANGED (Jun 12, 2026)** | `6d66406` | Member existed since 2023 as mutable `List<BaseType>?`; changed to `IReadOnlyList<BaseType>?` in `6d66406` |
| 14 | `removedSubNodes` | `IReadOnlyList<BaseType>?` | ✅ Yes | 🟨 **TYPE CHANGED (Jun 12, 2026)** | `6d66406` | Same as above — was `List<BaseType>?` since 2023 |
| 15 | `dlaiDif` | `IReadOnlyDictionary<string, IReadOnlyList<AttInfoDif>>` | ✅ Yes | 🟨 **TYPE CHANGED (Jun 12, 2026)** | `6d66406` | Member existed since 2023 as mutable `Dictionary<string, List<AttInfoDif>>`; changed to nested read-only interfaces in `6d66406` |

### DifNodeIET Constructors

#### Primary Constructor (Init-Only Auto-Properties) — Current Form (Jun 12, 2026+)

```csharp
DifNodeIET(
	string sGuidIET,
	bool isParChanged,
	bool isMoved,
	bool isNew,
	bool isRemoved,
	bool isAttListChanged,
	bool hasAddedSubNodes,
	bool hasRemovedSubNodes,
	IReadOnlyDictionary<string, AttributeInfo>? AddedAttributes,
	IReadOnlyDictionary<string, AttributeInfo>? RemovedAttributes,
	IReadOnlyDictionary<string, AttributeInfo>? ChangedAttributes,
	bool isChanged,
	IReadOnlyList<BaseType>? addedSubNodes,
	IReadOnlyList<BaseType>? removedSubNodes,
	IReadOnlyDictionary<string, IReadOnlyList<AttInfoDif>> dlaiDif
)
```

#### 2023 Baseline Constructor (for comparison — no longer current)

```csharp
DifNodeIET(
	string sGuidIET,
	bool isParChanged,
	bool isMoved,
	bool isNew,
	bool isRemoved,
	bool isAttListChanged,
	bool hasAddedSubNodes,
	bool hasRemovedSubNodes,
	List<BaseType>? addedSubNodes,
	List<BaseType>? removedSubNodes,
	Dictionary<string, List<AttInfoDif>> dlaiDif
)
```

#### Convenience Constructor Overload (Added Jun 12–16, 2026)

**Signature (Accepts Mutable Collections):**
```csharp
DifNodeIET(
	string sGuidIET,
	bool isParChanged,
	bool isMoved,
	bool isNew,
	bool isRemoved,
	bool isAttListChanged,
	bool hasAddedSubNodes,
	bool hasRemovedSubNodes,
	Dictionary<string, AttributeInfo>? AddedAttributes,
	Dictionary<string, AttributeInfo>? RemovedAttributes,
	Dictionary<string, AttributeInfo>? ChangedAttributes,
	bool isChanged,
	List<BaseType>? addedSubNodes,
	List<BaseType>? removedSubNodes,
	Dictionary<string, List<AttInfoDif>>? dlaiDif
)
```

**Behavior:** Converts mutable `Dictionary<>` and `List<>` to immutable `IReadOnlyDictionary<>` and `IReadOnlyList<>` before delegating to the primary constructor. This overload is new in June 2026 and exists specifically to ease the transition from the 2023-era mutable-collection API to the current read-only-collection API.

---

## Summary of Genuine Changes by Commit (Corrected)

| Commit SHA | Date | Struct(s) Affected | Key Changes |
|------------|------|-------------------|-------------|
| `835454d` (baseline) | Feb 28, 2023 | `AttInfoDif`, `DifNodeIET`, `DifNodeIET2` | Original 2023 shape of all three structs — establishes the baseline that most members trace back to |
| `533cc48` | May 26, 2026 | `AttInfoDif` | `addedInNew` / `removedInNew` computed properties confirmed present (introduced sometime between 2023 and this date, but before the June 1 window) |
| `09ab12c` | Jun 10, 2026 | `DifNodeIET` | `AddedAttributes`/`RemovedAttributes`/`ChangedAttributes`/`isChanged` still only `//TODO` comments — not yet implemented |
| `6d66406` | Jun 12, 2026 | `DifNodeIET` | **Genuinely implements** `AddedAttributes`, `RemovedAttributes`, `ChangedAttributes`, `isChanged`; **changes the type** of `addedSubNodes`, `removedSubNodes`, `dlaiDif` from mutable to read-only collections; adds convenience constructor overload |
| `54d45df` | Jun 14, 2026 | `AttributeInfo` (parent struct, not `AttInfoDif`/`DifNodeIET` directly) | Ad-hoc `XmlAnyAttribute` support added to `AttributeInfo.IsAdHocAttribute` — does not add members to `AttInfoDif` or `DifNodeIET` |
| `094fcb1` | Jun 16, 2026 | `AttInfoDif`, `DifNodeIET` | Documentation rewrite and comparison-logic refactor in `CompareTrees.cs`; no new members added to either struct beyond what `6d66406` introduced |
| `1724852` | Jun 16, 2026 | `AttInfoDif`, `DifNodeIET` | Merge/stabilization; minor doc and logic touch-ups |

---

## Key Design Principles

1. **Immutability:** All structures use `readonly` fields and record structs for value semantics and thread-safety — a principle in place since 2023.
2. **Computed Properties:** `AttInfoDif` computes `addedInNew` and `removedInNew` using three-valued logic (`true`, `false`, `null`) — present before the June 2026 window.
3. **Immutable Collections:** `DifNodeIET`'s collection-typed members were converted from mutable `List<>`/`Dictionary<>` to `IReadOnlyList<>`/`IReadOnlyDictionary<>` in June 2026 (`6d66406`), with a convenience overload retained for callers still using mutable collections.
4. **No Object References:** Structs store only identifiers (`sGuid`, `ObjectID`) rather than references to `BaseType` or other SDC nodes (except in sub-node lists) — unchanged since 2023.
5. **Ad-Hoc Attribute Support:** Added in June 2026 to `AttributeInfo` (the parent struct), not directly to `AttInfoDif` or `DifNodeIET`.

---

## Related Information

- **Repository:** https://github.com/rmoldwin/SDC_ObjectModel
- **Branch:** Features/NET10/Net10Main
- **Baseline commit referenced:** `835454d` (Feb 28, 2023, `Fred_NugetTest` branch)
- **Related Files:**
  - `SDC.Schema/Utility Classes/Attribute and PI Structs and Methods/AttributeInfo.cs`
  - `SDC.Schema/Utility Classes/Attribute and PI Structs and Methods/CompareTrees.cs`
  - `SDC.Schema/Utility Classes/Attribute and PI Structs and Methods/AttributeMethods.cs`
- **Documentation:** See `.github/copilot-instructions.md` and root `copilot-instructions.md` for style guidelines

---

**Document Corrected:** July 10, 2026
