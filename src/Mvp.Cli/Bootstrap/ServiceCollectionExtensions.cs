using Microsoft.Extensions.DependencyInjection;
using Mvp.Cli.Commands;
using Mvp.Core.Features.FullStack.Generation;
using Mvp.Core.Features.FullStack.Manifest;
using Mvp.Core.Features.Generation;
using Mvp.Core.Features.Incremental;
using Mvp.Core.Infrastructure.Output;
using Mvp.Core.Infrastructure.Processes;

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
