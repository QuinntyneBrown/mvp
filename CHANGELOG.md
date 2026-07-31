# Changelog

Notable changes to `mvp` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

No changes yet.

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

[Unreleased]: https://github.com/QuinntyneBrown/mvp/commits/main
[2.0.0]: https://github.com/QuinntyneBrown/mvp/releases/tag/v2.0.0
