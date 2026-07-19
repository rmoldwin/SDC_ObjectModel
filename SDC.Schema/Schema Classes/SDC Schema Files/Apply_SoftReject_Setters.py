#!/usr/bin/env python3
"""
Apply_SoftReject_Setters.py  —  post-generation transform for issue #8 (soft-reject validation).

WHY THIS EXISTS
---------------
xsd2code++ is a commercial GUI tool; its setter template is internal to the tool and is NOT a file
in this repository. With <ValidatePropertyInSetter>true</ValidatePropertyInSetter> (see
xsd2code.config) it emits, for every facet-constrained property, a setter of the form:

    set
    {
        if ((_field.Equals(value) != true))
        {
            ValidationContext validatorPropContext = new ValidationContext(this, null, null);
            validatorPropContext.MemberName = "prop";
            Validator.ValidateProperty(value, validatorPropContext);   // throws on invalid
            _field = value;                                            // assigns unconditionally
            OnPropertyChanged("prop", value);
        }
        _shouldSerializeprop = true;                                   // optional (some props)
    }

Issue #8 requires SOFT-REJECT semantics instead: never store an invalid value, never throw, and
record the offending value out-of-band. This script rewrites every such generated setter to:

    set
    {
        if ((_field.Equals(value) != true))
        {
            ValidationContext validatorPropContext = new ValidationContext(this, null, null);
            validatorPropContext.MemberName = "prop";
            if (SdcUtil.ValidateAndRaise(value, validatorPropContext))   // true => valid => assign
            {
                _field = value;
                OnPropertyChanged("prop", value);
                _shouldSerializeprop = true;        // gate ShouldSerialize on acceptance (variant 1)
            }
        }
        else
        {
            _shouldSerializeprop = true;            // unchanged value keeps prior serialize semantics
        }
    }

(Variant 2 — properties with no _shouldSerialize flag — omits both _shouldSerialize lines and the
else branch.)

HOW TO USE (after regenerating with xsd2code++)
-----------------------------------------------
    python "Apply_SoftReject_Setters.py" [--root <folder>] [--check]

* --root   Folder to process recursively. Defaults to the two compiled generated-class folders
           (SDC Constructor Removed, SDC Unmodified Classes) under SDC.Schema/SDC.Schema.
* --check  Report what WOULD change without writing (exit code 1 if any file would change).

The script is IDEMPOTENT: it only matches the raw Validator.ValidateProperty form, so re-running it
(or running it on already-converted files) is a no-op. BaseType.sGuid is a deliberate hard-reject
identity invariant and lives outside these folders, so it is never touched.
"""
import argparse
import pathlib
import re
import sys

# Raw xsd2code++ setter (Validator.ValidateProperty form). The trailing _shouldSerialize line is
# optional (variant 1 has it, variant 2 does not).
RAW = re.compile(
    r'(?P<i1>[ \t]*)ValidationContext validatorPropContext = new ValidationContext\(this, null, null\);\r?\n'
    r'[ \t]*validatorPropContext\.MemberName = "(?P<prop>[^"]+)";\r?\n'
    r'[ \t]*Validator\.ValidateProperty\(value, validatorPropContext\);\r?\n'
    r'[ \t]*(?P<field>_[A-Za-z0-9_]+) = value;\r?\n'
    r'[ \t]*OnPropertyChanged\("(?P=prop)", value\);\r?\n'
    r'(?P<iA>[ \t]*)\}'
    r'(?:\r?\n(?P<iB>[ \t]*)(?P<ss>_shouldSerialize[A-Za-z0-9_]+) = true;)?'
)


# ---------------------------------------------------------------------------------------------------
# Date-validation extension (issue: Net10 DateTimeValidation).
#
# A handful of string-backed date/date-part types (gYear, gYearMonth, gMonth, gMonthDay, gDay,
# duration) have NO XSD facet, so xsd2code++ emits a PLAIN setter (no Validator.ValidateProperty call)
# that the RAW transform above never touches. Their lexical validity is enforced by the regen-safe
# rule registry (SdcValidationRuleRegistry), but that registry is only consulted from inside
# SdcUtil.ValidateAndRaise — which the plain setter never calls. This second, NARROWLY SCOPED
# transform wires those specific `val` setters into the soft-reject pipeline so the registry rule is
# honored on programmatic set and on XML deserialization. It runs ONLY on the explicit allow-list
# below (so unrelated string `val` setters elsewhere are left untouched), and is idempotent.
PLAIN_VAL_FILES = {
    "gYear_Stype.cs",
    "gYearMonth_Stype.cs",
    "gMonth_Stype.cs",
    "gMonthDay_Stype.cs",
    "gDay_Stype.cs",
    "duration_Stype.cs",
}

PLAIN_VAL = re.compile(
    r'(?P<guard>(?P<i>[ \t]*)if \(\(\(_val == null\)[ \t]*\r?\n'
    r'[ \t]*\|\| \(_val\.Equals\(value\) != true\)\)\)\r?\n'
    r'(?P<i2>[ \t]*)\{)\r?\n'
    r'(?P<i3>[ \t]*)_val = value;\r?\n'
    r'[ \t]*OnPropertyChanged\("val", value\);\r?\n'
    r'(?P<i4>[ \t]*)\}'
)


def transform_plain(text: str, nl: str) -> tuple[str, int]:
    def repl(m: re.Match) -> str:
        guard, i3, i4 = m["guard"], m["i3"], m["i4"]
        inner = i3 + "\t"
        return (
            f'{guard}{nl}'
            f'{i3}ValidationContext validatorPropContext = new ValidationContext(this, null, null);{nl}'
            f'{i3}validatorPropContext.MemberName = "val";{nl}'
            f'{i3}if (SdcUtil.ValidateAndRaise(value, validatorPropContext)){nl}'
            f'{i3}{{{nl}'
            f'{inner}_val = value;{nl}'
            f'{inner}OnPropertyChanged("val", value);{nl}'
            f'{i3}}}{nl}'
            f'{i4}}}'
        )

    return PLAIN_VAL.subn(repl, text)


def transform(text: str, nl: str) -> tuple[str, int]:
    def repl(m: re.Match) -> str:
        i1, prop, field, iA = m["i1"], m["prop"], m["field"], m["iA"]
        ss, iB = m["ss"], m["iB"]
        inner = i1 + "\t"
        head = (
            f'{i1}ValidationContext validatorPropContext = new ValidationContext(this, null, null);{nl}'
            f'{i1}validatorPropContext.MemberName = "{prop}";{nl}'
            f'{i1}if (SdcUtil.ValidateAndRaise(value, validatorPropContext)){nl}'
            f'{i1}{{{nl}'
            f'{inner}{field} = value;{nl}'
            f'{inner}OnPropertyChanged("{prop}", value);{nl}'
        )
        if ss is None:
            return head + f'{i1}}}{nl}{iA}}}'
        return (
            head
            + f'{inner}{ss} = true;{nl}'
            + f'{i1}}}{nl}'
            + f'{iA}}}{nl}'
            + f'{iB}else{nl}'
            + f'{iB}{{{nl}'
            + f'{iB}\t{ss} = true;{nl}'
            + f'{iB}}}'
        )

    return RAW.subn(repl, text)


def main() -> int:
    here = pathlib.Path(__file__).resolve()
    default_root = here.parents[1]  # SDC.Schema/SDC.Schema
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", default=str(default_root))
    ap.add_argument("--check", action="store_true")
    args = ap.parse_args()

    root = pathlib.Path(args.root)

    def in_target_folder(p: pathlib.Path) -> bool:
        return "SDC Constructor Removed" in str(p) or "SDC Unmodified Classes" in str(p)

    files = []
    for p in root.rglob("*.cs"):
        if not in_target_folder(p):
            continue
        if p.name in PLAIN_VAL_FILES:
            files.append(p)
            continue
        if "Validator.ValidateProperty(value, validatorPropContext)" in \
                p.read_text(encoding="utf-8-sig", errors="surrogatepass"):
            files.append(p)

    total = 0
    changed_files = 0
    for p in files:
        raw = p.read_bytes()
        text = raw.decode("utf-8-sig")
        bom = raw.startswith(b"\xef\xbb\xbf")
        nl = "\r\n" if "\r\n" in text else "\n"
        new, n = transform(text, nl)
        if p.name in PLAIN_VAL_FILES:
            new, np = transform_plain(new, nl)
            n += np
        if n:
            changed_files += 1
            total += n
            print(f"{n:2d}  {p.relative_to(root)}")
            if not args.check:
                out = ("\ufeff" + new) if bom else new
                p.write_bytes(out.encode("utf-8"))

    verb = "would change" if args.check else "transformed"
    print(f"TOTAL setters {verb}: {total} across {changed_files} file(s) "
          f"(scanned {len(files)} candidate file(s)).")
    if args.check and total:
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
