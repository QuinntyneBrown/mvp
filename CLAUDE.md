# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with this repository.

## What this repository is

This repository ships two artifacts: `QuinntyneBrown.Mvp.Core`, a NuGet library holding the generation engine, and `mvp` (`PackageId` `Mvp.Cli`), a thin .NET 10 global tool over it that scaffolds layered .NET backends, Angular 22 workspaces, and Playwright end-to-end tests. Only the library is published to NuGet.org. Generation uses templates embedded in `Mvp.Core` and requires no network access.

Generated output under `out/` is disposable consumer output and is gitignored. Do not treat it as repository source.

## Commands

```powershell
# Restore, format-check, build, and test the complete solution
dotnet restore Mvp.sln --locked-mode
dotnet format Mvp.sln --verify-no-changes --no-restore
dotnet build Mvp.sln -c Release --no-restore -p:TreatWarningsAsErrors=true
dotnet test Mvp.sln -c Release --no-build

# Run one test class
dotnet test Mvp.sln -c Release --filter "FullyQualifiedName~ManifestValidatorTests"

# Run the CLI without installing it
dotnet run --project src/Mvp.Cli -- --help

# Pack and install to an isolated tool path
dotnet pack src/Mvp.Cli -c Release -o ./nupkgs --no-build
dotnet tool install --tool-path ./tool --add-source ./nupkgs Mvp.Cli
./tool/mvp --help

# Generate the representative sample
dotnet run --project src/Mvp.Cli -- new dotnet-angular-jwt-mvp --config samples/sample-mvp.yaml --output ./out
```

The SDK is pinned by `global.json`. Repository-wide compiler, analyzer, formatting, package-version, and lock-file policies live in `Directory.Build.props`, `.editorconfig`, `Directory.Packages.props`, and each project's `packages.lock.json`. CI repeats the complete checks on Windows, Linux, and macOS and additionally builds representative generated backend and frontend applications.

## Architecture

Two projects. The dependency direction is one-way: `Mvp.Cli` → `Mvp.Core`.

```text
src/Mvp.Cli/                     the command-line front end (package Mvp.Cli, unpublished)
  ├── Program.cs
  ├── Bootstrap/                 AddMvpCli, exception policy, exit codes
  └── Commands/RootCommandFactory.cs

src/Mvp.Core/                    the generation engine (package QuinntyneBrown.Mvp.Core)
  ├── ServiceCollectionExtensions.cs   AddMvpCore — the library's DI entry point
  ├── Errors/                    generation exception vocabulary
  ├── Manifests/                 mutable YAML input DTOs
  ├── Features/
  │   ├── FullStack/Manifest/    YAML loading and aggregate validation
  │   ├── FullStack/Generation/  full-stack planning and template rendering
  │   ├── Generation/            shared contracts, naming, TemplateResources
  │   └── Incremental/           solution-part planning and rendering
  ├── Infrastructure/
  │   ├── Output/                contained writes and transactional publication
  │   └── Processes/             argument-safe, cancellable child processes
  └── Templates/                 embedded as resources; 92 FullStack + 20 Incremental
```

`RootCommandFactory` owns the public command surface and funnels failures through one exception policy. Handlers return stable exit codes and propagate the invocation cancellation token; they must not terminate the process directly.

**Template resource names derive from `Mvp.Core`'s `RootNamespace`**, which is pinned equal to `AssemblyName`. `TemplateResources` derives the lookup prefix from the assembly rather than repeating a literal, because a mismatch compiles cleanly and fails only at run time. `TemplateResourceTests` binds the two together — if you change either property, that test is what tells you.

`PackAsTool` bundles `Mvp.Core.dll` into the tool package rather than taking a NuGet dependency, so the tool stays self-contained and works offline.

Manifest YAML is deserialized into input DTOs, checked for unmatched fields, validated in aggregate, and converted to immutable validated models before generation. `--name` overrides YAML before required-name validation. Never use unvalidated consumer text as a path or child-process argument.

All generation goes through `ITransactionalGenerationOutput`. It renders to a unique sibling staging directory and atomically publishes on success. Existing targets are conflicts unless `--force` is explicit; replacement uses a sibling backup and rollback. Keep every emitted path relative, contained, UTF-8, and LF-normalized.

The default Angular application path uses embedded templates. `--use-angular-cli` is the only path that invokes a local `ng` executable, through `IProcessRunner` and `ProcessStartInfo.ArgumentList`.

## Conventions

- Keep public command wiring thin and put behavior in feature/infrastructure collaborators.
- Give every type its own file, named after it, and use constructor injection for collaborators.
- One `<Version>` in `Directory.Build.props` governs the whole repository; projects must not override it, and feature work must not bump it. `IsPackable` defaults to `false`; packable projects opt in.
- Preserve nullable safety, asynchronous I/O, and end-to-end `CancellationToken` propagation.
- Do not expose stack traces or machine-local paths unless `--diagnostic` is present.
- Add a `// Acceptance Test` traceability header to every test file and update `docs/requirements-traceability.md` for implemented L2 behavior.
- Use `TestWorkspace` for isolated filesystem tests. Assert observable output and file contents, not only collaborator calls.
- Keep framework and dependency versions centralized in package policy or template package files.
- Update requirements, public documentation, template references, detailed designs, and the changelog with behavior changes.

## Docs worth reading before changing behavior

- `docs/specs/L1.md` and `docs/specs/L2.md` — baselined product requirements and delivery status.
- `docs/requirements-traceability.md` — implemented L2 requirement-to-test evidence.
- `docs/maintainability-audit.md` — historical findings and completed five-phase remediation record.
- `docs/detailed-designs/<subsystem>/<feature>/README.md` — feature designs with rendered PlantUML.
- `RELEASE.md` — how a release is cut and what is published.
- `skills/dotnet-angular-jwt-mvp/references/manifest-schema.md` — authoritative manifest contract.
- `skills/dotnet-angular-jwt-mvp/references/forge-shape.md` — generated file inventory.
