# Manifest schema

The YAML manifest is deserialized by `Mvp.Cli.Manifests.YamlMvpManifestLoader` using `YamlDotNet` with camelCase naming. Unmatched properties are ignored, so older manifests stay compatible with newer tool versions, but typos won't error — double-check field names against this document.

## Top-level

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | yes | PascalCase, no spaces. Drives the solution name, namespace root, and (kebab-cased) Angular workspace prefix. `--name` on the command line overrides this. |
| `output` | string | no | Default output directory if `--output` is omitted on the command line. Resolved relative to the current working directory. |
| `entities` | list | no | Domain entities. Each one generates a Domain class, Application command/query handlers, validators, a DTO, and an API controller. |
| `pages` | list | no | Frontend pages beyond the always-generated sign-in/sign-up/dashboard. |
| `components` | list | no | Empty component stubs under the chosen Angular library. |

If `name` is missing, the CLI exits with a non-zero status.

## `entities[]`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | yes | PascalCase, singular. Used as a class name and namespace segment. |
| `properties` | list | no | Persisted properties on the entity, beyond the implicit `Id: Guid` and `CreatedAt: DateTime`. |

### `entities[].properties[]`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | yes | PascalCase. Used as the C# property name and as the camelCase JSON/TypeScript name (when emitted to the frontend in future versions). |
| `type` | string | yes | C# type. Templates currently understand `string`, `int`, `long`, `decimal`, `bool`, `DateTime`, `Guid`. Unrecognized types pass through verbatim, which works for any type the generated `using` directives bring into scope but doesn't add a FluentValidation rule. |

Avoid entity names that collide with built-in identity entities (`User`, `RefreshToken`); the CLI will overwrite or duplicate them. `Task` is supported — handler templates fully-qualify `System.Threading.Tasks.Task` to avoid namespace shadowing.

## `pages[]`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | yes | PascalCase. Becomes the standalone component class name. |
| `route` | string | no | Kebab-case URL segment. If omitted, the kebab-cased page name is used. |
| `requiresAuth` | bool | no | Defaults to `true`. When `true`, the page is wrapped by the auth guard in `app.routes.ts`. |

Each page produces:

- `projects/<name>-app/src/app/pages/<route>/<route>.page.ts` (standalone component)
- `projects/<name>-app/src/app/pages/<route>/<route>.page.html` (placeholder content)
- A route entry in `app.routes.ts`
- `e2e/pages/<route>.page.ts` (Playwright POM extending `BasePage`)

The sign-in, sign-up, and dashboard pages are always generated regardless of what's in this list.

## `components[]`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | yes | PascalCase. |
| `library` | string | no | Either `components` (pure presentational, no dependency on the api library) or `domain` (may depend on the api library). Defaults to `components`. |

Component scaffolding is currently lightweight — it creates a placeholder TS file under the chosen library. The intent is to give Claude a hint about where each reusable component belongs as the project grows, not to fully generate them.

## Example

See `assets/example-manifest.yaml` for a complete worked example covering entities with multiple properties, several pages with mixed auth requirements, and components in both libraries.
