# SDC Validation Scenarios — End-to-End Guide

This document describes every supported validation entry point in the SDC object model, with
code examples and guidance on when to use each one.  
See `SDC.Schema/Utility Classes/` for the underlying helpers.

---

## 1. Direct Assignment

The simplest entry point.  Assign a value to any `*_Stype` or `*_DEtype` property directly.
The generated setter calls `SdcUtil.ValidateAndRaise` internally.

```csharp
var fd  = new FormDesignType(null, "FD.Demo");
fd.AddBody();
fd.Body.AddChildQuestionResponse("Q1", out var deType, "Score", dt: ItemChoiceType.integer);
var node = (integer_DEtype)deType.DataTypeDE_Item!;

node.minInclusive = 0m;      // valid → stored
node.minInclusive = 1.5m;    // invalid (FractionDigitsAttribute(0)) → NOT stored, soft-rejected

// Check whether the last assignment was accepted:
bool rejected = node.RejectedValues.ContainsKey("minInclusive");

// Read the rejection detail:
if (node.RejectedValues.TryGetValue("minInclusive", out var rv))
    Console.WriteLine(rv.Message);   // "Value '1.5' is invalid for 'minInclusive': ..."
```

**Key rule** (soft-reject contract, issue #8): an invalid value is *never* written to the backing
field.  The prior value is retained silently.  The offending value is recorded out-of-band.

---

## 2. WouldBeValid — Pure Predicate

Use `WouldBeValid` to check whether a value *would* pass DataAnnotations validation before
actually assigning it.  This is ideal for:

- **UI preview**: show a real-time error while the user types, before committing.
- **Pre-check before a batch**: scan many candidate values without touching the object model.

```csharp
// Simple bool overload
if (node.WouldBeValid(d => d.minInclusive, candidate))
    node.minInclusive = candidate;

// Overload that also returns a human-readable message
if (!node.WouldBeValid(d => d.minInclusive, candidate, out string? msg))
    ShowError(msg);   // msg is null on success, non-null on failure
```

**Side-effect guarantee**: `WouldBeValid` has **zero** side effects.
- No `ValidationOccurred` event is fired.
- The rejection store (`GetRejectedValues`) is untouched.
- `ValidationCollector` is not written.

This means it is safe to call speculatively in tight loops, inside render methods, or any
context where side effects would be unexpected.

---

## 3. TryAssignValue

Use `TryAssignValue` when you want a *try-style* assignment that goes through the real setter
and returns whether it succeeded, without catching exceptions yourself.

```csharp
// Simple bool overload
bool ok = node.TryAssignValue(d => d.minInclusive, candidate);
if (!ok)
    Console.WriteLine("Assignment rejected.");

// Overload with rejection detail
if (!node.TryAssignValue(d => d.minInclusive, candidate, out SdcRejectedValue? rej))
    Console.WriteLine(rej!.Message);
```

**Versus direct assignment**:

| Aspect | Direct (`node.prop = v`) | `TryAssignValue` |
|--------|--------------------------|------------------|
| Goes through real setter | ✓ | ✓ |
| Fires `ValidationOccurred` on failure | ✓ | ✓ |
| Clears stale prior rejection first | ✗ | ✓ |
| Returns success/failure as `bool` | ✗ | ✓ |
| Returns `SdcRejectedValue` on failure | ✗ (check store manually) | ✓ |
| Type-mismatch propagates as exception | ✓ | ✗ (soft reject) |

Both entry points fully fire `CheckValAgainstConstraints` and `CheckConstraintCoherence` on the
`*_DEtype` constraint setters.

---

## 4. ValidationOccurred Event

Subscribe to observe validation failures as they happen.  The event is raised by
`SdcUtil.ValidateAndRaise` (used by generated setters) as well as by
`SdcUtil.ValidateLexicalAndRaise` (used by lexical-string date setters).

```csharp
SdcValidationEvents.ValidationOccurred += (sender, e) =>
{
    Console.WriteLine($"[{e.Severity}] Node={e.NodeID}, Prop={e.PropertyName}");
    Console.WriteLine($"  AttemptedValue={e.AttemptedValue}");
    Console.WriteLine($"  Message={e.Message}");
    foreach (var r in e.Results)
        Console.WriteLine($"  • {r.ErrorMessage}");
};
```

**When it fires**:
- Direct setter assignment with an invalid value.
- `TryAssignValue` with an invalid value.
- Coherence advisory (`CheckConstraintCoherence`) after an *accepted* constraint setter call when
  sibling constraints are contradictory.
- `SdcDataTypeBuilder.AddDataTypesDE` if it encounters a malformed lexical string.

**When it does NOT fire**:
- `WouldBeValid` (pure predicate, zero side effects).
- Any scenario where `SuppressValidation.Value == true`.
- Valid setter assignments.

---

## 5. GetRejectedValues

Query the out-of-band store to see what the last setter rejected on a node.

```csharp
// Node-instance convenience property (BaseType)
IReadOnlyDictionary<string, SdcRejectedValue> rejectedValues = node.RejectedValues;

// Or via the static helper
var map = SdcUtil.GetRejectedValues(node);

if (map.TryGetValue("minInclusive", out var rv))
{
    Console.WriteLine($"Property:  {rv.PropertyName}");
    Console.WriteLine($"Attempted: {rv.AttemptedValue}");
    Console.WriteLine($"Message:   {rv.Message}");
    Console.WriteLine($"When:      {rv.RejectedAt}");
    foreach (var r in rv.Results)
        Console.WriteLine($"  • {r.ErrorMessage}");
}
```

`SdcRejectedValue` structure:

| Property | Type | Description |
|----------|------|-------------|
| `PropertyName` | `string` | Name of the rejected property |
| `AttemptedValue` | `object?` | The value that was rejected |
| `Message` | `string` | Human-readable reason |
| `RejectedAt` | `DateTimeOffset` | Timestamp (last attempt wins) |
| `Results` | `IReadOnlyList<ValidationResult>` | Raw DataAnnotations results |

---

## 6. ValidationCollector — Batch Sweep

Attach a `SdcValidationReport` to `ValidationCollector` before performing a tree sweep.  All
validation issues encountered during the sweep are collected into the report.

```csharp
var report = new SdcValidationReport();
SdcUtil.ValidationCollector.Value = report;
try
{
    // Any setter calls made here that fail will append to 'report'.
    node.minInclusive = -999m;  // triggers issue entry
}
finally
{
    SdcUtil.ValidationCollector.Value = null;
}

foreach (var issue in report.Issues)
{
    Console.WriteLine($"{issue.Severity}: {issue.NodeType}.{issue.PropertyName} → {issue.Message}");
}
bool allValid = report.IsValid;  // true when no Error-severity issues
```

Use `SdcValidate.ValidateTree(topNode)` for a full declarative sweep that sets and clears the
collector automatically:

```csharp
var report = topNode.ValidateTree();
if (!report.IsValid)
    foreach (var issue in report.Issues)
        Console.Error.WriteLine(issue.Message);
```

---

## 7. Normal Deserialization (SuppressValidation = true)

When `TopNodeSerializer<T>.DeserializeFromXml(xml)` (or `DeserializeFromJson`, etc.) is called
normally, the serializer sets both `IsDeserializing = true` and `SuppressValidation = true` for
the duration of the call.

```
IsDeserializing   = true  — setters skip graph-building side effects
SuppressValidation = true — ValidationOccurred event and ValidationCollector are silenced
```

What is **still enforced**:
- The soft-reject contract: invalid values in the XML are **not stored**.
- The rejection store is **always updated** (never suppressed by `SuppressValidation`).

What is **silenced**:
- `ValidationOccurred` events.
- `ValidationCollector` writes.

So after a normal deserialize you can inspect `node.RejectedValues` to learn which values from
the XML could not be round-tripped.

---

## 8. Validating Deserialization

To get a structured report of all XML validation failures while deserializing, use the
`*Validating*` overloads and leave `SuppressValidation` at its default (`false`):

```csharp
var report = new SdcValidationReport();
SdcUtil.ValidationCollector.Value = report;
SdcUtil.SuppressValidation.Value  = false;  // ensure events/collector are active

FormDesignType fd;
try
{
    fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlValidating(xml);
}
finally
{
    SdcUtil.ValidationCollector.Value = null;
    SdcUtil.SuppressValidation.Value  = false;
}

if (!report.IsValid)
    Console.WriteLine($"{report.Issues.Count} validation issue(s) found during deserialization.");
```

The `*Validating` overloads set `IsDeserializing = true` (to avoid tree-building side effects) but
intentionally leave `SuppressValidation` at its default so setter failures populate the collector.

---

## 9. SuppressValidation vs IsDeserializing — Decoupled Concerns

These two flags are independent `AsyncLocal<bool>` values with distinct purposes:

| Flag | Controls |
|------|----------|
| `SuppressValidation` | Whether `ValidationOccurred` and `ValidationCollector` are active |
| `IsDeserializing` | Whether tree-building side effects (e.g. node registration) run |

They often move together during deserialization but serve different purposes and can be set
independently:

- **Non-validating deserialize**: both `true` — quiet and fast.
- **Validating deserialize**: `IsDeserializing = true`, `SuppressValidation = false` — tree is
  rebuilt cleanly but all validation failures are collected.
- **Normal interactive use**: both `false` — full validation, events, and coherence checks.
- **Silent batch processing**: `SuppressValidation = true`, `IsDeserializing = false` — events are
  silenced but rejections are still recorded and the tree stays consistent.

---

## 10. SdcValidationRuleRegistry — Overriding Annotations

`SdcValidationRuleRegistry` lets you register custom validation attributes for any `(type, member)`
pair without editing the auto-generated `*_Stype`/`*_DEtype` files.  When a member has a registry
entry, `ValidateAndRaise` (and `WouldBeValid`) validate against those attributes
**instead of** the DataAnnotations physically declared on the property.

**Common uses**:
- Neutralise an impossible generated `[RegularExpression]` (e.g. `dateTimeStamp`).
- Strengthen a weak generated regex.
- Add `[Required]` to a property that XSD marks as optional but your domain requires.

```csharp
// Register a custom Range rule for integer_DEtype.minInclusive
SdcValidationRuleRegistry.Register(
    typeof(integer_DEtype), "minInclusive",
    new RangeAttribute(0.0, 1000.0) { ErrorMessage = "Score must be between 0 and 1000" });

// Now WouldBeValid and the setter use [Range(0, 1000)] instead of the generated annotation.
bool ok = node.WouldBeValid(d => d.minInclusive, 2000m);  // false (> 1000)

// Register an empty array to neutralise all annotations for a member:
SdcValidationRuleRegistry.Register(typeof(dateTimeStamp_Stype), "val" /* no attributes */);

// Check whether a rule is registered:
bool isReg = SdcValidationRuleRegistry.IsRegistered(typeof(integer_DEtype), "minInclusive");

// Walk up the hierarchy: a rule on decimal_Stype also applies to decimal_DEtype.
SdcValidationRuleRegistry.Register(
    typeof(decimal_Stype), "val",
    new RangeAttribute(-100.0, 100.0));

// TryGet walks the inheritance chain:
SdcValidationRuleRegistry.TryGet(typeof(decimal_DEtype), "val", out var attrs);
// attrs contains the RangeAttribute registered for decimal_Stype.
```

Registrations are **global and permanent** for the lifetime of the process.  If you need a
scoped override (e.g. in unit tests), restore the prior state in cleanup by registering an empty
array, or by re-registering the original attributes.
