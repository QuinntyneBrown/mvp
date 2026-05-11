---
name: dotnet-angular-jwt-mvp
description: Scaffold a complete JWT-authenticated .NET + Angular MVP solution using the `mvp` CLI dotnet tool. Use this skill whenever the user wants to start a new full-stack web app, asks for a JWT-authenticated backend with an Angular frontend, mentions "scaffolding an MVP", "starter project", "boilerplate", or wants something with the shape of the Forge reference project (clean-architecture .NET API + Angular workspace with api/components/domain libraries + Playwright E2E). Trigger even when the user doesn't say "scaffold" explicitly — phrases like "I want to start building an app", "set up a new project for X", or "I need a backend with login and a frontend" all qualify. Prefer this over hand-writing a project skeleton; the CLI is faster and produces something that builds.
---

# dotnet-angular-jwt-mvp

Scaffold a Forge-shaped MVP — a `dotnet build`-clean backend plus an Angular workspace skeleton plus Playwright POMs — using the `mvp` dotnet global tool. The tool ships a single command, `mvp new dotnet-angular-jwt-mvp`, that reads a YAML manifest describing the solution and emits a working tree. Source lives at `C:\projects\mvp` on this machine; the install steps below work from any directory once the tool is installed.

## When to use this skill

Use this skill whenever:

- The user wants to start a new full-stack web app and the stack matches (.NET 9 + Angular + JWT auth).
- The user describes a domain ("I need an app for tracking projects and tasks") and wants the wiring set up so they can focus on features.
- The user says "make me something like Forge" or refers to the Forge reference project's shape.
- The user is exploring an idea and wants a runnable baseline within a minute.

Skip this skill when the stack doesn't match — e.g., the user wants Next.js, a Python backend, a mobile app, or wants to add features to an existing project rather than start fresh.

## The shape of what this produces

The CLI emits a `<output>/<Name>/` tree with three top-level folders:

- **`backend/`** — `dotnet build`-clean solution. Four projects (`<Name>.Domain`, `<Name>.Application`, `<Name>.Infrastructure`, `<Name>.Api`) following the conventions in `docs/technology-guidance-and-practices.md`: MediatR + CQS, FluentValidation with a pipeline behavior, EF Core (in-memory by default, SqlServer via connection string), BCrypt password hashing, JWT bearer in `Program.cs`, `Register`/`SignIn` commands wired through an `AuthController`. Each user-supplied entity gets a Domain class, a `CreateXCommand`+handler+validator, a `GetXByIdQuery`+handler, a DTO, and a controller.
- **`frontend/`** — Angular 19 workspace. Main app under `projects/<name>-app/` (standalone components, `auth-state.service`, `auth.guard`, `auth.interceptor`, sign-in/sign-up/dashboard pages), plus library projects `projects/api` (interface-driven service consumption with `AUTH_SERVICE` token + `IAuthService` contract), `projects/components`, and `projects/domain`. User-supplied pages each get their own folder, route, and a matching Playwright POM.
- **`frontend/e2e/`** — Playwright config + `pages/base.page.ts` + per-page POMs + a passing `auth.spec.ts` that registers a user and asserts the dashboard renders.

See `references/forge-shape.md` for the full file-level inventory when the user wants more detail.

## Prerequisites

Before running the CLI:

1. **.NET 9 SDK** — verify with `dotnet --list-sdks` and look for a `9.x` entry. (`dotnet --version` only reports the highest SDK on PATH, which can be misleading when multiple SDKs are installed and a `global.json` pins to an older one.)
2. **The `mvp` tool installed globally.** As of writing, `Mvp.Cli` is not yet on nuget.org, so install from a local clone of the mvp repo:
   ```
   cd C:\projects\mvp
   dotnet pack src/Mvp.Cli -c Release -o ./nupkgs
   dotnet tool install -g --add-source ./nupkgs Mvp.Cli
   ```
   Once `Mvp.Cli` is published to nuget.org, the shorter form will work:
   ```
   dotnet tool install -g Mvp.Cli
   ```
   To upgrade later: `dotnet tool update -g Mvp.Cli`.

   **Verify the install before continuing**: run `mvp --help`. You should see `new` listed as a subcommand. If the shell reports `mvp: command not found` or `'mvp' is not recognized`, the global tools directory isn't on PATH — that's typically `%USERPROFILE%\.dotnet\tools` on Windows or `~/.dotnet/tools` on macOS/Linux. Add it, restart the shell, and re-verify.
3. **Node.js 20+** — only needed when the user wants to run the generated frontend. Not required for scaffolding itself.
4. **Playwright browsers** — only needed to run the generated E2E tests: `npx playwright install` inside `frontend/` after `npm install`.

If any of these are missing, stop and tell the user what's needed before scaffolding — running the CLI without the tool installed is the most common point of failure.

## The workflow

The flow is: **interview → manifest → run → post-scaffold**. Each step is short.

### 1. Interview the user

You need enough to write a manifest. Ask in plain language, not as a form:

- **Solution name** — PascalCase, no spaces (e.g., `Acme`, `Forge`, `Hearthstone`). This becomes the .NET solution name, csproj prefix, namespace root, and Angular workspace prefix.
- **Output directory** — where on disk. Default to the current working directory. The CLI will create a `<Name>/` subfolder.
- **Domain entities** — for each one, the name (PascalCase, singular) and the properties. Each property has a name (PascalCase) and a C# type (`string`, `int`, `long`, `decimal`, `bool`, `DateTime`, `Guid`). Don't ask for `Id` or `CreatedAt` — those are generated automatically.
- **Pages beyond auth/dashboard** — names (PascalCase) and routes (kebab-case). Ask whether each page requires authentication; default to yes.
- **Reusable components** *(optional, often skip)* — names plus which library (`components` for pure presentational, `domain` for ones that depend on the api library).

Be flexible. If the user already described their domain conversationally ("track workouts with date, duration, and equipment"), translate that into entities and properties yourself and read it back for confirmation — don't make them restate it as a list.

If the user just says "set up a starter with no entities, I'll add them later", that's fine — generate with empty entities/pages. The auth flow alone is a useful baseline.

### 2. Write the manifest

Create `mvp-manifest.yaml` (or any name) in a sensible location. Use the schema in `references/manifest-schema.md` and `assets/example-manifest.yaml` as a template. Show it to the user and let them edit before running — manifests are cheap to iterate on, scaffold runs are slower.

### 3. Run the command

```
mvp new dotnet-angular-jwt-mvp --config mvp-manifest.yaml --output <output-dir>
```

`--name` on the command line overrides whatever's in the manifest's `name:` field; useful when iterating.

Watch the output. It should end with `Generated JWT-authenticated MVP at '<path>'`. If it errors, the most common causes are: the output directory doesn't exist (the CLI will create it, but the *parent* must exist), the manifest has a type the templates don't recognize (stick to the C# types listed above), or the tool isn't actually installed (look for `mvp: command not found` or `'mvp' is not recognized`).

### 4. Post-scaffold steps

After the tree is generated, verify and prepare it for use:

1. **Replace the JWT signing key.** Open `<Name>/backend/src/<Name>.Api/appsettings.json` and replace the placeholder `Jwt.SigningKey` value with a freshly generated random string of at least 32 characters. The default placeholder is unsafe for anything beyond a first run; flag this to the user explicitly.
2. **Build the backend** to confirm everything compiled correctly:
   ```
   cd <Name>/backend
   dotnet build
   ```
   Expect zero errors. If anything fails, the per-entity templates are the most likely culprit (entity names that collide with framework types — the templates handle `Task` correctly but other system types may need a rename).
3. **Install frontend dependencies** *(only if the user wants to run the frontend now)*:
   ```
   cd <Name>/frontend
   npm install
   ```
   `npm install` is slow (1–3 minutes typical); skip it if the user is just going to read the code.
4. **Install Playwright browsers** *(only if the user wants to run E2E)*:
   ```
   npx playwright install
   ```

Don't run all of these unprompted. Build the backend (fast, useful verification), tell the user about the JWT key, and leave the npm/playwright steps for when they're ready to actually run the app.

## Things to remember

**Don't reinvent the templates.** If the user asks for "one small change" — say, an extra property on an entity — the right answer is to edit the manifest and re-scaffold into a fresh directory, then copy any handwritten code they've already added back. Editing the generated files directly works but loses the ability to regenerate cleanly. For larger changes (more entities later, new pages), regenerate and merge.

**`Task` as an entity name is fine.** The templates fully-qualify `System.Threading.Tasks.Task` everywhere it matters so a user-supplied entity named `Task` doesn't shadow it. But entity names that collide with `User` or `RefreshToken` will create duplicates of the built-in identity entities — pick different names.

**The in-memory database means data evaporates on restart.** That's intentional for fast first-run, but flag it: when the user is ready to persist, point them at the `ConnectionStrings.Default` field in `appsettings.json` and tell them to set it to a SQL Server connection string. The Infrastructure project already references `Microsoft.EntityFrameworkCore.SqlServer`, so no package changes are needed.

**The `mvp` tool source lives at `C:\projects\mvp`.** If the user changes the CLI source and asks to "rebuild" or "reinstall" the tool, run the pack-and-install sequence from Prerequisites — the `dotnet tool install` will refuse if the tool is already installed, so prefix it with `dotnet tool uninstall -g Mvp.Cli` or use `dotnet tool update -g Mvp.Cli --add-source ./nupkgs` instead.

## References

- `references/manifest-schema.md` — full YAML schema for the manifest, with every field documented.
- `references/forge-shape.md` — file-level inventory of what gets generated, mapped to Forge's structure. Read this when the user asks "what exactly will I get?" or "is this going to look like Forge?".
- `assets/example-manifest.yaml` — a worked manifest covering entities, pages, and components. Copy and adapt rather than writing from scratch.
