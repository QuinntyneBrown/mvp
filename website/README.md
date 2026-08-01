# Documentation site

The public documentation site for the `mvp` CLI, live at
<https://happy-mud-0689bad0f.7.azurestaticapps.net>. Hand-written static HTML with no build
step, no dependencies, and no external requests.

## Layout

```text
website/
├── index.html            Overview
├── install.html          Prerequisites and installation
├── quickstart.html       Generate, build, run, test
├── commands.html         Complete CLI reference
├── manifest.html         YAML manifest contract
├── generated.html        Generated file inventory
├── output-safety.html    Staging, conflicts, --force, determinism
├── errors.html           Exit codes and every message
├── limitations.html      Non-goals and delivery status
├── library.html          QuinntyneBrown.Mvp.Core API
├── 404.html
├── robots.txt
├── check-links.sh        Reference checker used by CI
├── staticwebapp.config.json
└── assets/
    ├── styles.css
    ├── site.js
    └── favicon.svg
```

Links between pages are relative and include the `.html` extension, so the site behaves
identically from `file://`, Azure Static Web Apps, GitHub Pages, or a storage container.

## Preview locally

Open `index.html` in a browser. Everything works from the filesystem — navigation, the
theme toggle, copy buttons, and the generated table of contents.

To exercise it over HTTP instead:

```shell
python -m http.server 8080 --directory website
# or
npx --yes http-server website -p 8080
```

## Check the links

CI runs this before deploying; run it yourself after renaming or adding a page:

```shell
bash website/check-links.sh
```

It resolves every relative `href` and `src` in every page and fails on the first miss.

## Constraints to preserve when editing

- **No external requests.** The deployed `Content-Security-Policy` is `default-src 'self'`
  with no `unsafe-inline`. No CDN scripts, no web fonts, no shields.io badges, no remote
  images. Status pills are CSS and the logo is inline SVG.
- **No inline `<script>` or `<style>`.** Both would be blocked. `assets/site.js` is loaded
  from `<head>` without `defer` so a stored theme choice applies before first paint.
- **Wide content scrolls itself.** Tables live in `.table-scroll` and code in `.code` or
  `.terminal`, each with its own horizontal scroll, so the page body never scrolls
  sideways.
- **Both themes.** Colours come from custom properties defined for
  `prefers-color-scheme` and overridden by `:root[data-theme="light"|"dark"]`. Check any
  new colour in both.
- **Content is captured, not remembered.** Console output, file trees, file counts, and
  error messages on this site were taken from real runs of the version in the footer. When
  the CLI changes, re-run it and paste the new output rather than editing prose to match.

## Deployment

Pushing to `main` with changes under `website/**` runs
[`.github/workflows/website.yml`](../.github/workflows/website.yml), which checks links and
deploys to Azure Static Web Apps. Pull requests get a preview environment, torn down when
the pull request closes.

A pull request from a fork runs the link check but skips deployment — forks cannot read the
deployment token, so the deploy would fail on an empty secret rather than say anything
useful.

### One-time setup

The workflow needs a repository secret named `AZURE_STATIC_WEB_APPS_API_TOKEN`.

1. Create a Static Web App in the Azure portal, or:

   ```shell
   az staticwebapp create \
     --name mvp-docs \
     --resource-group <resource-group> \
     --location <region> \
     --sku Free
   ```

2. Read its deployment token:

   ```shell
   az staticwebapp secrets list --name mvp-docs --query "properties.apiKey" -o tsv
   ```

3. Add it as a repository secret:

   ```shell
   gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "<token>"
   ```

Create the Static Web App **without** letting Azure add its own workflow — this repository
already has one, and Azure's generated file would duplicate the deployment.

### Alternative: Azure Storage static website

`staticwebapp.config.json` is ignored by Storage, so the security headers and the 404
rewrite it declares do not apply. Set the index and error documents on the account, then:

```shell
az storage blob upload-batch --account-name <account> -s website -d '$web'
```

### Adding a sitemap

No `sitemap.xml` ships, because a sitemap must contain absolute URLs and the canonical
hostname is assigned by the hosting environment. Once the site has a stable public address,
add one and reference it from `robots.txt`.
