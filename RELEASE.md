# Release process

This repository publishes two packages to NuGet.org: **`QuinntyneBrown.Mvp.Core`**, the generation
engine, and **`Mvp.Cli`**, the `mvp` global tool.

Both packages share one version, declared once in [Directory.Build.props](Directory.Build.props).

There are two publish paths, and they are not interchangeable:

| Path | Trigger | Publishes | Approval |
| --- | --- | --- | --- |
| [publish-preview.yml](.github/workflows/publish-preview.yml) | every commit to `main`, after `ci` passes | `Mvp.Cli` as `X.Y.Z-preview.N` | none; runs unattended |
| [release.yml](.github/workflows/release.yml) | a `vX.Y.Z` tag | both packages as `X.Y.Z` | required, on the `nuget` environment |

A stable release is always cut from a tag. The preview path exists so the newest `main` is
installable with `dotnet tool install -g Mvp.Cli --prerelease`; it never produces a stable version.

## Versioning

Semantic Versioning applies to the repository as a whole. Both packages carry the same version and
are released in lockstep, so classify a release by its largest change across either package.

### Bump `<Version>` immediately after a release

A prerelease sorts *below* the stable version it names: `2.2.0-preview.4` is older than `2.2.0`.
So once `v2.2.0` ships, leaving `<Version>` at `2.2.0` would make every subsequent preview publish
as older than what users already have.

`publish-preview.yml` refuses to publish when a tag matching the declared version already exists,
and says so in the run log. The fix is to bump `<Version>` on `main` to the next planned version as
soon as a release is cut. Treat that bump as the last step of the release, not the first step of
the next one.

Contributors must not edit `<Version>` in a feature pull request — add an entry to the
`Unreleased` section of [CHANGELOG.md](CHANGELOG.md) instead. Bumping the version is a release step,
and the release workflow refuses to publish when the tag and the declared version disagree.

## Cut a release

1. Confirm `main` is green and the `Unreleased` changelog section describes the release.
2. Open a release-preparation pull request:
   - Set `<Version>` in `Directory.Build.props`.
   - Rename `## [Unreleased]` to `## [X.Y.Z] - YYYY-MM-DD`, add a fresh empty `Unreleased`, and
     update the reference links at the bottom of `CHANGELOG.md`.
   - Update the supported-versions table in [SECURITY.md](SECURITY.md) if the minor line changed.
3. Merge after review.
4. Tag the merge commit and push:

   ```
   git switch main
   git pull
   git tag -a vX.Y.Z -m "vX.Y.Z"
   git push origin vX.Y.Z
   ```

5. The `release` workflow runs the full CI matrix, then waits for approval on the `nuget`
   environment. Approve it from the run summary.
6. Verify <https://www.nuget.org/packages/QuinntyneBrown.Mvp.Core>,
   <https://www.nuget.org/packages/Mvp.Cli>, and the generated GitHub release.
7. Bump `<Version>` on `main` to the next planned version, so preview publishing keeps working.

## Prerelease dry runs

Use `X.Y.Z-rc.N` to rehearse a release. **Do not use `-preview.N`**: that label belongs to the
automated per-commit stream, and a hand-cut tag would collide with a version the preview workflow
may already have claimed. The release workflow marks the GitHub release as a prerelease
automatically when the version contains a hyphen.

Rehearse before the **first** publish of any new package ID. NuGet.org permanently claims the ID on
first push and allows unlisting but never deletion, so a mistake is not undoable.

## Failure recovery

- **Tag/version mismatch or missing changelog section** — the workflow fails before publishing.
  Delete the tag (`git push --delete origin vX.Y.Z`), fix `main`, re-tag.
- **Push succeeded, a later step failed** — re-run the job. `--skip-duplicate` makes the push
  idempotent rather than a 409.
- **Bad package published** — NuGet.org packages cannot be deleted. Unlist it and release a fix
  version.

## Credentials

`NUGET_API_KEY` is a **repository** secret. It cannot live on the `nuget` environment: that
environment carries the approval gate, and the unattended preview publish would then be unable to
read the key without also inheriting the gate.

The key's scope must cover **both** package IDs. A key globbed only to `QuinntyneBrown.Mvp.*`
cannot push `Mvp.Cli` — the push fails with a 403 that reads like an authentication problem.

Keys expire after 365 days; rotate from <https://www.nuget.org/account/apikeys> with the
*Push new packages and package versions* scope and glob patterns `QuinntyneBrown.Mvp.*` **and**
`Mvp.Cli`.

## One-time setup

These steps are manual and must be completed before the first release:

1. **NuGet.org API key** — create with the scope and both globs above, then store it:

   ```
   gh secret set NUGET_API_KEY --repo QuinntyneBrown/mvp
   ```

2. **ID prefix reservation** — confirm the `QuinntyneBrown.` prefix is reserved to the publishing
   account so the library shows the verified-owner mark. `Mvp.Cli` sits outside that prefix and
   carries no reservation.
3. **GitHub environment `nuget`** — add required reviewers and restrict deployment to `v*` tags.
   The environment provides the approval gate only; it holds no secret.
4. **Tag ruleset** — restrict `v*` tag creation to maintainers so a stray tag cannot trigger a
   publish.

Once the pipeline is proven, consider replacing the API key with
[trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing), which removes
the long-lived credential in exchange for `id-token: write` and a policy registered on NuGet.org.
