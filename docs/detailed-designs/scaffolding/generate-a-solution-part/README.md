# Generate a solution part

## Overview

Incremental generation creates one structural part without rebuilding an entire
application. A *solution part* is a solution shell, backend project, Angular
application, or Angular library selected below the `mvp new` command group.

This feature supports extension of an existing work area. Each command owns a
small typed input surface and delegates file creation to a service dedicated to
that artifact family.

## Description

- **`NewSolutionCommand`** — creates a solution shell through
  `ISolutionGeneratorService`. The current service also adds an API project and
  Angular application.
- **`NewApiCommand`, `NewCoreCommand`, and `NewInfrastructureCommand`** — create
  individual backend projects through their matching generator interfaces.
- **`ApiGeneratorService`, `CoreGeneratorService`, and
  `InfrastructureGeneratorService`** — use `IProjectFactory` and
  `IArtifactGenerator` from the code-generation package.
- **`NewAppCommand` and `AppGeneratorService`** — create an Angular application.
  The service uses Angular CLI when available and writes a minimal local
  structure when it is absent.
- **`NewApiLibraryCommand`, `NewComponentsLibraryCommand`, and
  `NewDomainLibraryCommand`** — select one of the three frontend library kinds.
- **`AngularLibraryGeneratorService`** — writes the library directory and
  public exports. API and domain libraries expose a service interface, concrete
  service, and injection token. A components library emits a card component.
- **`IProcessRunner`** — process boundary used for Angular CLI detection and
  invocation.

The current component-library output is a placeholder card unit, matching the
partial status recorded for `L2-011` in the source specification.

## Requirements

The table reproduces the normative requirement text from `docs/specs/L2.md`.

| L2 ID | Refines (L1) | Requirement |
|-------|--------------|-------------|
| `L2-022` | `L1-005` | The product must generate a solution shell — a solution file with its initial backend project and frontend application — independently of full-stack generation. |
| `L2-023` | `L1-005` | The product must generate each backend layer project independently, so a consumer can add a missing layer to existing work. |
| `L2-024` | `L1-005` | The product must generate a standalone frontend application project independently of full-stack generation. |
| `L2-025` | `L1-005` | The product must generate each of the three frontend library types independently. |
| `L2-026` | `L1-005` | Generated frontend services must be consumed through an interface and an injection token rather than a concrete type, so that consumers can substitute implementations in tests and in alternative deployments. |

## Diagrams

### System context

The consumer selects one artifact family through `mvp`; optional Angular CLI
participates only in frontend application generation.

![C4 system context for generating a solution part](diagrams/c4-context.png)

### Containers

The CLI dispatches backend generation to the .NET artifact library and frontend
generation to either local writers or Angular CLI.

![C4 container view for generating a solution part](diagrams/c4-container.png)

### Components

Each command resolves one generator interface, while shared package factories
or `IProcessRunner` perform the low-level work.

![C4 component view for generating a solution part](diagrams/c4-component.png)

### Class structure

Command factories depend on generator interfaces; concrete services depend on
the package factory, artifact writer, or process runner appropriate to the part.

![Class diagram for generating a solution part](diagrams/class-structure.png)

### Behaviour — generate one part

The selected command validates `--name` and `--output`, resolves its service,
and reports the generated artifact under `L2-022` through `L2-026`.

![Sequence diagram for generating a solution part](diagrams/sequence-generate-part.png)
