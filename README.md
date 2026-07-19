# SDC_ObjectModel

Serialize/Deserialize SDC XML templates from/to the SDC Object Model (OM). Edit SDC templates inside the SDC OM. Validate SDC XML. Compare SDC XML versions.

## Projects

- **SDC.Schema** — The core SDC Object Model (OM) class library and serializers.
- **SDC.Schema.Tests** — MSTest unit and integration tests for SDC.Schema.
- **Benchmarks** — BenchmarkDotNet performance benchmarks.

## Serializers

The SDC OM supports four serialization formats:

| Serializer | Format | Library |
|---|---|---|
| `SdcSerializer<T>` | XML | `System.Xml.Serialization.XmlSerializer` |
| `SdcSerializerJson<T>` | JSON | `Newtonsoft.Json` |
| `SdcSerializerBson<T>` | BSON | `Newtonsoft.Json` (BsonDataWriter/BsonDataReader) |
| `SdcSerializerMsgPack<T>` | MessagePack | `Newtonsoft.Msgpack` (MessagePackWriter/MessagePackReader) |

