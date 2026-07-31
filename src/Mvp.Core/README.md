# QuinntyneBrown.Mvp.Core

The generation engine behind the [`mvp`](https://github.com/QuinntyneBrown/mvp) scaffolding tool.

It loads and validates a YAML manifest, renders embedded .NET 10 and Angular 22 templates with
[DotLiquid](https://github.com/dotliquid/dotliquid), and publishes the result to disk
transactionally. Templates ship inside the package, so generation needs no network access.

## Install

```
dotnet add package QuinntyneBrown.Mvp.Core
```

## Use

Register the engine, then resolve the piece you need:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Mvp.Core;
using Mvp.Core.Features.FullStack.Generation;
using Mvp.Core.Features.FullStack.Manifest;

var services = new ServiceCollection().AddMvpCore().BuildServiceProvider();

var loaded = services.GetRequiredService<IManifestLoader>().Load("mvp-manifest.yaml");
var manifest = services.GetRequiredService<ManifestValidator>()
    .Validate(loaded.Manifest, nameOverride: null, outputOverride: "./out", loaded.Warnings);

var result = await services.GetRequiredService<IFullStackGenerator>()
    .GenerateAsync(manifest, force: false, CancellationToken.None);

Console.WriteLine($"{result.Artifacts.Count} files written to {result.TargetPath}");
```

`IIncrementalGenerator` generates a single solution part instead of a whole tree:

```csharp
using Mvp.Core.Features.Generation;

await services.GetRequiredService<IIncrementalGenerator>().GenerateAsync(
    new GenerationRequest(GenerationKind.Api, "Acme", "./out"),
    CancellationToken.None);
```

## Behaviour worth knowing

- **Validation happens before any write.** `ManifestValidator` reports every problem at once and
  throws `ManifestValidationException` carrying the full list. Names, routes, property types, and
  duplicates are all checked, and no manifest-derived value can escape the output root.
- **Generation is transactional.** Output is rendered to a staging directory and published only
  once every stage succeeds, so a failed or cancelled run cannot be mistaken for a complete one.
- **Existing output is a conflict, not a target.** Generating over an existing directory throws
  `GenerationConflictException` unless `force` is set; replacement keeps a sibling backup and rolls
  back on failure.
- **Cancellation is honoured end to end**, including termination of any child process tree.

## Documentation

- [Manifest schema](https://github.com/QuinntyneBrown/mvp/blob/main/skills/dotnet-angular-jwt-mvp/references/manifest-schema.md)
- [Generated file inventory](https://github.com/QuinntyneBrown/mvp/blob/main/skills/dotnet-angular-jwt-mvp/references/forge-shape.md)
- [Requirements](https://github.com/QuinntyneBrown/mvp/blob/main/docs/specs/L2.md)
- [Changelog](https://github.com/QuinntyneBrown/mvp/blob/main/CHANGELOG.md)

Licensed under the [MIT License](https://github.com/QuinntyneBrown/mvp/blob/main/LICENSE).
