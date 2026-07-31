# Security policy

Security issues in `mvp` can affect both the CLI and the applications it generates. Thank you for reporting vulnerabilities responsibly and giving maintainers an opportunity to address them before public disclosure.

## Supported versions

Security fixes are made against the current `2.1.x` line and the `main` branch. This policy covers both the `mvp` tool and the `QuinntyneBrown.Mvp.Core` package, which share a version.

| Version | Supported |
| --- | --- |
| 2.1.x | Yes |
| 2.0.x | No |
| Current `main` branch | Yes |
| 1.x and earlier commits | No |
| Locally modified builds | No |

## Report a vulnerability

Do not disclose a suspected vulnerability in a public issue, pull request, discussion, test fixture, or log.

Use [GitHub's private vulnerability reporting flow](https://github.com/QuinntyneBrown/mvp/security/advisories/new). If that flow is unavailable, contact the [lead maintainer](https://github.com/QuinntyneBrown) privately using a contact method listed on the profile and ask for a secure reporting channel. Do not include exploit details in a public message.

Include as much of the following as is safe:

- Whether the issue affects the CLI, generated source, or both.
- The affected commit or tool version.
- A concise description of the vulnerability and its potential impact.
- Reproduction steps or a minimal proof of concept.
- Relevant operating system, .NET SDK, Node.js, and browser versions.
- Suggested mitigations or fixes, if known.
- Whether the issue has been disclosed elsewhere.

Remove credentials, personal data, proprietary manifests, and unrelated generated source from the report.

## What to expect

Maintainers will make a best effort to:

- Acknowledge a report within five business days.
- Confirm whether the report is in scope and provide an initial assessment within ten business days.
- Share material status changes at least every fourteen days while remediation is active.
- Coordinate a disclosure date and credit with the reporter when practical.

Timelines may vary with complexity, maintainer availability, and downstream coordination. Please allow a reasonable remediation period before public disclosure.

## Scope

In-scope reports include vulnerabilities in:

- CLI argument, manifest, filesystem, and process handling.
- Dependency use and package distribution for `Mvp.Cli`.
- Authentication, authorization, secret handling, validation, persistence, or error handling emitted by the authenticated generator.
- Generated defaults that create an exploitable condition before a consumer modifies the output.

Usually out of scope are:

- Vulnerabilities introduced only after a consumer changes generated code or configuration.
- Unsupported dependencies or platforms outside this repository's control, unless the generator selects or configures them unsafely.
- Reports that require access to a victim's machine or repository without demonstrating a boundary violation.
- Missing hardening that is already clearly documented as required before production deployment.

If you are uncertain, report privately and let the maintainers assess it.

## Coordinated disclosure

For accepted vulnerabilities, maintainers will work toward a fix, regression coverage, affected-version guidance, and a security advisory when warranted. Public disclosure should occur after a fix or mitigation is available, unless earlier disclosure is necessary to protect users.

Good-faith security research conducted within applicable law, without privacy violations, service disruption, data destruction, or harm to others, will not be treated as malicious activity by this project.
