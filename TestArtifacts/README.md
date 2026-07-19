# TestArtifacts

This folder holds **run-specific diagnostic dumps** produced automatically by tests and by
the JSON serializer when something goes wrong during a test run. They exist purely for
**offline, local inspection** of the most recent failure on the developer's machine.

## Why these files are NOT committed to git

The files below are **gitignored on purpose** (see the `TestArtifacts/` rules in the repo root
`.gitignore`). Reasons:

- **They are run-specific and machine-specific.** Their contents change on almost every run
  (timestamps, host time-zone offsets, full local stack traces with absolute paths, GUIDs).
- **They caused recurring merge conflicts.** Because every branch regenerated them with
  slightly different content, merging branches repeatedly produced spurious conflicts in these
  files even though they carry no source value.
- **They have no value on GitHub.** They are transient debugging output, not test fixtures or
  expected-result baselines. The tests do not read them back in as inputs.

## The files

| File | Produced by | Written when |
|------|-------------|--------------|
| `SdcSerializerJson_DeserializeError.json` | `SdcSerializerJson.Deserialize` (SDC Serializers/SdcSerializerJson.cs) | A JSON deserialization throws — the failing input JSON is dumped here. |
| `SdcSerializerJson_DeserializeError.txt`  | `SdcSerializerJson.Deserialize` | A JSON deserialization throws — the full exception text is dumped here. |
| `JSON_RoundTrip_Diffs.json`               | `SdcSerializationTests` round-trip assertions (Functional/Serialization/SdcSerializationTests.cs) | A JSON OM round-trip produces unexpected node diffs — the diff detail is dumped here. |
| `MsgPack_RoundTrip_Diffs.json`            | `SdcSerializationTests` round-trip assertions | A MsgPack OM round-trip produces unexpected node diffs — the diff detail is dumped here. |

## How to use them

When a serialization or round-trip test fails, open the matching file here to inspect the exact
failing input / exception / node diff from your **last local run**. They are overwritten on each
new failure. If the folder is empty, the corresponding tests passed (no dump was needed).

> Only this `README.md` is tracked in git; the diagnostic dumps themselves are ignored.
