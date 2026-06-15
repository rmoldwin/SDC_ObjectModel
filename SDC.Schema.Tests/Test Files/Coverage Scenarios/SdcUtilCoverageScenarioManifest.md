# SdcUtil Coverage Scenario Template Manifest

This folder contains copied (non-mutated) high-complexity XML templates selected to drive branch-heavy `SdcUtil` coverage tests.

## Selection Criteria

- Large node/attribute counts to maximize structural diversity.
- Existing real-world fixture lineage already used in this repository.
- Inclusion of both FormDesign and Package-oriented scenarios.

## Selected Templates

1. `BreastStagingTest2v5.SdcUtilCoverage.xml`
   - Source: `SDC.Schema.Tests/Test Files/BreastStagingTest2v5.xml`
   - Approx complexity: 2465 elements / 10692 attributes.
   - Intended focus: deep traversal, subtree refresh/reorder, reflective child discovery, metadata extraction.

2. `Breast.Invasive.Staging.359.SdcUtilCoverage.xml`
   - Source: `SDC.Schema.Tests/Test Files/Breast.Invasive.Staging.359_.CTP9_sdcFDF.xml`
   - Approx complexity: 2459 elements / 5513 attributes.
   - Intended focus: large oncology form branch paths across navigation/reflection/attribute scans.

3. `SampleSDCPackage.SdcUtilCoverage.xml`
   - Source: `SDC.Schema.Tests/Test Files/..Sample SDCPackage.xml`
   - Approx complexity: 552 elements / 1668 attributes.
   - Intended focus: package/retrieve-form topology and mixed object model paths.

4. `DemogCCOLungSurgery.SdcUtilCoverage.xml`
   - Source: `SDC.Schema.Tests/Test Files/Demog CCO Lung Surgery.xml`
   - Approx complexity: 1677 elements / 3720 attributes.
   - Intended focus: demog + surgery blend for alternate node compositions.

## Additional Candidate for Targeted AnyAttr Branches

- Existing file (not copied yet): `SDC.Schema.Tests/Test Files/AnyAttr Scenarios/AnyAttr_Add_Custom.xml`.
- Intended use: targeted `ReflectNodeXmlAttributes` ad-hoc attribute paths (`XmlAnyAttribute`).

## Notes

- Existing source templates were not modified.
- These files are intended as coverage-test inputs and may be augmented by additional synthetic templates later if specific uncovered branches require narrower shape control.
