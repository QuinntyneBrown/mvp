using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Mvp.Cli.Bootstrap;
using Mvp.Cli.Commands;

namespace Mvp.Cli.Tests.CommandIntegration;

// Acceptance Test
// Traces to: L2-001, L2-002, L2-003, L2-004, L2-008, L2-021, L2-060, L2-061, L2-066, L2-068
// Description: The command tree is stable, in-process testable, concise on failure, and transactional.
public sealed class CommandIntegrationTests
{
    private static readonly string[] ExpectedCommands =
    [
        "api",
        "api-library",
        "app",
        "components-library",
        "core",
        "domain-library",
        "dotnet-angular-jwt-mvp",
        "infrastructure",
        "solution",
    ];

    [Fact]
    public void Create_ExposesTheNineStableGenerationCommands()
    {
        var root = CreateRoot();
        var newCommand = Assert.Single(root.Subcommands, command => command.Name == "new");

        Assert.Equal(ExpectedCommands, newCommand.Subcommands.Select(command => command.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Invoke_MissingFullStackNameReturnsValidationExitWithoutStackTrace()
    {
        var result = await InvokeAsync("new", "dotnet-angular-jwt-mvp");

        Assert.Equal(CliExitCodes.InvalidInput, result.ExitCode);
        Assert.Contains("solution name is required", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at ", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invoke_IncrementalGenerationCreatesMissingParentsThenProtectsOutput()
    {
        using var workspace = new TestWorkspace();
        var output = workspace.PathFor("missing parent", "nested output");

        var created = await InvokeAsync("new", "api", "--name", "OrderDesk", "--output", output);
        var conflicted = await InvokeAsync("new", "api", "--name", "OrderDesk", "--output", output);
        var replaced = await InvokeAsync("new", "api", "--name", "OrderDesk", "--output", output, "--force");

        Assert.Equal(CliExitCodes.Success, created.ExitCode);
        Assert.Contains("Generating api", created.Output, StringComparison.Ordinal);
        Assert.Equal(CliExitCodes.Conflict, conflicted.ExitCode);
        Assert.Equal(CliExitCodes.Success, replaced.ExitCode);
        Assert.Contains("Replaced the existing target", replaced.Output, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(output, "OrderDesk.Api", "OrderDesk.Api.csproj")));
    }

    [Fact]
    public async Task Invoke_InvalidNameMakesNoFilesystemChanges()
    {
        using var workspace = new TestWorkspace();
        var output = workspace.PathFor("must-not-exist");

        var result = await InvokeAsync("new", "core", "--name", "../Escape", "--output", output);

        Assert.Equal(CliExitCodes.InvalidInput, result.ExitCode);
        Assert.False(Directory.Exists(output));
    }

    [Fact]
    public async Task Invoke_HelpDescribesForceAndAngularCliPolicy()
    {
        var apiHelp = await InvokeAsync("new", "api", "--help");
        var appHelp = await InvokeAsync("new", "app", "--help");

        Assert.Equal(0, apiHelp.ExitCode);
        Assert.Contains("--force", apiHelp.Output, StringComparison.Ordinal);
        Assert.Contains("--use-angular-cli", appHelp.Output, StringComparison.Ordinal);
    }

    private static RootCommand CreateRoot()
    {
        var services = new ServiceCollection();
        services.AddMvpCli();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<RootCommandFactory>().Create();
    }

    private static async Task<InvocationResult> InvokeAsync(params string[] args)
    {
        var root = CreateRoot();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var configuration = new InvocationConfiguration
        {
            Output = output,
            Error = error,
            EnableDefaultExceptionHandler = false,
        };
        var exitCode = await root.Parse(args).InvokeAsync(configuration);
        return new InvocationResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record InvocationResult(int ExitCode, string Output, string Error);
}
