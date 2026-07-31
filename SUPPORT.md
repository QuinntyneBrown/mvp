# Support

`mvp` is a community-maintained open-source project. The resources below help route questions to the right place and keep security-sensitive information private.

## Start here

1. Read the [README](README.md), especially the prerequisites, installation, and post-generation guidance.
2. Run `mvp --help` or `mvp new <command> --help` for the installed command surface.
3. Check the [documentation index](docs/README.md) and [manifest reference](skills/dotnet-angular-jwt-mvp/references/manifest-schema.md).
4. Search [existing issues](https://github.com/QuinntyneBrown/mvp/issues) for the error message or behavior.

## Where to ask

| Need | Channel |
| --- | --- |
| Reproducible CLI defect | [Open a bug report](https://github.com/QuinntyneBrown/mvp/issues/new?template=bug_report.yml) |
| Feature or workflow proposal | [Open a feature request](https://github.com/QuinntyneBrown/mvp/issues/new?template=feature_request.yml) |
| Documentation correction | Open an issue or a focused pull request |
| Usage or troubleshooting question | [Open a support issue](https://github.com/QuinntyneBrown/mvp/issues/new/choose) with the question clearly labeled |
| Suspected vulnerability | Follow [SECURITY.md](SECURITY.md); do not open a public issue |
| Conduct concern | Follow the private reporting instructions in [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) |

## Writing a useful support request

Include:

- The exact command you ran.
- The output of `mvp --version`, `dotnet --info`, and your operating system version.
- A minimal, sanitized manifest if the issue involves YAML-driven generation.
- The expected result and the actual result.
- The smallest reproduction you can provide.
- Complete error text with secrets, personal information, and private paths removed.

Use fenced code blocks for commands and logs. Do not upload an entire private generated solution when a minimal example will reproduce the issue.

## Support boundaries

Maintainers can help with the CLI, its documented generation contracts, and defects in freshly generated output. Application architecture choices, feature development, deployment, cloud configuration, and code changed after generation remain the adopting team's responsibility.

Support is provided on a best-effort basis. There is no guaranteed response time or service-level agreement.
