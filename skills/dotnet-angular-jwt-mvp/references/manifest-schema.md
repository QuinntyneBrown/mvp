# Manifest schema

The YAML manifest is deserialized as data by `YamlManifestLoader` using `YamlDotNet` with camelCase naming. Files are limited to 1 MiB. Unmatched properties are ignored for forward compatibility and reported as warnings, so verify warnings and field names against this document.

## Top-level

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | yes | PascalCase, no spaces. Drives the solution name, namespace root, and (kebab-cased) Angular workspace prefix. `--name` on the command line overrides this. |
| `output` | string | no | Default output directory if `--output` is omitted on the command line. Resolved relative to the current working directory. |
| `entities` | list | no | Domain entities. Each one generates a Domain class, Application command/query handlers, validators, a DTO, and an API controller. |
| `pages` | list | no | Frontend pages beyond the always-generated sign-in/sign-up/dashboard. |
| `components` | list | no | Standalone Angular component units under the chosen library, including public exports. |

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
| `type` | string | no | Exact C# type: `string`, `int`, `long`, `decimal`, `bool`, `DateTime`, or `Guid`. Defaults to `string`; any other spelling is rejected. |

Entity names `User` and `RefreshToken` are rejected because they belong to the built-in identity model. Property names `Id` and `CreatedAt` are rejected because the generator supplies them. Names and routes are checked for case-insensitive duplicates before writing output. `Task` is supported — handler templates fully qualify `System.Threading.Tasks.Task` to avoid namespace shadowing.

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
| `library` | string | no | One of `api`, `components`, or `domain`. Defaults to `components`. |

Each component produces a standalone TypeScript component, HTML template, SCSS file, and public API export in the selected library.

## Example

See `assets/example-manifest.yaml` for a complete worked example covering entities with multiple properties, several pages with mixed auth requirements, and components in both libraries.
