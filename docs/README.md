# Documentation

The `mvp` documentation is organized by intent:

- [L1 requirements](specs/L1.md) define product outcomes.
- [L2 requirements](specs/L2.md) define testable behavior and delivery status.
- [Requirement traceability](requirements-traceability.md) names the automated evidence for implemented requirements.
- [Detailed designs](detailed-designs/) describe the implementation by vertical slice and include rendered architecture diagrams.
- [Maintainability audit](maintainability-audit.md) records the baseline findings and five-phase remediation plan.
- [Manifest schema](../skills/dotnet-angular-jwt-mvp/references/manifest-schema.md) documents every accepted YAML field.
- [Generated tree](../skills/dotnet-angular-jwt-mvp/references/forge-shape.md) inventories the full-stack output.
- [Release process](../RELEASE.md) describes how versions are cut and which packages are published.
- [Mvp.Core readme](../src/Mvp.Core/README.md) documents the published generation library.

The former `technology-guidance-and-practices.md` document was intentionally retired. Its normative product behavior now lives in the requirements, while implementation decisions live in the detailed designs.
