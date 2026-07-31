using Microsoft.Extensions.DependencyInjection;
using Mvp.Core.Features.FullStack.Generation;
using Mvp.Core.Features.FullStack.Manifest;
using Mvp.Core.Features.Generation;
using Mvp.Core.Features.Incremental;
using Mvp.Core.Infrastructure.Output;
using Mvp.Core.Infrastructure.Processes;

namespace Mvp.Core;

/// <summary>
/// Registers the generation engine with a dependency-injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds manifest loading and validation, template rendering, transactional output, and
    /// child-process execution to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection to add the engine to.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    public static IServiceCollection AddMvpCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddSingleton<ManifestValidator>()
            .AddSingleton<IManifestLoader, YamlManifestLoader>()
            .AddSingleton<ITransactionalGenerationOutput, TransactionalGenerationOutput>()
            .AddSingleton<IProcessRunner, ProcessRunner>()
            .AddSingleton<IIncrementalGenerator, IncrementalGenerator>()
            .AddSingleton<IFullStackGenerator, FullStackGenerator>();
    }
}
