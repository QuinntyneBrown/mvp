using Microsoft.Extensions.DependencyInjection;
using Mvp.Cli.Commands;
using Mvp.Core;

namespace Mvp.Cli.Bootstrap;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMvpCli(this IServiceCollection services) => services
        .AddMvpCore()
        .AddSingleton<CliExceptionPolicy>()
        .AddSingleton<RootCommandFactory>();
}
