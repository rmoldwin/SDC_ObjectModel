# Bug Report: BSON and JSON Serializers Cannot Round-Trip the SDC Object Model

**Repository:** https://github.com/rmoldwin/SDC_ObjectModel  
**Packages affected:**
- `Newtonsoft.Json` v13.0.4 (via `SdcSerializerJson<T>` and `SdcSerializerBson<T>`)
- `Newtonsoft.Json.Bson` v1.0.3 (via `SdcSerializerBson<T>`)
- `Newtonsoft.Msgpack` v0.1.11 (NOT affected — works via XML tunnel; see §6)
- Code generator: `Xsd2Code++` v6.0.64.0 / v5.1.1.0 (auto-generated serializer stubs)

**Date discovered:** 2025  
**Severity:** High — BSON and JSON round-trips are silently broken; deserialized trees are
either corrupt or throw during construction, making these serialization formats unusable
for the SDC Object Model without the fixes described below.

---

## 1. Summary

`SdcSerializerBson<T>` and `SdcSerializerJson<T>` both fail to restore a valid SDC object
model tree when deserializing. The two formats share one root cause (missing
`TypeNameHandling`) and BSON adds a second independent root cause (missing
`ConstructorHandling`).

| Format | Failure point | Exception |
|--------|--------------|-----------|
| BSON   | Deserialization — constructor selection | `NullReferenceException`: "parentNode can only be null if this object implements ITopNode" |
| JSON   | Deserialization — abstract-type instantiation | `JsonSerializationException`: "Could not create an instance of type `SDC.Schema.IdentifiedExtensionType`. Type is an interface or abstract class and cannot be instantiated." |

---

## 2. Root Causes

### Bug-1 (BSON only) — Missing `ConstructorHandling.AllowNonPublicDefaultConstructor`

**Location:** `SdcSerializerBson<T>.SerializerBson` property (line 29–38,
`SDC Serializers/SdcSerializerBson.cs`)

```csharp
// CURRENT (broken)
private static JsonSerializer SerializerBson
{
	get
	{
		if ((_serializerBson == null))
			_serializerBson = new JsonSerializer();   // ← no ConstructorHandling set
		return _serializerBson;
	}
}
```

Every SDC class (e.g. `FormDesignType`, `SectionItemType`, `QuestionItemType`) has a
**public parameterized constructor** that requires a non-null `parentNode` argument, AND a
**protected/internal parameterless constructor** used only during deserialization.

Without `ConstructorHandling.AllowNonPublicDefaultConstructor`, Newtonsoft selects the
public constructor and passes `null` for the `parentNode` parameter. The constructor body
immediately throws:

```
NullReferenceException: parentNode can only be null if this object implements ITopNode.
```

The same `SerializerBson` instance is reused for both serialization and deserialization,
so the fix must be applied when constructing it.

**Fix (BSON):**
```csharp
_serializerBson = new JsonSerializer
{
	ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

---

### Bug-2 (BSON and JSON) — Missing `TypeNameHandling.All`

**Location:**
- `SdcSerializerBson<T>.SerializerBson` property (same as Bug-1)
- `SdcSerializerJson<T>.SerializeJson` and `DeserializeJson` (lines 35 and 77,
  `SDC Serializers/SdcSerializerJson.cs`)

The SDC object model uses deep polymorphic inheritance. For example,
`ChildItemsType.ChildItemsList` is declared as `List<IdentifiedExtensionType>` but at
runtime contains `SectionItemType`, `QuestionItemType`, `DisplayedType`, etc.
`ListType.Items` similarly holds mixed `ListItemType` / `DisplayedType` objects.

Without `TypeNameHandling.All`, Newtonsoft serializes these objects without writing
`"$type"` discriminators. During deserialization it tries to instantiate the declared
base type (`IdentifiedExtensionType`), which is abstract, and throws:

```
JsonSerializationException: Could not create an instance of type
SDC.Schema.IdentifiedExtensionType. Type is an interface or abstract class and cannot
be instantiated. Path 'Body.ChildItems.Items[0].title', line 1, position 3183.
```

Even where a base type is concrete, Newtonsoft would construct the wrong class, so the
resulting node would be silently missing subtype-specific fields and child collections.

Note: the JSON serializer already fixes Bug-1 by setting
`ConstructorHandling.AllowNonPublicDefaultConstructor` in `DeserializeJson`; however
without TypeNameHandling this fix is never effective on polymorphic list elements.

**Fix (JSON):**
```csharp
// SerializeJson
return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
{
	TypeNameHandling = TypeNameHandling.All
});

// DeserializeJson
var settings = new JsonSerializerSettings
{
	TypeNameHandling = TypeNameHandling.All,
	ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
return JsonConvert.DeserializeObject<T>(input, settings);
```

**Fix (BSON):**
```csharp
_serializerBson = new JsonSerializer
{
	TypeNameHandling    = TypeNameHandling.All,
	ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

---

### Bug-3 (BSON→JSON intermediate path) — Type-discriminator erasure

**Location:** Any code that converts BSON bytes to JSON via
`JToken.ReadFrom(new BsonDataReader(...))` before calling `JsonConvert.DeserializeObject`.

Because Bug-2 causes BSON to be serialized without `"$type"` fields, any intermediary
step that reads BSON into a `JToken`/`JObject` and then serializes that to JSON will also
produce JSON without type discriminators. The downstream `DeserializeObject` call then
hits the same abstract-type instantiation error. This path must be fixed by applying
`TypeNameHandling.All` at the BSON **serialization** step (Bug-2 fix), not only at
deserialization.

---

## 3. Reproduction Steps

### 3.1 Reproduce BSON failure

```csharp
// Requires: Newtonsoft.Json 13.x, Newtonsoft.Json.Bson 1.x
// Any SDC FormDesignType loaded from XML (Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml)

var fd = FormDesignType.DeserializeFromXmlPath(path);
BaseType.ResetLastTopNode();
string bson = TopNodeSerializer<FormDesignType>.GetBson(fd, refreshSdc: false);

BaseType.ResetLastTopNode();
// Throws: NullReferenceException "parentNode can only be null if this object implements ITopNode"
var fdRt = TopNodeSerializer<FormDesignType>.DeserializeFromBson(bson, refreshSdc: true);
```

### 3.2 Reproduce JSON failure

```csharp
var fd = FormDesignType.DeserializeFromXmlPath(path);
BaseType.ResetLastTopNode();
string json = TopNodeSerializer<FormDesignType>.GetJson(fd, refreshSdc: false);

BaseType.ResetLastTopNode();
// Throws: JsonSerializationException "Could not create an instance of type
//   SDC.Schema.IdentifiedExtensionType. Type is an interface or abstract class."
var fdRt = TopNodeSerializer<FormDesignType>.DeserializeFromJson(json, refreshSdc: true);
```

### 3.3 Automated test coverage

The failing tests are in `SDC.Schema.Tests\Functional\TreeOperations\MoveTests.cs`:

- `CloneSdcSubtreeBsonTest` — asserts full round-trip fidelity (node count + first section
  identity); currently **fails** with `NullReferenceException` (Bug-1).
- `CloneSdcSubtreeJsonTest` — asserts full round-trip fidelity; currently **fails** with
  `JsonSerializationException` (Bug-2).
- `CloneSdcSubtreeMpackTest` — **passes** (MsgPack tunnels through XML; not affected).

Run with:
```
dotnet test SDC.Schema.Tests/SDC.Schema.Tests.csproj \
  --filter "Name=CloneSdcSubtreeBsonTest|Name=CloneSdcSubtreeJsonTest|Name=CloneSdcSubtreeMpackTest"
```

---

## 4. Expected vs. Actual Behavior

| | Expected | Actual |
|---|---|---|
| `GetBson` + `DeserializeFromBson` | Returns `FormDesignType` with same node count and section identity as original | Throws `NullReferenceException` during deserialization |
| `GetJson` + `DeserializeFromJson` | Returns `FormDesignType` with same node count and section identity as original | Throws `JsonSerializationException` during deserialization |
| `GetMsgPack` + `DeserializeFromMsgPack` | Returns equivalent tree | ✅ Works (tunnels XML) |

---

## 5. Why MsgPack Is Unaffected

`SdcSerializerMsgPack<T>` avoids both root causes by delegating to the **XML serializer**
as a byte payload:

```csharp
// SerializeMsgPack — encodes XML as UTF-8 bytes
string xml = SdcSerializer<T>.Serialize(obj);
return Encoding.UTF8.GetBytes(xml);

// DeserializeMsgPack — decodes UTF-8 bytes back to XML, then deserializes
string xml = Encoding.UTF8.GetString(input);
return SdcSerializer<T>.Deserialize(xml);
```

`SdcSerializer<T>` uses `System.Xml.Serialization.XmlSerializer`, which:
1. Handles polymorphic types through `[XmlInclude]` attributes decorating `BaseType`
   (there are ~150 such attributes, covering the entire type hierarchy).
2. Uses its own constructor-resolution logic that is immune to the Newtonsoft issues.

The MsgPack format name is therefore misleading: no MessagePack encoding is actually
performed; the bytes are UTF-8 XML.

---

## 6. Recommended Fix Summary

### For `SdcSerializerBson<T>` (file: `SDC Serializers/SdcSerializerBson.cs`)

Replace the `SerializerBson` property initialization:

```csharp
// BEFORE
_serializerBson = new JsonSerializer();

// AFTER
_serializerBson = new JsonSerializer
{
	TypeNameHandling    = TypeNameHandling.All,
	ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

### For `SdcSerializerJson<T>` (file: `SDC Serializers/SdcSerializerJson.cs`)

Update `SerializeJson` and `DeserializeJson`:

```csharp
// BEFORE (SerializeJson)
return JsonConvert.SerializeObject(obj);

// AFTER
return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
{
	TypeNameHandling = TypeNameHandling.All
});

// BEFORE (DeserializeJson)
var settings = new JsonSerializerSettings
{
	ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
return JsonConvert.DeserializeObject<T>(input, settings);

// AFTER
var settings = new JsonSerializerSettings
{
	TypeNameHandling    = TypeNameHandling.All,
	ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};
return JsonConvert.DeserializeObject<T>(input, settings);
```

> **Security note on `TypeNameHandling.All`:** When accepting JSON/BSON from untrusted
> external sources, `TypeNameHandling.All` opens a known deserialization gadget-chain
> vulnerability. In those cases, supply a custom `SerializationBinder` that whitelists
> only types in the `SDC.Schema` assembly. For internal/trusted round-trips (the current
> usage), `TypeNameHandling.All` is safe.

---

## 7. Additional Notes for Xsd2Code++ Maintainers

The auto-generated serializer stubs (both `SdcSerializerBson<T>` and
`SdcSerializerJson<T>`) were generated by Xsd2Code++ without these settings. Future
generator versions should emit:

- `ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor` in all
  Newtonsoft-based deserializers.
- A commented-out block showing how to enable `TypeNameHandling.All` for polymorphic SDC
  schemas (with a security warning for external-input scenarios).

The Xsd2Code++ issue tracker is at: https://www.xsd2code.com

---

## 8. Additional Notes for Newtonsoft.Json Maintainers

The `BsonDataReader`/`BsonDataWriter` API (in `Newtonsoft.Json.Bson`) is a thin binary
wrapper over the JSON object model. BSON document format does not include a built-in
polymorphic type-discriminator field; Newtonsoft uses the `"$type"` JSON convention
written into the BSON stream. If `TypeNameHandling` is not set, no `"$type"` key is
written, and deserialization of abstract/interface-typed properties is impossible.

A warning or analyzer suggestion at the point of constructing `JsonSerializer` /
`JsonSerializerSettings` when serializing to BSON—if `TypeNameHandling` is not set and
the schema contains polymorphic types—would significantly improve developer experience.

The `Newtonsoft.Json.Bson` package repository is at:
https://github.com/JamesNK/Newtonsoft.Json.Bson

---

*Report generated by GitHub Copilot from automated test analysis in the SDC_ObjectModel
repository.*
