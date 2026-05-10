using Microsoft.Extensions.DependencyInjection;
using Mvp.Cli.Services;

namespace Mvp.Cli.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMvpServices(this IServiceCollection services)
    {
        services.AddDotNetServices();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IApiGeneratorService, ApiGeneratorService>();
        services.AddSingleton<ICoreGeneratorService, CoreGeneratorService>();
        services.AddSingleton<IInfrastructureGeneratorService, InfrastructureGeneratorService>();
        services.AddSingleton<IAppGeneratorService, AppGeneratorService>();
        services.AddSingleton<IAngularLibraryGeneratorService, AngularLibraryGeneratorService>();
        services.AddSingleton<ISolutionGeneratorService, SolutionGeneratorService>();
        return services;
    }
}
