# Changelog

Notable changes to `mvp` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- A public documentation site under `website/`, covering the complete command surface, the manifest contract, the generated file inventory, output-safety semantics, every error message and exit code, and the tool's stated limitations. It is hand-written static HTML with no build step and no external requests, and its command output, file trees, and file counts are captured from real runs rather than paraphrased.
- `website/check-links.sh`, which resolves every relative reference in the site and fails on a miss.
- A `website` workflow that runs the reference check and deploys the site to Azure Static Web Apps on changes under `website/**`, with per-pull-request preview environments. Deployment requires an `AZURE_STATIC_WEB_APPS_API_TOKEN` repository secret.
- Requirements `L1-018` and `L2-071` through `L2-077`, which baseline `QuinntyneBrown.Mvp.Core` as a supported product surface: package distribution, the `AddMvpCore` entry point, the supported public API, the typed error vocabulary, the embedded templates, host independence, and the release contract. `L2-073` is `Partial` because package validation is not yet baselined against a published version.
- A detailed design for embedding the generation engine, at `docs/detailed-designs/generation-engine/embed-the-generation-engine/`, with rendered C4, class, and sequence diagrams.

### Changed

- The L1 requirements now describe two artifacts rather than one, and define *generation engine*, *host*, and *library consumer*.
- Existing detailed designs name the assembly that owns each type, and the command-line container diagram shows the `Mvp.Cli` and `Mvp.Core` package boundary.
- Corrected the run-safety design: `ProcessRunner` returns captured child output in a `ProcessResult` for the host to present, rather than writing it to structured logs.

## [2.1.0] - 2026-07-31

### Added

- `QuinntyneBrown.Mvp.Core`, a NuGet library carrying the manifest validation, template rendering, and transactional generation engine that previously existed only inside the `mvp` tool.
- `AddMvpCore`, a single dependency-injection entry point for consumers of the library.
- `ManifestLoadResult.CreateEmpty` for callers that generate without a manifest file.
- A tag-triggered release workflow that reconciles the tag against the repository version, publishes the library to NuGet.org behind an approval gate, and creates a GitHub release from the changelog.
- Source Link and embedded untracked sources, so published symbols resolve to repository source.
- A documented release process in `RELEASE.md`.
- `TemplateResourceTests`, which binds the generators' resource prefix to the shipped template names so a mismatch fails a unit test instead of a generation run.

### Changed

- The `mvp` tool consumes `Mvp.Core`; the packaged tool remains self-contained and offline.
- One `<Version>` in `Directory.Build.props` now governs every package, replacing a version literal that was duplicated across the project file, workflow, and documentation.
- `IsPackable` defaults to `false`; packable projects opt in.
- Continuous integration packs the whole solution, uploads the packages as build artifacts, and no longer pins a hard-coded tool version.
- `AddMvpServices` is now `AddMvpCli`, composed from `AddMvpCore`.
- Every type in `Mvp.Core` lives in its own file, and the four generation exceptions moved from `Mvp.Cli.Bootstrap` to `Mvp.Core.Errors`.
- Requirement traceability now scans every project under `tests/` rather than a single named project.

## [2.0.0] - 2026-07-31

### Added

- Tool-owned, embedded templates for offline .NET 10 and Angular 22 scaffolding.
- Aggregate manifest validation, unknown-field warnings, and a one-mebibyte input limit.
- Transactional staging, explicit conflicts, recoverable `--force` replacement, and contained writes.
- Stable command exit codes, global diagnostic output, invocation cancellation, and child-process cleanup.
- Unit, command-integration, golden-output, performance, governance, generated-build, and package smoke gates.
- Cross-platform CI, central package management, lock files, analyzers, dependency automation, and MIT licensing.
- Baselined requirements, automated traceability, detailed designs, and rendered architecture diagrams.

### Changed

- Updated the supported platform baseline to .NET 10, Angular 22, and Node.js 24.15 or newer.
- Reorganized implementation code by feature and isolated command, generation, process, and output concerns.
- Made packaged templates the default for Angular application generation; Angular CLI use is explicit with `--use-angular-cli`.
- Moved incremental .NET generation in-process to remove the vulnerable legacy generator dependency.

### Deprecated

- Nothing yet.

### Removed

- Removed the `QuinntyneBrown.CodeGenerator.DotNet` runtime dependency and duplicated command/service wrappers.

### Fixed

- Prevented manifest-derived path traversal, implicit overwrites, partial publication, stack-trace disclosure, and swallowed cancellation.

### Security

- Added restrictive path, name, route, type, duplicate, and reserved-name validation before filesystem mutation.
- Verified the CLI dependency graph has no known NuGet vulnerabilities.

[Unreleased]: https://github.com/QuinntyneBrown/mvp/compare/v2.1.0...HEAD
[2.1.0]: https://github.com/QuinntyneBrown/mvp/releases/tag/v2.1.0
[2.0.0]: https://github.com/QuinntyneBrown/mvp/commit/080ca85
