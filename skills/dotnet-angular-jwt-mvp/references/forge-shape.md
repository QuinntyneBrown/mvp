# What "Forge-shaped" means

When the user asks for something "like Forge" or with "Forge's complexity", this is the file-level inventory the CLI produces. Forge itself (the reference project at `C:\projects\forge` if you have access to it) has more domain detail — workouts, rewards, leaderboards, audit logs, refresh-token rotation, etc. — but the *shape* is the same: clean-architecture .NET, library-segmented Angular workspace, Playwright POM E2E.

The CLI doesn't try to recreate Forge's domain. It gives Claude (and the user) the same skeleton so that adding Forge-level features is a matter of writing handlers and components, not rebuilding the wiring.

## Generated tree (for a solution named `Acme` with two entities `Project` and `TaskItem`)

```
Acme/
├── README.md
├── .gitignore
├── backend/
│   ├── Acme.sln
│   ├── Directory.Build.props          # net9.0, nullable enable, implicit usings
│   ├── global.json                    # SDK version pin
│   ├── .gitignore
│   └── src/
│       ├── Acme.Domain/
│       │   ├── Acme.Domain.csproj     # No dependencies beyond System
│       │   ├── User.cs                # Identity user entity
│       │   ├── RefreshToken.cs        # Refresh-token entity
│       │   ├── Project.cs             # User-supplied entity
│       │   └── TaskItem.cs            # User-supplied entity
│       ├── Acme.Application/
│       │   ├── Acme.Application.csproj  # MediatR, FluentValidation, EF Core abstractions
│       │   ├── DependencyInjection.cs   # AddApplication() — MediatR + validators + ValidationBehavior
│       │   ├── Abstractions/
│       │   │   ├── IAppDbContext.cs
│       │   │   ├── IClock.cs
│       │   │   ├── ICurrentUser.cs
│       │   │   ├── IJwtTokenIssuer.cs
│       │   │   └── IPasswordHasher.cs
│       │   ├── Auth/
│       │   │   ├── AuthResult.cs                # Access token + claims DTO returned by both flows
│       │   │   ├── RegisterCommand.cs + Handler + Validator
│       │   │   ├── SignInCommand.cs   + Handler + Validator
│       │   │   ├── EmailAlreadyRegisteredException.cs
│       │   │   └── InvalidCredentialsException.cs
│       │   ├── Behaviors/
│       │   │   └── ValidationBehavior.cs        # MediatR pipeline behavior; runs FluentValidation before each handler
│       │   ├── Project/
│       │   │   ├── CreateProjectCommand.cs + Handler + Validator
│       │   │   ├── GetProjectByIdQuery.cs  + Handler
│       │   │   └── ProjectDto.cs
│       │   └── TaskItem/                # mirrors Project/
│       ├── Acme.Infrastructure/
│       │   ├── Acme.Infrastructure.csproj  # EF Core SqlServer + InMemory, BCrypt, AspNetCore framework ref
│       │   ├── AppDbContext.cs             # DbSets for Users, RefreshTokens; OnModelCreating fluent config
│       │   ├── BCryptPasswordHasher.cs     # WorkFactor=12
│       │   ├── DependencyInjection.cs      # AddInfrastructure() — registers DbContext (InMemory if conn string is "InMemory" or empty), services, JwtOptions
│       │   ├── HttpContextCurrentUser.cs   # Reads sub claim from HttpContext.User
│       │   ├── JwtOptions.cs               # Issuer/Audience/SigningKey/AccessTokenMinutes
│       │   ├── JwtTokenIssuer.cs           # HS256, 60-min expiry by default
│       │   └── SystemClock.cs
│       └── Acme.Api/
│           ├── Acme.Api.csproj             # JwtBearer, EF Core Design
│           ├── Program.cs                  # AddApplication + AddInfrastructure, JWT bearer, CORS for localhost:4200, exception handler that maps ValidationException -> 400
│           ├── appsettings.json            # ConnectionStrings.Default = "InMemory", Jwt section with PLACEHOLDER signing key
│           └── Controllers/
│               ├── AuthController.cs       # POST /api/auth/register, POST /api/auth/sign-in
│               ├── RegisterRequest.cs
│               ├── SignInRequest.cs
│               ├── ProjectController.cs    # [Authorize], POST + GET by id
│               └── TaskItemController.cs
└── frontend/
    ├── package.json                        # Angular 19, Material, RxJS, Playwright 1.48
    ├── angular.json                        # Application project + 3 library projects
    ├── tsconfig.json                       # @<name>/api, @<name>/components, @<name>/domain path aliases
    ├── playwright.config.ts
    ├── .gitignore
    ├── projects/
    │   ├── acme-app/                       # Main standalone Angular app
    │   │   ├── tsconfig.app.json
    │   │   └── src/
    │   │       ├── main.ts                 # bootstrapApplication
    │   │       ├── index.html
    │   │       ├── styles.scss
    │   │       └── app/
    │   │           ├── app.component.ts/.html/.scss
    │   │           ├── app.config.ts        # providers: router, http, animations, AUTH_SERVICE token
    │   │           ├── app.routes.ts        # sign-in, sign-up, dashboard (auth-guarded), user pages, **
    │   │           ├── auth-state.service.ts  # Signal-based, localStorage-backed
    │   │           ├── auth.guard.ts          # CanActivateFn redirecting to sign-in
    │   │           ├── auth.interceptor.ts    # Adds Bearer header from auth state
    │   │           └── pages/
    │   │               ├── sign-in/sign-in.page.ts/.html
    │   │               ├── sign-up/sign-up.page.ts/.html
    │   │               ├── dashboard/dashboard.page.ts/.html
    │   │               ├── projects/projects.page.ts/.html   # From manifest pages[]
    │   │               └── backlog/backlog.page.ts/.html
    │   ├── api/                            # Library: HTTP services + models
    │   │   └── src/
    │   │       ├── public-api.ts
    │   │       └── lib/
    │   │           ├── auth.service.ts             # Concrete implementation
    │   │           ├── auth.service.contract.ts    # IAuthService + AUTH_SERVICE InjectionToken
    │   │           ├── auth-result.model.ts
    │   │           ├── sign-in-request.model.ts
    │   │           └── register-request.model.ts
    │   ├── components/                     # Library: pure presentational components
    │   │   └── src/public-api.ts           # Placeholder export — populate as components are added
    │   └── domain/                         # Library: components that depend on api
    │       └── src/public-api.ts           # Placeholder export
    └── e2e/
        ├── pages/
        │   ├── base.page.ts                # Abstract BasePage with byTestId helper
        │   ├── sign-in.page.ts             # SignInPomPage extends BasePage
        │   ├── sign-up.page.ts
        │   ├── dashboard.page.ts
        │   ├── projects.page.ts            # From manifest pages[]
        │   └── backlog.page.ts
        └── tests/
            └── auth.spec.ts                # Registers a user via API, asserts dashboard renders
```

## How this maps to Forge

| Forge concept | What the CLI emits | Gap |
| --- | --- | --- |
| `Forge.Domain` entities | `<Name>.Domain/` with `User`, `RefreshToken`, plus user-supplied entities | Forge has ~10 domain entities (WorkoutSession, WeightEntry, RewardCatalogItem, etc.). User adds these via the manifest. |
| `Forge.Application` CQS + validators + behaviors | Same structure; `Auth/` is fully wired, per-entity folders contain Create + GetById | Forge has Update/Delete/List/Duplicate commands per entity; the CLI emits Create + GetById and leaves the rest to the user. |
| `Forge.Infrastructure` JWT, BCrypt, EF Core, throttle, refresh-token store, audit logger | JWT + BCrypt + EF Core wired identically | Sign-in throttling, refresh-token rotation, audit logging, security headers middleware are not yet generated. Add later by hand or in a follow-up CLI command. |
| `Forge.Api` controllers + middleware + Program.cs | AuthController + per-entity controllers + Program.cs with JWT bearer + exception handler | Forge has security-headers middleware, request logging, redacting logger factory. Not currently emitted. |
| Angular workspace with `api`, `components`, `domain`, `forge` app | Same four projects; main app named `<name>-app` | Forge ships ~25 page components (workouts list/detail, rewards catalog, leaderboard, profile, etc.). CLI emits sign-in/sign-up/dashboard plus whatever pages the manifest lists. |
| Playwright POM E2E with `auth`, `dashboard`, `workouts`, `rewards`, ... | `base.page.ts` + per-page POMs + auth.spec.ts | Forge's spec suite is much larger; CLI gives the pattern, not the volume. |

The summary: the CLI ships the **skeleton and conventions** of Forge — every architectural decision is wired and every layer references the next one correctly. What it doesn't ship is the **domain volume** — Forge has hundreds of files of feature code on top of this same skeleton. That's the work the user takes on after scaffolding.
