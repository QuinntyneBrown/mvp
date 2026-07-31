# Maintainability and Implementation Audit

**Project:** `mvp` CLI

**Audit date:** 2026-07-31

**Scope:** Repository organization, production implementation, tests, build configuration, and developer documentation in the current working tree.

## Executive summary

The codebase is small and approachable, with a clear executable/test split, dependency injection at the composition root, structured logging, nullable reference types, and interfaces around important generation dependencies. Those choices provide a reasonable base.

The current baseline is not ready to evolve safely, however. The solution test project does not compile, command failures terminate the process from inside handlers, manifest-derived names are not validated before becoming paths or tool arguments, cancellation is not reliable, and direct file writes can overwrite or leave partial output. Most generation behavior is also encoded as large C# string literals and is covered only by shallow existence tests. These issues make changes risky even though the repository is still small.

The recommended order is:

1. Restore a green solution build and CI gate.
2. Introduce one testable command execution and error-handling path.
3. Add preflight input/path validation and non-destructive output handling.
4. Make cancellation and child-process cleanup reliable.
5. Extract templates and add golden-output/build tests.
6. Reorganize by feature and add repository-wide engineering defaults.

## Remediation completion

**Completed:** 2026-07-31 for the `2.0.0` implementation.

All five roadmap phases are implemented in the current working tree:

| Phase | Delivered outcome |
| --- | --- |
| 1 — Green baseline | .NET 10 SDK pin, central package versions and lock files, analyzer/editor defaults, warnings-as-errors build, 50 passing tests, three-OS CI, pack and installed-tool smoke gates. |
| 2 — Safe command boundary | Stable System.CommandLine 2.0 API, one in-process command factory, aggregate manifest/name validation, path normalization, argument-list process invocation, concise default errors, diagnostic opt-in, stable exit codes, and invocation cancellation. |
| 3 — Safe output pipeline | Structured generation results, complete preflight validation, sibling staging, explicit conflicts, transactional `--force`, backup rollback, contained UTF-8/LF writes, and cancellation cleanup. |
| 4 — Template and test architecture | Tool-owned embedded .NET 10 and Angular 22 templates, offline defaults, explicit Angular CLI opt-in, unit/command/golden/cancellation/governance tests, generated backend build, generated frontend build, and package-install smoke coverage. |
| 5 — Documentation and governance | Expanded README and documentation index, reconciled L1/L2 baseline, automated requirement-to-test register, repaired skill/reference material, updated detailed designs, and 50 rendered PlantUML diagrams. |

The original roadmap expected the legacy CodeGenerator package to remain for
incremental .NET work. Restore verification exposed high and critical
vulnerabilities in that package's transitive graph, so v2 also moved incremental
templates into `mvp`. `dotnet list Mvp.sln package --vulnerable
--include-transitive` now reports no vulnerable NuGet packages.

The findings and line references below are retained as the historical baseline
that motivated the remediation. Current evidence is recorded in
[`requirements-traceability.md`](requirements-traceability.md) and CI.

## Audit evidence

The following checks were run from the repository root:

| Check | Result |
| --- | --- |
| `dotnet build src/Mvp.Cli/Mvp.Cli.csproj --configuration Release --no-restore -p:TreatWarningsAsErrors=true` | Passed with 0 warnings and 0 errors. |
| `dotnet test Mvp.sln --configuration Release --no-restore` | Failed during test compilation with four `CS0854` errors. No tests ran. |
| `mvp --help` and `mvp new --help` | Ran successfully; `new` currently exposes nine subcommands. |
| Missing manifest invocation | Exited with code 1, but default output included a stack trace and absolute local source paths. |
| Repository conventions | No `global.json`, `.editorconfig`, `Directory.Build.props`, or CI workflow is present. |
| Root documentation | `README.md` contains only `# mvp`. |

The installed machine selected .NET SDK `11.0.100-preview` because the repository does not pin an SDK, even though the projects target .NET 9. This did not break the production build, but demonstrates that contributor and CI behavior is environment-dependent.

## What is already working well

- `src/` and `tests/` are separated and project names are consistent.
- `Program.cs` is a small composition root rather than a container for business logic.
- `ServiceCollectionExtensions.AddMvpServices` centralizes dependency registration.
- The manifest loader, full-stack options mapping, external process runner, and generators have distinct types.
- Production logging uses structured message properties.
- File paths are generally constructed with `Path.Combine`, supporting cross-platform separators.
- Nullable reference types and implicit usings are enabled.
- Generator contracts already accept cancellation tokens, providing a migration path for reliable cancellation.
- Tests use isolated temporary directories rather than writing into the repository.

These strengths should be retained while addressing the findings below.

## Prioritized findings

### P0 — Restore a buildable, trustworthy test baseline

`dotnet test Mvp.sln` cannot compile because Moq expression trees omit optional arguments:

- `tests/Mvp.Cli.Tests/Services/ApiGeneratorServiceTests.cs:66`
- `tests/Mvp.Cli.Tests/Services/ApiGeneratorServiceTests.cs:87`
- `tests/Mvp.Cli.Tests/Services/CoreGeneratorServiceTests.cs:66`
- `tests/Mvp.Cli.Tests/Services/InfrastructureGeneratorServiceTests.cs:66`

In addition, `NewCommand.Create` registers nine subcommands (`src/Mvp.Cli/Commands/NewCommand.cs:11`), while `NewCommandTests.Create_HasEightSubcommands` expects eight (`tests/Mvp.Cli.Tests/Commands/NewCommandTests.cs:37`). Once compilation is repaired, that test should fail. The test suite also does not assert that `dotnet-angular-jwt-mvp` is present.

**Recommended changes**

- Supply every method parameter explicitly in Moq `Setup` and `Verify` expressions, including optional cancellation parameters.
- Update the command inventory test to cover all nine names. Prefer a single expected-name collection over a brittle count plus one test per name.
- Add a CI workflow that restores, builds with warnings as errors, tests, and packs the tool.
- Require the solution-level build/test job before merging.
- Do not publish coverage percentages until the suite compiles and meaningful behavior tests exist.

**Exit criteria:** A clean clone can run `dotnet test Mvp.sln -c Release` successfully on the pinned SDK.

### P0 — Validate all consumer input before creating directories or invoking tools

The loader validates only that the solution name is non-empty (`YamlMvpManifestLoader.cs:24`). The value is then used directly in `Path.Combine(outputDirectory, manifest.Name)` (`DotNetAngularJwtAuthenticatedMvpGeneratorService.cs:25`). A rooted name or a name containing `..` can resolve outside the requested output root. The same general risk applies to entity, page, component, route, and incremental-command names passed to downstream generators.

`AppGeneratorService` also constructs one argument string from the unvalidated name (`AppGeneratorService.cs:50`). `UseShellExecute = false` prevents shell expansion, but spaces or option-like text can still change how `ng` parses the invocation.

**Recommended changes**

- Add a preflight `ManifestValidator` that returns all validation failures in one result before any write occurs.
- Define and enforce rules for solution/entity/property/page/component identifiers, supported property types, routes, libraries, duplicates, and reserved names.
- Normalize the output root with `Path.GetFullPath`, resolve each planned path, and verify every path remains under that root using OS-appropriate path comparison.
- Reject rooted names, `.`/`..` segments, directory separators, control characters, and names that cannot be valid C#, Angular, npm, or file identifiers in their target context.
- Use `ProcessStartInfo.ArgumentList` instead of interpolating one arguments string.
- Keep YAML deserialization as an input DTO step; pass only a validated, normalized model into generation.
- Add table-driven tests for traversal, rooted paths, spaces, Unicode, option-like names, duplicates, invalid types, and reserved names.

**Exit criteria:** Invalid input produces no filesystem changes, and no manifest-derived path can escape the resolved output root.

### P0 — Replace `Environment.Exit` with a centralized command result path

Every leaf command catches broad exceptions and calls `Environment.Exit(1)`. This duplicates roughly the same handler in eight incremental commands and the full-stack command. It also makes handlers difficult to test, bypasses normal disposal/finally behavior, and can truncate buffered output.

The full-stack handler logs the exception object (`NewDotNetAngularJwtAuthenticatedMvpCommand.cs:64`). A missing manifest therefore prints a stack trace and absolute source paths at default verbosity, despite `docs/specs/L2.md` requiring user-facing errors without implementation details.

**Recommended changes**

- Return or set an invocation exit code instead of terminating the process inside a handler.
- Add one top-level exception policy that maps validation, conflict, external-tool, cancellation, and unexpected failures to stable exit codes and concise messages.
- Show stack traces only under an explicit diagnostic/verbose option.
- Let `Program` own host disposal and the final integer return value.
- Use typed exceptions or result types carrying an error code, operation, actionable message, and optional internal exception.
- Consolidate repeated name/output option creation and handler wiring in a small command factory or shared command base.

**Exit criteria:** Commands can be invoked in-process in tests, errors return stable exit codes, and default output contains no stack trace or machine-local path.

### P1 — Make cancellation and child-process lifetime reliable

Cancellation tokens appear in service contracts but are not consistently honored:

- Command handlers call generators without a bound invocation cancellation token.
- API, Core, Infrastructure, and parts of solution generation do not pass the token to downstream calls.
- `AngularLibraryGeneratorService` performs synchronous writes, ignores the token, and returns `Task.CompletedTask`.
- `AppGeneratorService.IsAngularCliAvailableAsync` catches every exception (`AppGeneratorService.cs:42`), including `OperationCanceledException`, then continues by generating the fallback project.
- `ProcessRunner.WaitForExitAsync` observes cancellation but does not explicitly terminate the child process tree.

**Recommended changes**

- Bind Ctrl+C/invocation cancellation once in the command layer and propagate the same token through every stage.
- Never swallow `OperationCanceledException`; rethrow it or use an exception filter that excludes cancellation.
- On cancellation, kill the child process with `entireProcessTree: true`, await termination, and report the partially written output location.
- Check cancellation between file writes or use cancellable async file APIs.
- If an operation is intentionally synchronous, expose it synchronously instead of returning an already-completed `Task`.
- Add deterministic tests for pre-cancelled tokens, cancellation during probing, and cleanup of a long-running child process.

**Exit criteria:** Ctrl+C stops generation promptly, no child process remains, and cancellation never falls through to the Angular fallback.

### P1 — Make generation non-destructive and failure-safe

`AppGeneratorService` and `AngularLibraryGeneratorService` call `Directory.CreateDirectory` and `File.WriteAllText` directly. Existing files are silently replaced and an exception can leave a tree that looks partially generated. The full-stack command also creates the resolved output directory before validation/generation (`NewDotNetAngularJwtAuthenticatedMvpCommand.cs:58`).

**Recommended changes**

- Build a generation plan before writing: target root, complete file list, conflicts, and required external actions.
- Fail if the solution target already exists unless an explicit overwrite policy is provided.
- Generate into a unique sibling staging directory and move it into place only after all stages succeed.
- If overwrite is supported, make it explicit and recoverable; never merge unknown existing files silently.
- Track files written so cleanup and error reporting are deterministic.
- Introduce a narrow output abstraction for containment checks, encoding/newline policy, conflict handling, and testability.

**Exit criteria:** A failed or cancelled run cannot be mistaken for success, and existing consumer work is unchanged without explicit consent.

### P1 — Separate templates from orchestration code

`AppGeneratorService.cs` is 295 lines and combines Angular CLI discovery, orchestration, file layout, dependency versions, JSON, TypeScript, HTML, SCSS, and documentation as raw C# strings. `AngularLibraryGeneratorService.cs` has the same mixture at a smaller scale. Template changes are consequently noisy, hard to syntax-check, and easy to break through escaping or formatting mistakes.

**Recommended changes**

- Move generated content into packaged template assets under a versioned `Templates/` tree.
- Add a small renderer that receives a validated generation model and renders templates with explicit variables.
- Package templates as embedded resources or tool content and add a pack/install smoke test to ensure they ship.
- Keep orchestrators responsible only for planning stages and delegating rendering/writes.
- Store framework/dependency versions in one template-version policy rather than scattering them through generated files.
- Add golden-tree tests that compare complete output, plus build tests for representative generated projects.

**Exit criteria:** Updating an Angular or .NET template does not require editing orchestration code, and generated syntax is verified automatically.

### P1 — Expand tests from collaboration checks to observable behavior

Most current tests verify that a dependency was called or that a file exists. There are no tests for `YamlMvpManifestLoader`, `DotNetAngularJwtAuthenticatedMvpGeneratorService`, `ProcessRunner`, command execution/exit codes, validation, overwrite behavior, output containment, or cancellation. File-generation tests rarely parse or compile the generated content.

There is also repeated temporary-directory setup/cleanup in nearly every test, and the requirements state that acceptance tests should declare requirement traceability while the checked-in tests do not.

**Recommended test layers**

1. **Unit:** validation/normalization, manifest mapping, naming, path containment, error mapping, and generation planning.
2. **Command integration:** invoke the command tree in-process with captured stdout/stderr and assert exit code plus filesystem effects.
3. **Golden output:** compare complete generated trees for empty and representative manifests.
4. **Consumer build:** build the generated backend; install/build or at least type-check the generated frontend in a controlled job.
5. **Packaging smoke:** pack, install to an isolated tool path, run help, generate a sample, and uninstall the isolated copy.
6. **Cross-platform:** run the core suite on Windows, Linux, and macOS because output paths and external process behavior are product requirements.

Create a reusable temporary-workspace fixture and assertion helpers so tests emphasize behavior rather than cleanup mechanics.

### P1 — Reconcile requirements and documentation with the implementation

The current working tree has `docs/technology-guidance-and-practices.md` deleted, but command help and the bundled skill still refer to it. The root `README.md` is effectively empty. The untracked `docs/specs/L2.md` marks several behaviors as implemented that the audited code does not fully satisfy:

- L2-015: output containment is not enforced.
- L2-061: default error output includes a stack trace for a missing manifest.
- L2-062: interactive cancellation is not bound and probing can swallow cancellation.
- L2-066: the full command creates missing ancestors, while the acceptance criteria say a missing parent is an error.
- Acceptance-test trace comments required by the specification are absent from the checked-in tests.

**Recommended changes**

- Decide whether the deleted guidance document will be restored or replaced, then update every CLI/skill reference atomically.
- Turn requirement status into evidence: a requirement should be `Implemented` only when a named automated test passes.
- Add a lightweight traceability check mapping implemented L2 identifiers to tests.
- Expand `README.md` with purpose, prerequisites, install/update commands, quick start, manifest example, command reference link, development commands, and generated-output safety notes.
- Keep product requirements, user documentation, and implementation changes in the same pull request when behavior changes.

### P2 — Establish repository-wide build and dependency policy

Project configuration is duplicated and important defaults are absent. The CLI references a beta `System.CommandLine` package, while other package versions are pinned independently in each project. There is no SDK pin, analyzer policy, formatting policy, lock file, or automated dependency update/security scan visible in the repository.

**Recommended changes**

- Add `global.json` targeting the supported .NET 9 SDK feature band with an intentional roll-forward policy.
- Add `Directory.Build.props` for nullable settings, warnings-as-errors in CI, deterministic builds, analyzer level, and shared metadata.
- Add `.editorconfig` and enforce formatting/analyzers in CI.
- Consider `Directory.Packages.props` and NuGet lock files for centralized, repeatable dependency management.
- Migrate from the beta command-line package to a supported stable API in a dedicated change with command integration tests.
- Add automated dependency update and vulnerability scanning.
- Complete NuGet tool metadata: description, repository URL, license, authors, symbols/readme, and package validation as appropriate.

### P2 — Use stronger domain types and consistent naming

The manifest is represented by mutable classes whose string fields double as paths, C# identifiers, TypeScript identifiers, routes, and enum-like selectors. `libraryName`/`Library` are magic strings, and list properties can still become `null` through deserialization despite non-null initializers. Incremental generators also repeat nearly identical service and interface shapes.

**Recommended changes**

- Deserialize into input DTOs, then create immutable normalized records for generation.
- Replace magic strings with enums or validated value objects at the domain boundary.
- Centralize naming transformations (`PascalCase`, kebab-case, npm package name, namespace) and use culture-invariant casing.
- Return a structured generation result containing the resolved root and generated artifacts instead of only logging success.
- Review the thin API/Core/Infrastructure wrappers after validation and planning are extracted. Keep separate feature-facing contracts only where they provide a meaningful seam; share the repetitive implementation internally.

## Recommended source organization

The repository does not need additional assemblies yet. A feature-oriented layout within `Mvp.Cli` would make behavior easier to find while preserving a simple deployment model:

```text
src/Mvp.Cli/
  Program.cs
  Bootstrap/
    ServiceCollectionExtensions.cs
    CliExceptionPolicy.cs
  Commands/
    NewCommand.cs
    SharedGenerationCommand.cs
  Features/
    FullStack/
      DotNetAngularJwtCommand.cs
      Generation/
        FullStackGenerator.cs
        FullStackGenerationPlan.cs
      Manifest/
        ManifestDto.cs
        ManifestLoader.cs
        ManifestValidator.cs
        ValidatedManifest.cs
      Templates/
        ...
    Incremental/
      DotNet/
        ...
      Angular/
        ...
  Infrastructure/
    Processes/
      ProcessRunner.cs
    Output/
      GenerationOutput.cs
tests/Mvp.Cli.Tests/
  Unit/
  CommandIntegration/
  Golden/
  Packaging/
```

This keeps command adapters, application orchestration, input modeling, template assets, and infrastructure concerns distinct. Avoid splitting these into many class-library projects until independent reuse or dependency enforcement justifies the overhead.

## Suggested implementation roadmap

### Phase 1 — Green baseline

- Fix the four Moq compile errors and stale subcommand expectation.
- Pin the .NET SDK.
- Add build/test/pack CI and basic repository conventions.
- Add command smoke tests for help, missing name, and missing manifest.

### Phase 2 — Safe command boundary

- Replace `Environment.Exit` with centralized exit-code/error handling.
- Add validated/normalized manifest and name types.
- Enforce path containment and use `ArgumentList` for child processes.
- Correct default diagnostics and bind cancellation.

### Phase 3 — Safe output pipeline

- Add generation planning, conflict detection, staging, and atomic completion.
- Add a structured generation result and output abstraction.
- Cover overwrite, partial failure, and cancellation behavior.

### Phase 4 — Template and test architecture

- Extract packaged templates from C#.
- Add golden trees, generated backend/frontend validation, and package-install smoke tests.
- Consolidate repeated incremental-command plumbing.

### Phase 5 — Documentation and requirements governance

- Repair broken documentation references and expand the README.
- Reconcile L2 statuses with automated evidence.
- Add requirement-to-test traceability and cross-platform gates.

## Definition of done for the remediation

- `dotnet build Mvp.sln -c Release` and `dotnet test Mvp.sln -c Release` pass from a clean clone on the pinned SDK.
- The packed tool installs in an isolated location and all help commands work.
- Invalid or malicious manifest values cause no writes and no external process invocation.
- Default failures return documented exit codes without stack traces or machine-local paths.
- Cancellation stops generation and terminates child processes.
- Existing output is never overwritten without explicit policy.
- Representative generated backends compile and generated frontends pass their chosen static/build checks.
- Windows, Linux, and macOS CI validate the supported cross-platform behavior.
- README and requirements accurately describe the behavior verified by tests.
