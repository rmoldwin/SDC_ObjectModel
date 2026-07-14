// Method-level parallelism was removed: SDC.Schema uses [ThreadStatic] ambient construction
// state (BaseType.LastTopNode) plus async-local deserialization flags (SdcUtil.IsDeserializing,
// SdcUtil.SuppressValidation) that are safe under sequential test execution but were observed
// to leak across concurrently-scheduled test methods on reused thread-pool threads, causing
// intermittent, non-reproducible-in-isolation failures (e.g. GetJson()/GetXml() throwing, or a
// rule seeing 0 findings instead of 1). The much larger SDC.Schema.Tests suite (696 tests) has
// never opted into assembly-level parallelism and has been stable; this brings QA.Tests in line
// with that proven-safe configuration. See guide/07-known-gaps-and-future-work.md for the
// tracked gap in SDC.Schema's ambient-state thread-safety guarantees.
