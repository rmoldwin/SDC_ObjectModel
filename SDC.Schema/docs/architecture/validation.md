# Validation

> **Status:** Stub — content to be migrated from the public GitHub gist
> "SDC OM Validation: ad hoc + (de)serialization-time validation, QaEngine/SdcValidationReport
> bridge" in PR2.

This chapter will cover:

- Built-in Structured Data Capture (SDC) / College of American Pathologists (CAP) structural
  validation.
- Ad hoc validation and (de)serialization-time validation.
- The Quality Assurance (QA) engine and its bridge to `SdcValidationReport`.
- The soft-reject validation contract (see `BP-VAL-001` in
  [qa-best-practices.md](qa-best-practices.md)): the SDC Object Model (OM) must never store an
  invalid value — instead it keeps the prior value, records the offending value in
  `RejectedValues`, and raises an event, both in property setters and during deserialization.
