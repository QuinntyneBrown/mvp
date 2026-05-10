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
        services.AddSingleton(Mock.Of<IAppGeneratorService>());
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
    public void Create_HasThreeSubcommands()
    {
        var command = NewCommand.Create(_services);

        Assert.Equal(3, command.Subcommands.Count);
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
}
