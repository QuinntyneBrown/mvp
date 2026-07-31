# Requirement-to-test traceability

This register names the automated evidence for every L2 requirement marked `Implemented` in the v2 baseline. The repository test `RequirementsTraceabilityTests.AllImplementedRequirementsHaveNamedEvidence` fails when an implemented requirement is absent from this register. Acceptance-test source files also carry the required `Traces to:` header comments.

| L2 requirements | Automated evidence |
| --- | --- |
| L2-001, L2-002, L2-003, L2-004 | `CommandIntegrationTests` and the cross-platform `build-test` CI job |
| L2-005, L2-006 | `security-package-consumers / Install packed tool and smoke commands` |
| L2-007, L2-008, L2-009, L2-010, L2-011, L2-012, L2-013, L2-014, L2-015 | `ManifestValidatorTests`, `YamlManifestLoaderTests`, and `FullStackGenerationTests` |
| L2-016, L2-017, L2-018, L2-019, L2-020, L2-021 | `FullStackGenerationTests` and `security-package-consumers` generated consumer builds |
| L2-022, L2-023, L2-024, L2-025, L2-026 | `IncrementalGenerationTests` and `CommandIntegrationTests` |
| L2-027, L2-028, L2-029, L2-030, L2-031, L2-032 | `FullStackGenerationTests` verifies the packaged authentication vertical slice; `security-package-consumers` compiles both consumers; generated `auth.spec.ts` is the runtime acceptance test |
| L2-034, L2-035, L2-036, L2-037, L2-038, L2-039 | `FullStackGenerationTests` and `security-package-consumers` backend/frontend builds |
| L2-044, L2-045, L2-046, L2-047 | `FullStackGenerationTests`, generated Playwright `auth.spec.ts`, and `RequirementsTraceabilityTests` |
| L2-048, L2-050, L2-051, L2-052, L2-053 | `FullStackGenerationTests`, `ManifestValidatorTests`, `YamlManifestLoaderTests`, and `CommandIntegrationTests` |
| L2-054, L2-055, L2-056 | `FullStackGenerationTests` plus the generated backend build in `security-package-consumers` |
| L2-057, L2-058 | `FullStackGenerationTests` and `IncrementalGenerationTests` |
| L2-060, L2-061, L2-062 | `CommandIntegrationTests`, `ProcessRunnerTests`, and `TransactionalGenerationOutputTests` |
| L2-063, L2-064, L2-065 | `IncrementalGenerationTests`, `AngularCliOptInTests`, the three-OS `build-test` matrix, and `DocumentationTests` |
| L2-066, L2-067, L2-068 | `TransactionalGenerationOutputTests`, `CommandIntegrationTests`, and `FullStackGenerationTests.GenerateAsync_IsDeterministicForTheSameManifest` |
| L2-069, L2-070 | `DocumentationTests` and the generated README assertion in `FullStackGenerationTests` |

Requirements marked `Partial` or `Planned` remain in the baseline but are intentionally excluded from the implemented-evidence gate until their acceptance criteria are release-gated.
