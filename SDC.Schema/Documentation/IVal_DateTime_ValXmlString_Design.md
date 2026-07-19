# IVal.ValXmlString — Date/Time Family Design Decision

## Scope

This document covers the `IVal.ValXmlString` implementation strategy for the SDC "date/time" family
of `_Stype` classes: `date_Stype`, `dateTime_Stype`, `dateTimeStamp_Stype`, and `time_Stype`.
These are the four `IVal` types whose XSD lexical representation includes an **optional timezone
suffix** (`Z` or `±hh:mm`), and whose generated `val` property is `System.DateTime` (not
`DateTimeOffset`), with a separate generated `string timeZone` property alongside it.

This decision only concerns non-generated code (the `Partial Classes` layer). No generated
xsd2code++ classes are modified by this design.

## Problem

XML Schema's `dateTime`/`date`/`time` lexical form is:

```
YYYY-MM-DDThh:mm:ss[.fff][Z|(+|-)hh:mm]
```

The timezone suffix is optional, and per the W3C XML Schema spec, its absence means the value is
explicitly **timezone-unspecified** — this is a distinct third state, not equivalent to UTC and not
equivalent to "the local machine's current timezone."

`ValXmlString` must convert between this lexical string and the class's `val` (`DateTime`) +
`timeZone` (`string`) pair, in both directions, with **perfect round-trip fidelity**: parsing a
lexical string and then re-serializing it must reproduce the original string exactly, for all three
cases (`Z`, an explicit numeric offset, or no suffix at all).

## Options considered

### Option A — Keep `val: DateTime` + `timeZone: string` (chosen)

Treat `timeZone` as the raw XSD suffix text, stored verbatim (`"Z"`, `"-06:00"`, or `null`/empty for
"unspecified"). `val` stores only the literal wall-clock digits, with no offset arithmetic ever
applied to it.

```csharp
private static readonly Regex TzSuffix = new(@"(Z|[+-]\d{2}:\d{2})$", RegexOptions.Compiled);

public string ValXmlString
{
    get => XmlConvert.ToString(val, "yyyy-MM-ddTHH:mm:ss.FFFFFFF") + (timeZone ?? "");
    set
    {
        var m = TzSuffix.Match(value);
        timeZone = m.Success ? m.Value : null;
        var dtPart = m.Success ? value[..^m.Length] : value;
        val = XmlConvert.ToDateTime(dtPart, XmlDateTimeSerializationMode.Unspecified);
    }
}
```

- `"...T12:00:00Z"` → `val` = literal digits, `timeZone = "Z"` → getter reproduces the identical string.
- `"...T12:00:00-06:00"` → `timeZone = "-06:00"` preserved exactly as written; never converted to the
  local machine's timezone, never recomputed as an offset.
- `"...T12:00:00"` (no suffix) → `timeZone = null`; round-trips back to the same bare string.
- Every case is lossless because no step ever performs offset math or Kind conversion — `val` is
  parsed/formatted as plain wall-clock components, and `timeZone` is copied as literal text.
- No generated files are touched; this lives entirely in `Partial Classes`.

### Option B — Change `val` to `DateTimeOffset` in the generated classes (rejected)

```csharp
public string ValXmlString
{
    get => XmlConvert.ToString(val);                 // always emits an offset
    set => val = XmlConvert.ToDateTimeOffset(value);  // throws, or assumes local machine offset, if no offset is present in the string
}
```

Rejected because:
1. It requires editing ~10 auto-generated `*_Stype.cs` files across `SDC Unmodified Classes`,
   `SDC Constructor Removed`, and `SDC Schema Files` — folders that require explicit user approval
   before any change, per project policy.
2. It does **not** actually solve the "timezone unspecified" case. `DateTimeOffset` always carries an
   offset; `XmlConvert.ToDateTimeOffset` on an offset-less string falls back to the local machine's
   current offset, which reintroduces the exact machine-dependent ambiguity this design is meant to
   avoid, and does not guarantee the original offset-less string is reproduced on re-serialization.
3. No round-trip fidelity gain over Option A, for materially higher risk and broader generated-code
   churn.

## Decision

**Option A.** `val` remains `System.DateTime` (never touched by offset math; effectively
`Kind = Unspecified` wall-clock digits), and `timeZone` remains the generated `string` property,
populated with the literal XSD timezone suffix text (or `null`/empty when absent). `ValXmlString`
performs only string splitting/joining plus `XmlConvert` formatting of the date/time digits — never
`DateTimeOffset`, never local-machine timezone conversion.

## Behavior summary

| Incoming `ValXmlString` (set)     | `val`                         | `timeZone` | `ValXmlString` (get) reproduces |
|------------------------------------|-------------------------------|------------|----------------------------------|
| `2024-07-07T12:00:00Z`             | `2024-07-07 12:00:00` (naked) | `"Z"`      | `2024-07-07T12:00:00Z`           |
| `2024-07-07T12:00:00-06:00`        | `2024-07-07 12:00:00` (naked) | `"-06:00"` | `2024-07-07T12:00:00-06:00`      |
| `2024-07-07T12:00:00` (no suffix)  | `2024-07-07 12:00:00` (naked) | `null`     | `2024-07-07T12:00:00`            |

This design applies identically to `date_Stype`, `dateTime_Stype`, `dateTimeStamp_Stype`, and
`time_Stype` (adjusting only the `XmlConvert` format string per type's precision, e.g. `date_Stype`
omits the time-of-day component).

## Testing implications

Round-trip tests for this family must assert **exact string equality** after a set/get cycle for all
three timezone-suffix cases (`Z`, explicit offset, none), not just that the parsed `DateTime` values
are equal — the `timeZone` string must also be asserted to confirm no offset conversion occurred.
