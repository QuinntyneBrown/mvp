# Contributing to mvp

Thank you for helping improve `mvp`. Contributions may include code, tests, documentation, manifest examples, issue triage, accessibility feedback, or design proposals.

By participating, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md). Use [SUPPORT.md](SUPPORT.md) for help and [SECURITY.md](SECURITY.md) for vulnerabilities.

## Before you begin

- Search existing [issues](https://github.com/QuinntyneBrown/mvp/issues) and [pull requests](https://github.com/QuinntyneBrown/mvp/pulls) before starting work.
- Open an issue before making a large behavioral change, altering the manifest format, changing generated architecture, or adding a dependency.
- Keep pull requests focused. Separate unrelated refactoring or formatting from functional changes.
- Never include credentials, signing keys, private manifests, personal data, or sensitive generated output in issues, tests, screenshots, commits, or logs.
- Check the [L1 requirements](docs/specs/L1.md), [L2 requirements](docs/specs/L2.md), and relevant [detailed design](docs/detailed-designs/) before changing an established contract.

## Development setup

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then clone and verify the repository:

```shell
git clone https://github.com/QuinntyneBrown/mvp.git
cd mvp
dotnet restore
dotnet build Mvp.sln --configuration Release
dotnet test Mvp.sln --configuration Release
dotnet run --project src/Mvp.Cli -- --help
```

Node.js is not required for the CLI build. Install a [Node.js version supported by Angular 22](https://angular.dev/reference/versions) when validating a generated Angular workspace.

## Development workflow

1. Create a branch from the latest `main`. Use a short prefix such as `feat/`, `fix/`, `docs/`, `test/`, or `chore/`.
2. Add or update tests for behavioral changes. Confirm a new test fails for the expected reason before implementing a fix when practical.
3. Make the smallest coherent change and preserve existing public behavior unless the change is intentional and documented.
4. Run the checks relevant to the files you changed.
5. Update public documentation, examples, detailed designs, and the `Unreleased` section of [CHANGELOG.md](CHANGELOG.md) when behavior changes.
6. Open a pull request using the repository template and respond to review feedback.

Do not commit directly to `main`. Maintainers merge approved changes after the required review and validation are complete.

## Engineering guidelines

### CLI and services

- Keep command definitions focused on argument binding, dependency resolution, user-facing diagnostics, and exit behavior.
- Put generation behavior behind services and interfaces registered through dependency injection.
- Preserve nullable reference type safety and use asynchronous APIs for I/O-bound operations.
- Accept and propagate `CancellationToken` through generation and process boundaries.
- Use `Path` APIs for filesystem paths; generation must remain portable across Windows, macOS, and Linux.
- Never log secret values or the contents of a consumer's private manifest.

### Generated output

- Generated projects must build from a clean tree without manual source edits.
- Output must be deterministic for identical inputs and tool versions.
- Do not silently overwrite consumer-owned files.
- Keep generated authentication, validation, cross-origin, and error-handling defaults restrictive.
- If a template changes, test both a minimal name-only generation and a representative manifest containing entities, pages, and components.

### Tests

- Use xUnit for tests. Prefer small in-memory fakes or real isolated collaborators over mock-heavy interaction tests.
- Give tests behavior-oriented names and keep each test focused on one observable outcome.
- Write generated output to an isolated temporary directory and remove it during cleanup.
- Assert important file contents as well as file existence when a contract depends on generated code.
- Add regression coverage for every bug fix.

## Validation

Run the smallest useful set during development and the complete suite before requesting review:

```shell
dotnet build Mvp.sln --configuration Release
dotnet test Mvp.sln --configuration Release
```

For changes to the authenticated generator, also generate the sample into an ignored output directory:

```shell
dotnet run --project src/Mvp.Cli -- \
  new dotnet-angular-jwt-mvp \
  --config samples/sample-mvp.yaml \
  --output ./out
```

Then build the generated backend. If your change affects the frontend, install its dependencies and run its build and relevant tests as well. Record the commands and results in the pull request.

Documentation-only and repository-metadata changes need only the checks appropriate to those files.

## Commits and pull requests

Write concise, imperative commit subjects. Conventional prefixes are encouraged, for example:

```text
feat(manifest): validate duplicate entity names
fix(generator): preserve routes containing hyphens
docs: clarify local tool installation
```

A pull request should:

- Explain the problem and the resulting user-visible behavior.
- Link related issues and requirements.
- Include validation evidence.
- Call out compatibility, security, generated-output, and migration implications.
- Include documentation and changelog updates where relevant.
- Avoid unrelated generated artifacts and formatting churn.

At least one maintainer approval is required. Maintainers may ask for additional review when a change affects security, public contracts, or architecture.

## Releases

Releases are cut by maintainers from a Git tag. Contributors do not change versions: do not edit `<Version>` in `Directory.Build.props` in a feature pull request — add your entry to the `Unreleased` section of [CHANGELOG.md](CHANGELOG.md) instead. See [RELEASE.md](RELEASE.md) for the procedure.

## Licensing

By submitting a contribution, you agree that it may be distributed under the terms of the project's [MIT License](LICENSE). You retain copyright in your contribution.

Merged contributors are recognized through the repository's [contributors graph](https://github.com/QuinntyneBrown/mvp/graphs/contributors) and may add themselves to [CONTRIBUTORS.md](CONTRIBUTORS.md).
