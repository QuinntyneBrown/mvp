using Mvp.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Mvp.Cli.Services;
using System.CommandLine;
using Xunit;

namespace Mvp.Cli.Tests.Commands;

public class NewSolutionCommandTests
{
    private readonly IServiceProvider _services;

    public NewSolutionCommandTests()
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
    public void Create_ReturnsCommandWithNameSolution()
    {
        var command = NewSolutionCommand.Create(_services);

        Assert.Equal("solution", command.Name);
    }

    [Fact]
    public void Create_CommandHasNameOption()
    {
        var command = NewSolutionCommand.Create(_services);

        var nameOption = command.Options.FirstOrDefault(o => o.Name == "name");
        Assert.NotNull(nameOption);
    }

    [Fact]
    public void Create_CommandHasOutputOption()
    {
        var command = NewSolutionCommand.Create(_services);

        var outputOption = command.Options.FirstOrDefault(o => o.Name == "output");
        Assert.NotNull(outputOption);
    }

    [Fact]
    public void Create_NameOptionIsRequired()
    {
        var command = NewSolutionCommand.Create(_services);

        var nameOption = command.Options.FirstOrDefault(o => o.Name == "name") as Option<string>;
        Assert.NotNull(nameOption);
        Assert.True(nameOption.IsRequired);
    }
}
