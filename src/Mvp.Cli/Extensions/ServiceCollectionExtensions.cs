using Microsoft.Extensions.DependencyInjection;
using Mvp.Cli.Services;

namespace Mvp.Cli.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMvpServices(this IServiceCollection services)
    {
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IApiGeneratorService, ApiGeneratorService>();
        services.AddSingleton<IAppGeneratorService, AppGeneratorService>();
        services.AddSingleton<ISolutionGeneratorService, SolutionGeneratorService>();
        return services;
    }
}
