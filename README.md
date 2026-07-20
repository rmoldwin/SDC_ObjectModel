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

## Documentation

- [`..docs/summary.md`](..docs/summary.md) — start here: the technical knowledge base index (architecture, roadmap, glossary, conventions).
- [`..docs/roadmap.md`](..docs/roadmap.md) — planned work, each item linked to a GitHub issue.
- [`sessions/README.md`](sessions/README.md) — AI session continuity/handoff document index.

