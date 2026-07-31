# Project governance

This document explains how `mvp` is maintained, how decisions are made, and how contributors can take on greater responsibility.

## Principles

The project is guided by five principles:

- User safety and trust take priority over convenience.
- Public behavior is described by tests and versioned documentation.
- Decisions and their rationale should be visible and reviewable.
- Contributions are evaluated on technical merit and community impact.
- Authority is earned through sustained, constructive participation.

All participants must follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Roles

### Users

Users install the tool, evaluate generated output, report defects, request capabilities, and share feedback. No repository contribution is required.

### Contributors

Contributors improve code, tests, documentation, templates, design, or issue triage. Contributors are expected to follow [CONTRIBUTING.md](CONTRIBUTING.md) and participate constructively in review.

### Maintainers

Maintainers have repository permissions and are accountable for:

- Reviewing and merging contributions.
- Protecting architecture, compatibility, quality, and security boundaries.
- Triaging issues and coordinating vulnerability response.
- Managing releases and project metadata.
- Applying the Code of Conduct fairly.
- Keeping project decisions and delivery status transparent.

The current lead maintainer is [Quinntyne Brown](https://github.com/QuinntyneBrown).

## Decision making

Routine decisions are made through issues and pull-request review. Maintainers seek practical consensus by considering requirements, user impact, security, maintainability, compatibility, and implementation cost.

Changes with broad impact—such as a manifest breaking change, generated architecture change, security-default change, or new long-term dependency—should begin with an issue or design proposal before implementation.

When consensus cannot be reached in a reasonable time, the lead maintainer makes the final decision and records the rationale in the relevant issue or pull request. Security response and Code of Conduct enforcement may be handled privately when disclosure would cause harm.

## Review and merge policy

- Contributors do not merge their own pull requests unless another maintainer has approved them.
- At least one maintainer approval is required for a merge.
- Security-sensitive or architectural changes may require additional review.
- Required checks must pass, and material review comments must be resolved.
- Maintainers may close proposals that conflict with project scope, duplicate existing work, or cannot be supported responsibly.

The merging maintainer is responsible for ensuring release notes and documentation are updated when required.

## Releases

Maintainers decide when the project is ready for a release. A release should have:

- A documented and reviewed scope.
- Passing required builds and tests.
- Updated user documentation and changelog entries.
- Reviewed dependency and security posture.
- A versioned package and matching Git tag.

All packages in this repository share one version and are released in lockstep. `QuinntyneBrown.Mvp.Core` is published to NuGet.org; `Mvp.Cli` is packed and validated on every change but is not currently published. The procedure is documented in [RELEASE.md](RELEASE.md).

Security releases may use an expedited private process followed by public release notes and an advisory.

## Becoming a maintainer

An existing maintainer may nominate a contributor who has demonstrated sustained participation, sound technical judgment, respectful review, care for compatibility and security, and willingness to perform maintenance work.

Existing maintainers approve new maintainers by consensus. Repository access begins at the smallest permission level needed and may expand with responsibility.

Maintainers who expect to be inactive for an extended period should say so when practical. Access may be reduced after prolonged inactivity or removed immediately when required for project or community safety. Returning contributors may be nominated again through the normal process.

## Changing this document

Governance changes use the normal pull-request process and require explicit maintainer approval. Material changes should remain open long enough for community feedback unless an urgent safety or security concern requires immediate action.
