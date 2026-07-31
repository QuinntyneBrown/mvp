using Microsoft.Extensions.DependencyInjection;
using Mvp.Cli.Commands;
using Mvp.Cli.Features.FullStack.Generation;
using Mvp.Cli.Features.FullStack.Manifest;
using Mvp.Cli.Features.Generation;
using Mvp.Cli.Features.Incremental;
using Mvp.Cli.Infrastructure.Output;
using Mvp.Cli.Infrastructure.Processes;

namespace Mvp.Cli.Bootstrap;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMvpServices(this IServiceCollection services) => services
        .AddSingleton<CliExceptionPolicy>()
        .AddSingleton<ManifestValidator>()
        .AddSingleton<IManifestLoader, YamlManifestLoader>()
        .AddSingleton<ITransactionalGenerationOutput, TransactionalGenerationOutput>()
        .AddSingleton<IProcessRunner, ProcessRunner>()
        .AddSingleton<IIncrementalGenerator, IncrementalGenerator>()
        .AddSingleton<IFullStackGenerator, FullStackGenerator>()
        .AddSingleton<RootCommandFactory>();
}
