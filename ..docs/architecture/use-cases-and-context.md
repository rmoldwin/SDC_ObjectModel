# SDC Object Model (OM) — Use Cases and Context

> Migrated from the root-level `SDC_OM_UseCases_Context.md` (2026-07-20) as part of the docs
> restructuring, since its content is architecture material rather than a top-level repo file.
> Stale cross-references from the original have been corrected below.

## Overview

The SDC (Structured Data Capture) Object Model is a comprehensive framework for healthcare form
design, data capture, and exchange.

## Primary Use Cases

### 1. Data Entry Form (DEF) Design
- Interactive design and authoring of structured data capture forms
- Form templates for healthcare questionnaires, surveys, and clinical data collection
- Support for complex form logic, conditional display, and validation rules

### 2. Data Exchange Messages
- User responses stored in the OM and serialized to various formats:
  - **SDC XML** - Native SDC format
  - **HL7** - Health Level 7 messaging standards
  - **FHIR** - Fast Healthcare Interoperability Resources
- Enables interoperability between healthcare systems

### 3. Validation
- **Form validation** - Ensures SDC-based forms meet specification requirements
- **Data validation** - Validates incoming SDC-based data for completeness, correctness, and compliance
- Business rules and constraint checking

### 4. Automated DEF Generation
- Dynamic form generation based on templates or data models
- **ObservableCollection approach** - Real-time UI updates for WPF, Blazor, and other reactive frameworks
- Support for alternative generation patterns as needed
- Programmatic form construction and manipulation

## Architecture Considerations

### Tree Structure
- The SDC OM uses a hierarchical tree structure with:
  - **TopNode** - Root of each tree (implements `ITopNode`)
  - **ParentNode** - Upward pointer to immediate parent
  - **Child collections** - Downward pointers to descendants
- **Nested TopNodes** are supported - trees can be nested to any depth
- Each TopNode maintains dictionaries:
  - `Nodes` - All nodes in the tree by GUID
  - `IETnodes` - Subset of nodes with special IET (Item Entry Type) semantics

### Mutability
- The OM is **mutable** by design to support:
  - Interactive form editing
  - Dynamic form generation
  - Runtime data updates
- Mutation operations maintain tree integrity through:
  - Automatic parent/child relationship updates
  - Dictionary registration/unregistration
  - GUID-based node tracking

### Thread Safety
- The OM is **not thread-safe** in the current implementation
- Concurrent modifications require external synchronization
- See [thread-safety.md](thread-safety.md) for details (this chapter's original reference to
  `SDC.Schema.Tests/Documentation/ThreadSafetyAnalysis.md` is stale — that content was migrated
  here during the docs restructuring)

## Testing Strategy

### Regression Tests
- `SDC.Schema.Tests/OMTests/BaseTypeTests.cs` - Core mutator and navigation tests

### Functional Tests
- `SDC.Schema.Tests/Functional/_OMTreeStabilityTests.cs` - Complex tree manipulation scenarios
- Validates dictionary consistency, parent-child symmetry, and GUID uniqueness

### Thread Safety Tests
- `SDC.Schema.Tests/OMTests/BaseTypeThreadSafetyTests.cs` - Concurrent access scenarios
- Documents known race conditions for future mitigation

### Validation Helpers
- `SDC.Schema.Tests/Helpers/TreeValidationHelper.cs` - Shared validation utilities
- Dictionary-based validation (does not use reflection traversal)

## Key Design Principles

1. **GUID-based identity** - Every node has a unique GUID
2. **Single TopNode ownership** - Each node belongs to exactly one TopNode tree
3. **Bidirectional relationships** - Parent and child pointers are kept in sync
4. **Observable patterns** - Support for UI binding via ObservableCollections
5. **Serialization fidelity** - Round-trip XML serialization maintains structure

## Related Documentation

- Form design specifications: (external SDC standards documentation)
- HL7/FHIR mapping: (integration documentation)
- Validation rules: (business rules documentation)
- ObservableCollection usage: (UI framework integration examples)

---

**Note:** This document provides high-level context for developers working with the SDC OM codebase.
For detailed API documentation, see XML comments in the source code.
