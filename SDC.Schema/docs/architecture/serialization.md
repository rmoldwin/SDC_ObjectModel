# Serialization

> **Status:** Stub — to be populated in PR2 (gist migration) and beyond.

This chapter will cover the SDC.Schema serializer architecture across all supported formats:

- **XML** — `SdcSerializer<T>`, built on `System.Xml.Serialization.XmlSerializer`, handling
  polymorphism via roughly 150 `[XmlInclude]` attributes on `BaseType`.
- **JSON** — `SdcSerializerJson<T>`, built on `Newtonsoft.Json`'s `JsonConvert`, using
  `TypeNameHandling.All` and `ConstructorHandling.AllowNonPublicDefaultConstructor` for
  polymorphic round-trips (diverging from stock generated JSON serializers, which use an empty
  `new JsonSerializerSettings()`).
- **BSON (Binary JSON)** — `SdcSerializerBson<T>`, using `Newtonsoft.Json.Bson`'s
  `BsonDataWriter`/`BsonDataReader` with a `Newtonsoft.Json.JsonSerializer` (settings align with JSON).
  BSON bytes are stored as Base64 strings.
- **MessagePack** — `SdcSerializerMsgPack<T>`, using `MsgPack.Cli`'s
  `MessagePackSerializer<T>.Pack`/`Unpack`, following the same generated-code template as
  JSON/BSON (`SaveToFile`, `LoadFromFile`, `Serialize`, `Deserialize`).
- Ad hoc/"any" attribute and namespace round-tripping across all of the above formats, including
  multiple mixed namespaces and inherited/default namespace usage (see
  [qa-best-practices.md](qa-best-practices.md) for the relevant numbered example and QA rules).

Source material to migrate here: the public GitHub gist "SDC OM Validation..." (serialization
portions) and relevant sections of `SDC.Schema.Tests/Documentation/BsonJsonSerializationBugReport.md`.
