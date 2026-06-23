# Kickstart Prompt — Date / Date-Part Soft-Reject Validation (new session)

Copy everything in the fenced block below as the first message of the new session.

---

```
Resume/begin work on adding soft-reject validation + exceptionally helpful error messages for ALL
XML date and date-part types in the SDC Object Model (rmoldwin/SDC_ObjectModel).

START IN PLAN MODE. Do NOT implement until I approve the plan. First read, in this order:
1. SDC.Schema.Tests/Documentation/DateTimeValidation_Plan.md   ← the authoritative plan for THIS work
2. SDC.Schema.Tests/Documentation/Session_Handoff_ValidationPlan.md   ← Open-Question I-1 (dateTimeStamp)
3. SDC.Schema.Tests/Documentation/NumericRange_XSD_vs_NET.md   ← the doc + soft-reject pattern to mirror
4. SDC.Schema/Utility Classes/SdcUtil.cs (ValidateAndRaise, RecordRejectedValue, RejectedValues store)
   and SDC.Schema/Utility Classes/SdcRejectedValue.cs   ← the issue #8 pipeline you will reuse

CONTEXT: Issue #8 (commit f7fd14a, merged to Features/NET10/Net10Main) established the soft-reject
contract for NUMERIC types: SdcUtil.ValidateAndRaise(value, ctx) returns bool (true=valid→assign,
false=invalid→skip); invalid values are NEVER stored, never throw, and are recorded out-of-band in
BaseType.RejectedValues with a message that includes the offending value. Events/ValidationCollector
are gated by SuppressValidation; recording is unconditional. A regen-safe post-processor
(SDC.Schema/SDC.Schema/SDC Schema Files/Apply_SoftReject_Setters.py) keeps generated setters in the
soft-reject shape. Full suite is green (537/537).

THIS WORK extends that same contract to the date/date-part types, with one added requirement: the
validation messages must be EXCEPTIONALLY helpful and must mirror the official XML Schema explanation —
quote the offending value, name the xs: type, give the canonical lexical form with a concrete valid
example, and pinpoint the exact violation (e.g. "month must be 01–12; you supplied '13'"). These types
are unfamiliar to users, so the messages must teach the correct form.

KEY FACTS ALREADY ESTABLISHED (see the plan doc for detail + line numbers):
- Backing types split into two shapes:
  * String-backed (the XSD lexical string IS the value): gYear, gYearMonth, gMonth, gMonthDay, gDay,
    duration, dayTimeDuration, yearMonthDuration. These currently have NO/weak regex validation — set
    val="garbage" stores garbage. They can ride the existing #8 setter pipeline via a regen-safe XSD
    [RegularExpression] facet.
  * DateTime-backed (System.DateTime val): date, dateTime, time, dateTimeStamp. A DateTime cannot hold
    an invalid XSD lexical value, so lexical validation must happen at the STRING boundary (the
    IDataHelpers.AddDataTypesDE parse path and a proposed SetLexicalValue entry point), routed through
    SdcUtil.RecordRejectedValue + the event hub. The separate string `timeZone` property needs its own
    offset-range validator (−14:00…+14:00).
- REGRESSION TO FIX FIRST (Open-Question I-1, now upgraded): dateTimeStamp_Stype.val and the four
  dateTimeStamp_DEtype facets are System.DateTime but carry [RegularExpression(".*(Z|±dd:dd)")]. Under
  #8 soft-reject, that regex runs against DateTime.ToString() and can NEVER match, so EVERY
  dateTimeStamp value is now rejected/dropped. Fix this first with a regression test.
- The authoritative XSD regexes ALREADY EXIST inside IDataHelpers.AddDataTypesDE (date/dateTime/
  dateTimeStamp/time/gYear/gYearMonth/gMonth/gMonthDay/gDay). Promote them to shared const fields
  (new XsdDateTimePatterns class) — do not re-derive. duration/dayTimeDuration/yearMonthDuration need
  full ISO-8601 patterns authored.

REGEN-SAFE STRATEGY (auto-generated date files in protected folders must NOT be edited without my
explicit approval — stop and list exact files if needed): DECIDED — use a custom (Type, memberName) →
ValidationAttribute[] rule registry consulted inside SdcUtil.ValidateAndRaise. Do NOT use [MetadataType]
buddy classes: Validator.TryValidateProperty does not honor buddy metadata in this project without an
AssociatedMetadataTypeTypeDescriptionProvider, so the registry is the robust, regen-safe choice. New
code lives in SDC Customized Classes/, Utility Classes/, Interfaces/ only.

DECIDED BEHAVIORS:
- date/time lexical strictness: HARD soft-reject. Setting an xs:date from a string with a stray time
  component (or an xs:time with a stray date component) is rejected with a helpful message — never
  silently truncated via .Date. Matches "never store invalid" and real-world UI date/time pickers.

DELIVERABLE FOR THIS PLAN PHASE: review the plan doc, validate my findings against the live code,
refine the task breakdown in Section 5, resolve the Section 8 open questions with me (especially the
buddy-vs-registry decision and date/time lexical strictness), and report the finalized plan back to me
(the creator session) BEFORE implementing.

GUARDRAILS: Branch Features/NET10/Net10_DateTimeValidation off Features/NET10/Net10Main. Test budgets:
unit <1s, functional <10s — abort/root-cause if exceeded, never loop. Tests need rationale comments. Cover legal
AND illegal values, timezone variants, and XML⇄OM⇄JSON/BSON/MsgPack round-trips incl. mixed/inherited
namespaces. Close the VS solution before any checkout/merge. Keep all .md docs in git. Use the issue #8
RejectedValues/event pipeline — do not invent a parallel mechanism.
```

---

## Notes for the operator (not part of the prompt)

- The full plan is committed at `SDC.Schema.Tests/Documentation/DateTimeValidation_Plan.md`.
- Base the new session off `Features/NET10/Net10Main` (which now contains issue #8), in a new branch
  named `Features/NET10/Net10_DateTimeValidation`.
- First implementation task once approved: **fix-datetimestamp-i1** (the regression), since valid
  `dateTimeStamp` values are currently being dropped.
