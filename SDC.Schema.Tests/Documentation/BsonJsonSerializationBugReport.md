# Bug Report / Fix Record: BSON and JSON Serializers — Round-Trip Fidelity

**Repository:** https://github.com/rmoldwin/SDC_ObjectModel  
**Packages affected:**
- `Newtonsoft.Json` v13.0.4 (via `SdcSerializerJson<T>` and `SdcSerializerBson<T>`)
- `Newtonsoft.Json.Bson` v1.0.3 (via `SdcSerializerBson<T>`)
- `MsgPack.Cli` (`Newtonsoft.Msgpack` v0.1.11 wrapper) — see §5
- Code generator: `Xsd2Code++` v6.0.64.0 / v5.1.1.0 (auto-generated serializer stubs)

**Date discovered:** 2025  
**Date fixed:** 2025  
**Severity at discovery:** High — BSON and JSON round-trips were silently broken.  
**Current status:** ✅ BSON and JSON fixes applied. MsgPack workaround reverted (see §5).

---

## Background: Why does the BSON serializer use `JsonSerializer`?

`Newtonsoft.Json.Bson` does **not** provide its own independent serializer class.
`BsonDataWriter` and `BsonDataReader` are subclasses of `JsonWriter` and `JsonReader`
respectively. They translate the Newtonsoft JSON object graph to/from the BSON binary
wire format, byte-for-byte. `JsonSerializer` is the serialization engine that drives both:

```csharp
// BSON serialize — output is binary BSON, not text JSON
SerializerBson.Serialize(new BsonDataWriter(memoryStream), obj);

// BSON deserialize
SerializerBson.Deserialize<T>(new BsonDataReader(memoryStream));
```

This is the correct and intended API pattern for `Newtonsoft.Json.Bson`. All `JsonSerializer`
settings (`TypeNameHandling`, `ConstructorHandling`, etc.) apply equally to the BSON path.

---

## 1. Summary

`SdcSerializerBson<T>` and `SdcSerializerJson<T>` both failed to restore a valid SDC object
model tree when deserializing. The two formats shared one root cause (missing
`TypeNameHandling`) and BSON had a second independent root cause (missing
`ConstructorHandling`). **Both have been fixed.**

| Format | Failure point | Exception | Status |
|--------|--------------|-----------|--------|
| BSON   | Deserialization — constructor selection | `NullReferenceException`: "parentNode can only be null if this object implements ITopNode" | ✅ Fixed |
| JSON   | Deserialization — abstract-type instantiation | `JsonSerializationException`: "Could not create an instance of type `SDC.Schema.IdentifiedExtensionType`..." | ✅ Fixed |

---

## 2. Root Causes and Fixes

### Bug-1 (BSON only) — Missing `ConstructorHandling.AllowNonPublicDefaultConstructor`

**Location:** `SdcSerializerBson<T>.SerializerBson` property,
`SDC Serializers/SdcSerializerBson.cs`

Every SDC class (e.g. `FormDesignType`, `SectionItemType`, `QuestionItemType`) has a
**public parameterized constructor** that requires a non-null `parentNode` argument, AND a
**protected/internal parameterless constructor** used only during deserialization.

Without `ConstructorHandling.AllowNonPublicDefaultConstructor`, Newtonsoft selected the
public constructor and passed `null` for `parentNode`. The constructor immediately threw:

```
NullReferenceException: parentNode can only be null if this object implements ITopNode.
```

**Fix applied** (`SdcSerializerBson.cs`):
```csharp
// BEFORE (broken)
_serializerBson = new JsonSerializer();

// AFTER (fixed)
_serializerBson = new JsonSerializer
{
    TypeNameHandling    = TypeNameHandling.All,
    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

---

### Bug-2 (BSON and JSON) — Missing `TypeNameHandling.All`

**Location:**
- `SdcSerializerBson<T>.SerializerBson` property (same as Bug-1)
- `SdcSerializerJson<T>.SerializeJson` and `DeserializeJson`,
  `SDC Serializers/SdcSerializerJson.cs`

The SDC object model uses deep polymorphic inheritance. For example,
`ChildItemsType.ChildItemsList` is declared as `List<IdentifiedExtensionType>` but at
runtime contains `SectionItemType`, `QuestionItemType`, `DisplayedType`, etc.

Without `TypeNameHandling.All`, Newtonsoft serialized these objects without writing
`"$type"` discriminators. During deserialization it tried to instantiate the declared
base type (`IdentifiedExtensionType`), which is abstract, and threw:

```
JsonSerializationException: Could not create an instance of type
SDC.Schema.IdentifiedExtensionType. Type is an interface or abstract class and cannot
be instantiated.
```

Even where a base type is concrete, Newtonsoft constructed the wrong class, so the
resulting node silently lost subtype-specific fields and child collections.

**Fix applied** (`SdcSerializerJson.cs`):
```csharp
// SerializeJson — BEFORE
return JsonConvert.SerializeObject(obj);

// SerializeJson — AFTER
return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All
});

// DeserializeJson — BEFORE
var settings = new JsonSerializerSettings
{
    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};

// DeserializeJson — AFTER
var settings = new JsonSerializerSettings
{
    TypeNameHandling    = TypeNameHandling.All,
    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

**Fix applied** (`SdcSerializerBson.cs`): combined with Bug-1 fix above.

> **Security note on `TypeNameHandling.All`:** Writing `"$type"` discriminators is safe
> for internal/trusted round-trips. When accepting JSON or BSON from **untrusted** external
> sources, `TypeNameHandling.All` opens a known deserialization gadget-chain vulnerability.
> In those cases, supply a custom `SerializationBinder` that whitelists only types in the
> `SDC.Schema` assembly.

---

### Bug-3 (BSON→JSON intermediate path) — Type-discriminator erasure

**Status:** Resolved as a side-effect of the Bug-2 fix.

Any code that converts BSON bytes to JSON via `JToken.ReadFrom(new BsonDataReader(...))`
before calling `JsonConvert.DeserializeObject` was also affected, because BSON was
serialized without `"$type"` fields. With `TypeNameHandling.All` now applied at
serialization time, `"$type"` discriminators are present in the BSON stream and survive
any such intermediate conversion.

---

## 3. Reproduction Steps (historical — bugs no longer reproduce)

### 3.1 Former BSON failure

```csharp
var fd = FormDesignType.DeserializeFromXmlPath(path);
BaseType.ResetLastTopNode();
string bson = TopNodeSerializer<FormDesignType>.GetBson(fd, refreshSdc: false);
BaseType.ResetLastTopNode();
// Previously threw: NullReferenceException "parentNode can only be null..."
var fdRt = TopNodeSerializer<FormDesignType>.DeserializeFromBson(bson, refreshSdc: true);
```

### 3.2 Former JSON failure

```csharp
var fd = FormDesignType.DeserializeFromXmlPath(path);
BaseType.ResetLastTopNode();
string json = TopNodeSerializer<FormDesignType>.GetJson(fd, refreshSdc: false);
BaseType.ResetLastTopNode();
// Previously threw: JsonSerializationException "Could not create an instance of type
//   SDC.Schema.IdentifiedExtensionType..."
var fdRt = TopNodeSerializer<FormDesignType>.DeserializeFromJson(json, refreshSdc: true);
```

### 3.3 Automated test coverage

The authoritative round-trip fidelity tests are now in two files:

**`SDC.Schema.Tests\Functional\Serialization\SdcSerializationTests.cs`** (CompareTrees fidelity):
- `XmlRoundTripFidelityTest` — ✅ passes
- `JsonRoundTripFidelityTest` — ✅ passes after Bug-2 fix
- `BsonRoundTripFidelityTest` — ✅ passes after Bug-1 + Bug-2 fix
- `MsgPackRoundTripFidelityTest` — ❓ may fail; see §5

**`SDC.Schema.Tests\Functional\TreeOperations\MoveTests.cs`** (node-count + section identity):
- `CloneSdcSubtreeBsonTest` — ✅ passes after fix
- `CloneSdcSubtreeJsonTest` — ✅ passes after fix
- `CloneSdcSubtreeMpackTest` — ❓ may fail; see §5

---

## 4. Expected vs. Actual Behavior (post-fix)

| | Expected | Actual (post-fix) |
|---|---|---|
| `GetBson` + `DeserializeFromBson` | Returns `FormDesignType` with zero CompareTrees differences | ✅ Works |
| `GetJson` + `DeserializeFromJson` | Returns `FormDesignType` with zero CompareTrees differences | ✅ Works |
| `GetMsgPack` + `DeserializeFromMsgPack` | Returns equivalent tree | ❓ See §5 |

---

## 5. MsgPack Status — Workaround Reverted

### Background: xsd2code++ generated MsgPack pattern

Per the official xsd2code.com documentation, the MsgPack serializer is generated by
setting `DefaultSerialiser = MessagePackSerializer` and uses the `msgpack-cli` NuGet
package (`MsgPack.Cli`). The generated pattern is `MessagePackSerializer<T>.Pack` to
serialize to a binary byte stream and `MessagePackSerializer<T>.Unpack` to deserialize,
wrapped in the same `SaveToFile`/`LoadFromFile` helpers as JSON and BSON. No special
settings (analogous to `TypeNameHandling` or `ConstructorHandling`) are documented for
MsgPack — the serializer relies on native CLR reflection.

### Workaround history

A prior version of `SdcSerializerMsgPack<T>` replaced the `Pack`/`Unpack` calls with
an XML-as-UTF8-bytes tunnel, so `CloneSdcSubtreeMpackTest` passed while `MsgPack.Cli`
was actually bypassed entirely:

```csharp
// FORMER WORKAROUND (now reverted) — not the correct xsd2code++ pattern
// SerializeMsgPack — XML bytes disguised as MsgPack
string xml = SdcSerializer<T>.Serialize(obj);
return Encoding.UTF8.GetBytes(xml);

// DeserializeMsgPack
string xml = Encoding.UTF8.GetString(input);
return SdcSerializer<T>.Deserialize(xml);
```

**This workaround has been reverted.** `SdcSerializerMsgPack<T>` now calls
`MessagePackSerializer<T>.Pack` and `Unpack`, which is the correct xsd2code++ generated
pattern.

### Known risk

`MsgPack.Cli` uses CLR reflection and does not understand `[XmlInclude]` or
`[JsonProperty]` attributes. SDC node properties that use `XmlElement`, `XmlAttribute`,
or deep polymorphic inheritance may not serialize correctly. If so,
`MsgPackRoundTripFidelityTest` and `CloneSdcSubtreeMpackTest` will fail — which is
**correct and expected behavior**. The failure must remain visible so the MsgPack
serializer can be properly fixed (e.g., with custom `IMessagePackSerializer` adapters)
or replaced with a library that natively supports the SDC schema.

---

## 6. Notes for Xsd2Code++ Maintainers

The auto-generated serializer stubs (both `SdcSerializerBson<T>` and
`SdcSerializerJson<T>`) were generated by Xsd2Code++ without these settings. Future
generator versions should emit:

- `ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor` in all
  Newtonsoft-based deserializers.
- A commented-out block showing how to enable `TypeNameHandling.All` for polymorphic SDC
  schemas (with a security warning for external-input scenarios).

The Xsd2Code++ issue tracker is at: https://www.xsd2code.com

---

## 7. Notes for Newtonsoft.Json Maintainers

The `BsonDataReader`/`BsonDataWriter` API (in `Newtonsoft.Json.Bson`) is a thin binary
wrapper over the JSON object model. BSON document format does not include a built-in
polymorphic type-discriminator field; Newtonsoft uses the `"$type"` JSON convention
written into the BSON stream. If `TypeNameHandling` is not set, no `"$type"` key is
written, and deserialization of abstract/interface-typed properties is impossible.

A warning or analyzer suggestion at the point of constructing `JsonSerializer` /
`JsonSerializerSettings` when serializing to BSON — if `TypeNameHandling` is not set and
the schema contains polymorphic types — would significantly improve developer experience.

The `Newtonsoft.Json.Bson` package repository is at:
https://github.com/JamesNK/Newtonsoft.Json.Bson

---

## 8. Xsd2Code++ Serializer Architecture

The four serializer classes were originally generated by Xsd2Code++ v5.1.1.0 and then
manually modified. Understanding the generator's intent is essential when diagnosing
serializer behavior. The canonical reference is **https://www.xsd2code.com**.

### BSON and JSON are identical generation steps

Per the official xsd2code.com documentation, producing JSON or BSON involves exactly the
same generation steps. The only difference is the `DefaultSerialiser` setting:
- `JSonSerializer` → JSON text output
- `BSonSerializer` → BSON binary output (same code structure, different writer)

To place serializer methods in a shared base class rather than per-class:
`GenericBaseClass → Enable = true`

### Stock generated code (before SDC OM modifications)

The **stock** generated BSON serializer creates a `JsonSerializer` with no settings:
```csharp
// Stock xsd2code++ generated BSON serializer (unmodified)
_serializer = new JsonSerializer();   // no TypeNameHandling, no ConstructorHandling
```

The **stock** generated JSON serializer uses empty `JsonSerializerSettings`:
```csharp
// Stock xsd2code++ generated JSON serialize (unmodified)
Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
return JsonConvert.SerializeObject(this, settings);
```

Both of these are correct for simple, flat, non-polymorphic schemas. They are
**insufficient for the SDC OM** because the SDC schema requires polymorphic
type discrimination and non-public constructor resolution (see §2).

### SDC OM modifications to the generated code

The SDC OM serializers diverge from stock generated code with two manually added settings
that are required by the SDC polymorphic type hierarchy:

| Setting | Serializer | Reason |
|---|---|---|
| `TypeNameHandling.All` | JSON + BSON | Writes `"$type"` discriminators so polymorphic list elements (e.g. `List<IdentifiedExtensionType>`) deserialize as the correct concrete type |
| `ConstructorHandling.AllowNonPublicDefaultConstructor` | JSON + BSON | Selects the protected/internal parameterless constructor instead of the public `parentNode`-requiring constructor |

### Pattern common to all four serializers

Each serializer follows the same Xsd2Code++ structural template:

1. **Single lazy-initialized static serializer instance:**
   ```csharp
   private static XxxSerializer _serializer;
   private static XxxSerializer Serializer { get { if (_serializer == null) _serializer = ...; return _serializer; } }
   ```
   Note: the BSON serializer caches a `JsonSerializer` instance; the JSON serializer
   creates `JsonSerializerSettings` inline on each `JsonConvert` call (no cached instance).

2. **Three overloads of Deserialize/DeserializeXxx:**
   - `T DeserializeXxx(input)` — core; throws on failure
   - `bool DeserializeXxx(input, out T obj, out Exception exception)` — try-pattern
   - `bool DeserializeXxx(input, out T obj)` — delegates, discards exception

3. **File I/O helpers** `SaveToFileXxx` / `LoadFromFileXxx` — wrap the core methods
   with `FileStream`/`StreamReader`.

4. **`BaseType.ResetLastTopNode()` around deserialization** — required by the SDC OM to
   clear the static `LastTopNode` reference before and after constructing a new tree.

### Per-serializer specifics

| Serializer | Library | Key type | Polymorphism mechanism |
|---|---|---|---|
| `SdcSerializer<T>` | `System.Xml.Serialization.XmlSerializer` | `XmlSerializer` | ~150 `[XmlInclude]` attrs on `BaseType` |
| `SdcSerializerJson<T>` | `Newtonsoft.Json` | `JsonConvert` (settings per call) | `TypeNameHandling.All` writes `"$type"` |
| `SdcSerializerBson<T>` | `Newtonsoft.Json` + `Newtonsoft.Json.Bson` | `JsonSerializer` + `BsonDataWriter`/`BsonDataReader` | same `TypeNameHandling.All` via shared `SerializerBson` instance |
| `SdcSerializerMsgPack<T>` | `MsgPack.Cli` (`MsgPack.Serialization`) | `MessagePackSerializer<T>.Pack`/`Unpack` | native CLR reflection — no `[XmlInclude]`/`[JsonProperty]` awareness; polymorphic SDC types may not round-trip (see §5) |

### `SdcSerializerBson<T>` — why `JsonSerializer` and not a dedicated BSON class

`Newtonsoft.Json.Bson` provides no independent serializer. `BsonDataWriter` and
`BsonDataReader` subclass `JsonWriter` and `JsonReader` respectively — they are a
binary wire-format translation layer. `JsonSerializer` drives them exactly as it drives
text JSON; all settings (including `TypeNameHandling` and `ConstructorHandling`) apply
identically. The xsd2code++ generator chose this pattern because it is the only API
the `Newtonsoft.Json.Bson` package provides.

### BSON wire format and storage

BSON is a binary format (encodes type and length information for fast parsing) stored
as a **Base64 string** by `SdcSerializerBson<T>`. `SerializeBson` calls
`Convert.ToBase64String(memoryStream.ToArray())` and `DeserializeBson` calls
`Convert.FromBase64String(input)`. This matches the xsd2code++ pattern for binary
payloads saved to text files or string fields.

### Available advanced JSON settings (xsd2code++ configurables)

The following `Newtonsoft.Json.JsonSerializerSettings` properties are exposed as
xsd2code++ generator options and may be relevant for future SDC OM tuning:

| Setting | Options |
|---|---|
| `DateFormatHandling` | `IsoDateFormat` (default, e.g. `"2012-03-21T05:40Z"`) / `MicrosoftDateFormat` |
| `DateFormatString` | Custom format string, e.g. `"dd MM, yyyy"` |
| `DateParseHandling` | Controls how date strings are parsed during deserialization |
| `DateTimeZoneHandling` | Controls time zone behavior |
| `DefaultValueHandling` | `Include` (default) / `Ignore` — omit properties matching `[DefaultValue]` to reduce output size |
| `FloatFormatHandling` | Controls floating-point serialization |
| `FloatParseHandling` | Controls floating-point deserialization |
| `MissingMemberHandling` | `Ignore` (default) / `Error` — whether to throw when JSON contains unknown properties |
| `NullValueHandling` | `Include` (default) / `Ignore` — skip null properties to reduce output size |
| `StringEscapeHandling` | Controls string escaping |

`DefaultValueHandling.Ignore` and `NullValueHandling.Ignore` are particularly relevant
for the SDC OM since many optional properties default to null or zero, and omitting them
would significantly reduce JSON/BSON payload size.

---

*Originally generated by GitHub Copilot from automated test analysis in the SDC_ObjectModel
repository. Updated to reflect applied fixes, MsgPack workaround revert, and Xsd2Code++
architecture notes from official documentation (https://www.xsd2code.com) and actual
generated source.*

