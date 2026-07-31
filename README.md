# mvp

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg)](CONTRIBUTING.md)
[![Project status: active development](https://img.shields.io/badge/status-active%20development-orange.svg)](#project-status)

`mvp` is an open-source .NET command-line tool for scaffolding full-stack minimum viable products. It generates an opinionated, buildable starting point with a layered .NET backend, an Angular workspace, JWT authentication, and Playwright end-to-end test assets.

[Get started](#get-started) | [CLI reference](#cli-reference) | [Documentation](docs/README.md) | [Contributing](CONTRIBUTING.md) | [Support](SUPPORT.md)

## Why mvp?

Starting a product should mean working on its domain, not repeatedly assembling authentication, project boundaries, frontend libraries, and test infrastructure. `mvp` turns a small YAML manifest into a consistent application baseline that a team can own and extend.

The primary generator provides:

- A .NET 9 backend organized into Domain, Application, Infrastructure, and API projects.
- Registration and sign-in flows using JWT bearer authentication.
- An Angular 19 workspace with application, API, component, and domain boundaries.
- Entity, page, and reusable-component scaffolding driven by YAML.
- Playwright page objects and an authentication journey.
- In-memory persistence for local evaluation, with SQL Server support available through configuration.
- Focused commands for generating individual solution parts.

> [!IMPORTANT]
> This project is under active development. Review the [requirements status](docs/specs/L2.md#delivery-status-summary) and generated code before using it in production. Replace generated placeholder secrets and complete an application-specific security review before deployment.

## Get started

### Prerequisites

To build and run the CLI, install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

Node.js 20 or later and npm are required only when you want to install or run a generated Angular application. Playwright browser binaries are required only for generated end-to-end tests.

### Run from source

```shell
git clone https://github.com/QuinntyneBrown/mvp.git
cd mvp
dotnet restore
dotnet build Mvp.sln --configuration Release
dotnet run --project src/Mvp.Cli -- --help
```

Generate the included sample:

```shell
dotnet run --project src/Mvp.Cli -- \
  new dotnet-angular-jwt-mvp \
  --config samples/sample-mvp.yaml \
  --output ./out
```

In PowerShell, the same command can be entered on one line:

```powershell
dotnet run --project src/Mvp.Cli -- new dotnet-angular-jwt-mvp --config samples/sample-mvp.yaml --output ./out
```

The generated solution is written to `./out/Acme` because the sample manifest names the solution `Acme`.

### Install as a global tool from source

`Mvp.Cli` is not currently published to NuGet.org. You can package and install the current source locally:

```shell
dotnet pack src/Mvp.Cli --configuration Release --output ./nupkgs
dotnet tool install --global --add-source ./nupkgs Mvp.Cli
mvp --help
```

To reinstall a locally packaged version, uninstall the existing tool first or use `dotnet tool update` with the same package source.

## Create an authenticated full-stack MVP

For a minimal solution, provide a name and output directory:

```shell
mvp new dotnet-angular-jwt-mvp --name Contoso --output ./out
```

For a domain-aware solution, describe it in YAML:

```yaml
name: Contoso

entities:
  - name: Project
    properties:
      - name: Title
        type: string
      - name: BudgetCents
        type: long

pages:
  - name: Projects
    route: projects
    requiresAuth: true

components:
  - name: ProjectCard
    library: domain
```

Then generate it:

```shell
mvp new dotnet-angular-jwt-mvp --config mvp.yaml --output ./out
```

`--name` overrides the name in the manifest when both are supplied. See the [manifest reference](skills/dotnet-angular-jwt-mvp/references/manifest-schema.md) and [complete sample](samples/sample-mvp.yaml) for supported fields and types.

## After generation

Treat the generated tree as application source code, not a production-ready deployment:

1. Replace the placeholder `Jwt:SigningKey` in the generated API configuration with a secret of at least 32 random characters. Store real secrets outside source control.
2. Build the generated backend from its `backend` directory with `dotnet build`.
3. From `frontend`, run `npm install` and `npm start` to serve the Angular application.
4. If you need end-to-end tests, run `npx playwright install` once and then run the generated Playwright test command.
5. Configure durable persistence, allowed origins, observability, deployment, and environment-specific security controls before any non-evaluation use.

The default in-memory store is intentionally disposable; data does not survive a process restart.

## CLI reference

Run `mvp new <command> --help` for the authoritative options for a command.

| Command | Purpose |
| --- | --- |
| `mvp new dotnet-angular-jwt-mvp` | Generate the authenticated .NET and Angular vertical slice from a name or YAML manifest. |
| `mvp new solution` | Generate a .NET solution, Web API, and Angular application shell. |
| `mvp new api` | Generate a .NET Web API project. |
| `mvp new core` | Generate a .NET Core project. |
| `mvp new infrastructure` | Generate a .NET Infrastructure project. |
| `mvp new app` | Generate an Angular application, using Angular CLI when available. |
| `mvp new api-library` | Generate an Angular API library. |
| `mvp new components-library` | Generate an Angular components library. |
| `mvp new domain-library` | Generate an Angular domain library. |

All component commands accept `--name` (`-n`) and `--output` (`-o`). The authenticated full-stack command also accepts `--config` (`-c`).

## Repository layout

```text
src/Mvp.Cli/        CLI commands, manifest loading, and generation services
tests/Mvp.Cli.Tests Unit tests for commands and generators
samples/            Example manifests
skills/             Reusable guidance and generation references
docs/specs/         Baselined L1 and L2 product requirements
docs/detailed-designs/
                    Feature and subsystem design documentation
```

## Documentation

Start with the [documentation index](docs/README.md). Key resources include:

- [High-level requirements](docs/specs/L1.md)
- [Detailed requirements and delivery status](docs/specs/L2.md)
- [Detailed designs](docs/detailed-designs/)
- [Manifest schema](skills/dotnet-angular-jwt-mvp/references/manifest-schema.md)
- [Generated solution inventory](skills/dotnet-angular-jwt-mvp/references/forge-shape.md)
- [Maintainability audit](docs/maintainability-audit.md)

## Project status

`mvp` is in active development and does not yet have a tagged public release. Capabilities are tracked as implemented, partial, or planned in the [L2 requirements](docs/specs/L2.md). Interfaces and generated output may change while the project approaches its first release.

## Contributing

Contributions to code, tests, documentation, templates, and design are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request. Participation in this project is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

For usage questions and non-sensitive problems, see [SUPPORT.md](SUPPORT.md). Do not disclose a vulnerability in a public issue; follow [SECURITY.md](SECURITY.md) instead.

## Governance

Maintainer responsibilities, project decision-making, and the path to maintainership are described in [GOVERNANCE.md](GOVERNANCE.md). Contributors are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md), and notable changes are recorded in [CHANGELOG.md](CHANGELOG.md).

## License

Copyright (c) 2026 Quinn Brown and contributors. Released under the [MIT License](LICENSE).
