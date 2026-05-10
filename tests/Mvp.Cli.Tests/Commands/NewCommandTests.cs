using Mvp.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Mvp.Cli.Services;
using System.CommandLine;
using Xunit;

namespace Mvp.Cli.Tests.Commands;

public class NewCommandTests
{
    private readonly IServiceProvider _services;

    public NewCommandTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<ISolutionGeneratorService>());
        services.AddSingleton(Mock.Of<IApiGeneratorService>());
        services.AddSingleton(Mock.Of<ICoreGeneratorService>());
        services.AddSingleton(Mock.Of<IInfrastructureGeneratorService>());
        services.AddSingleton(Mock.Of<IAppGeneratorService>());
        services.AddSingleton(Mock.Of<IAngularLibraryGeneratorService>());
        services.AddSingleton(Mock.Of<IProcessRunner>());
        _services = services.BuildServiceProvider();
    }

    [Fact]
    public void Create_ReturnsCommandWithNameNew()
    {
        var command = NewCommand.Create(_services);

        Assert.Equal("new", command.Name);
    }

    [Fact]
    public void Create_HasEightSubcommands()
    {
        var command = NewCommand.Create(_services);

        Assert.Equal(8, command.Subcommands.Count);
    }

    [Fact]
    public void Create_HasSolutionSubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "solution");
    }

    [Fact]
    public void Create_HasApiSubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "api");
    }

    [Fact]
    public void Create_HasAppSubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "app");
    }

    [Fact]
    public void Create_HasCoreSubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "core");
    }

    [Fact]
    public void Create_HasInfrastructureSubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "infrastructure");
    }

    [Fact]
    public void Create_HasApiLibrarySubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "api-library");
    }

    [Fact]
    public void Create_HasComponentsLibrarySubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "components-library");
    }

    [Fact]
    public void Create_HasDomainLibrarySubcommand()
    {
        var command = NewCommand.Create(_services);

        Assert.Contains(command.Subcommands, c => c.Name == "domain-library");
    }
}
