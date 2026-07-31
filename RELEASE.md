# Release process

This repository publishes one package to NuGet.org: **`QuinntyneBrown.Mvp.Core`**. The `Mvp.Cli`
tool package is built, packed, and smoke-tested on every change but is not published yet.

Both packages share one version, declared once in [Directory.Build.props](Directory.Build.props).
Releases are cut from a Git tag; there is no other publish path.

## Versioning

Semantic Versioning applies to the repository as a whole. Both packages carry the same version and
are released in lockstep, so classify a release by its largest change across either package.

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
6. Verify <https://www.nuget.org/packages/QuinntyneBrown.Mvp.Core> and the generated GitHub release.

## Prerelease dry runs

Use `X.Y.Z-preview.N` to rehearse. The workflow marks the GitHub release as a prerelease
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

`NUGET_API_KEY` is stored on the `nuget` GitHub environment, not as a repository secret, so no other
workflow can read it. Keys expire after 365 days; rotate from
<https://www.nuget.org/account/apikeys> with the *Push new packages and package versions* scope and
glob pattern `QuinntyneBrown.Mvp.*`.

## One-time setup

These steps are manual and must be completed before the first release:

1. **NuGet.org API key** — create with the scope and glob above.
2. **ID prefix reservation** — confirm the `QuinntyneBrown.` prefix is reserved to the publishing
   account so the package shows the verified-owner mark.
3. **GitHub environment `nuget`** — add required reviewers, restrict deployment to `v*` tags, and
   store `NUGET_API_KEY` as an environment secret.
4. **Tag ruleset** — restrict `v*` tag creation to maintainers so a stray tag cannot trigger a
   publish.

Once the pipeline is proven, consider replacing the API key with
[trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing), which removes
the long-lived credential in exchange for `id-token: write` and a policy registered on NuGet.org.
