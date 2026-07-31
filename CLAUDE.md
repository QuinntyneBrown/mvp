# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repository is

`mvp` is a .NET 9 global tool (`PackageId` `Mvp.Cli`, `ToolCommandName` `mvp`) that scaffolds full-stack solutions: a layered .NET backend, an Angular workspace, and Playwright E2E tests. A consumer describes the target solution in a YAML manifest; the tool writes a source tree.

Two things are easy to get wrong when starting here:

1. **Most generation logic is not in this repo.** The heavy lifting lives in the `QuinntyneBrown.CodeGenerator.DotNet` NuGet package (`CodeGenerator.Core.*`, `CodeGenerator.DotNet.*`). This repo is mostly a CLI shell, a manifest model, and thin services that map CLI input onto that package's factories. When a generated file looks wrong, check whether the template belongs to the package before hunting in `src/`.
2. **The repo contains generated output.** `out/` holds a previously scaffolded sample solution (`out/Acme/`) and is gitignored. Do not treat files under `out/` as source.

## Commands

```powershell
# Build just the CLI (fast; this is what usually matters)
dotnet build src/Mvp.Cli/Mvp.Cli.csproj

# Build/test the whole solution
dotnet build Mvp.sln -c Release
dotnet test Mvp.sln -c Release

# Run a single test / test class
dotnet test Mvp.sln --filter "FullyQualifiedName~ApiGeneratorServiceTests"
dotnet test Mvp.sln --filter "FullyQualifiedName~ApiGeneratorServiceTests.GenerateAsync_CallsArtifactGeneratorWithProject"

# Run the CLI without installing it
dotnet run --project src/Mvp.Cli -- new --help

# Pack + install the tool locally (the skill and docs assume this flow)
dotnet pack src/Mvp.Cli -c Release -o ./nupkgs
dotnet tool install -g --add-source ./nupkgs Mvp.Cli   # or: dotnet tool update -g Mvp.Cli --add-source ./nupkgs

# End-to-end smoke run against the sample manifest
mvp new dotnet-angular-jwt-mvp --config samples/sample-mvp.yaml --output ./out
```

**Known-broken baseline:** `dotnet test Mvp.sln` currently fails at *compile* time with four `CS0854` errors (Moq expression trees omitting the optional `CancellationToken` in `ApiGeneratorServiceTests`, `CoreGeneratorServiceTests`, `InfrastructureGeneratorServiceTests`). No tests run until those are fixed. `NewCommandTests.Create_HasEightSubcommands` is also stale — `NewCommand` registers nine. Expect both when you first run the suite; they are not something you broke.

There is no `global.json`, `.editorconfig`, `Directory.Build.props`, or CI workflow, so the SDK selected is whatever is highest on the machine even though projects target `net9.0`.

## Architecture

```
Program.cs                  composition root: Host + AddMvpServices + RootCommand
  └── Commands/NewCommand   the only top-level command; registers 9 subcommands
        ├── 8 incremental   solution, api, core, infrastructure, app,
        │                   api-library, components-library, domain-library
        └── dotnet-angular-jwt-mvp   the full-stack, manifest-driven path
```

**Commands** (`Commands/`) are static classes with a single `Create(IServiceProvider services) → Command`. They own `System.CommandLine` option wiring, resolve their generator from the provider inside the handler, and on failure log and call `Environment.Exit(1)`. This shape is duplicated across all nine commands deliberately-by-accretion, not by design — see the audit before consolidating.

**Services** (`Services/`) are `I…GeneratorService` / implementation pairs registered as singletons in `Extensions/ServiceCollectionExtensions.AddMvpServices`, which also calls `AddDotNetServices()` from the CodeGenerator package to bring in `IProjectFactory`, `ISolutionFactory`, `ISolutionService`, `IArtifactGenerator`, and `IJwtAuthenticatedMvpFactory`. Two distinct kinds live here:

- *Delegating* services (`ApiGeneratorService`, `CoreGeneratorService`, `InfrastructureGeneratorService`, `SolutionGeneratorService`, `DotNetAngularJwtAuthenticatedMvpGeneratorService`) — thin adapters over package factories. Naming convention is enforced here: projects are emitted as `{name}.Api`, `{name}.Core`, `{name}.App`, `{name}.{LibraryName}`.
- *Self-contained* services (`AppGeneratorService`, `AngularLibraryGeneratorService`) — they write files themselves, with every generated `package.json`, `angular.json`, `.ts`, `.html`, and `.scss` embedded as C# raw string literals. `AppGeneratorService` probes for the Angular CLI via `IProcessRunner` and falls back to a hand-written minimal workspace when `ng` is absent. Angular/TypeScript dependency versions are hardcoded in these literals — that is where to change them.

**Manifests** (`Manifests/`) — `MvpManifest` plus `MvpManifestEntity`/`Property`/`Page`/`Component`, deserialized by `YamlMvpManifestLoader` (YamlDotNet, camelCase naming, unmatched properties ignored). `DotNetAngularJwtAuthenticatedMvpGeneratorService` maps this model onto the package's `JwtAuthenticatedMvpOptions`; the manifest model exists purely as that boundary DTO. Only `name` is validated. CLI `--name` overrides the manifest's `name:`; output resolves `--output` → `manifest.Output` → cwd, and the solution lands in a `<Name>/` subfolder.

## Conventions

- **One type per file**, named after the type. This is enforced across `src/` and `tests/` — do not add a second type to an existing file.
- Constructor injection with `ILogger<T>`; log with structured message properties, never string interpolation.
- Nullable reference types and implicit usings are on in both projects.
- Tests are xUnit + Moq, mirroring the source folder layout (`Commands/`, `Services/`), each creating and deleting its own temp directory under `Path.GetTempPath()`.

## Docs worth reading before changing behavior

- `docs/specs/L1.md` / `docs/specs/L2.md` — the requirements baseline. L2 requirements carry a Status field and are the traceability target for acceptance tests. Several L2 items are marked Implemented that the code does not fully satisfy; the audit lists which.
- `docs/maintainability-audit.md` — current prioritized findings (path-traversal risk from unvalidated manifest names, `Environment.Exit` in handlers, unhonored cancellation tokens, destructive writes, templates-as-string-literals). Read this before any refactor; it also proposes a target feature-oriented layout.
- `docs/detailed-designs/<subsystem>/<feature>/README.md` — per-feature designs with rendered PlantUML.
- `skills/dotnet-angular-jwt-mvp/` — the packaged skill that drives this CLI, including `references/manifest-schema.md` (authoritative manifest schema) and `references/forge-shape.md` (file-level inventory of generated output).

`docs/technology-guidance-and-practices.md` is referenced by the `dotnet-angular-jwt-mvp` command's help text and by the skill, but is deleted in the working tree. Restore it or update both references together.
