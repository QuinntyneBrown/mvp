using Mvp.Core.Features.Generation;
using Mvp.Core.Features.Incremental;
using Mvp.Core.Infrastructure.Output;
using Mvp.Core.Infrastructure.Processes;

namespace Mvp.Cli.Tests.Golden;

// Acceptance Test
// Traces to: L2-022, L2-023, L2-024, L2-025, L2-058, L2-063, L2-067
// Description: Incremental commands render packaged offline templates by default.
public sealed class IncrementalGenerationTests
{
    [Theory]
    [InlineData(GenerationKind.Solution, "OrderDesk/OrderDesk.sln")]
    [InlineData(GenerationKind.Api, "OrderDesk.Api/OrderDesk.Api.csproj")]
    [InlineData(GenerationKind.Core, "OrderDesk.Core/OrderDesk.Core.csproj")]
    [InlineData(GenerationKind.Infrastructure, "OrderDesk.Infrastructure/OrderDesk.Infrastructure.csproj")]
    [InlineData(GenerationKind.App, "OrderDesk.App/package.json")]
    [InlineData(GenerationKind.ApiLibrary, "OrderDesk.Api/src/lib/public-api.ts")]
    [InlineData(GenerationKind.ComponentsLibrary, "OrderDesk.Components/src/lib/public-api.ts")]
    [InlineData(GenerationKind.DomainLibrary, "OrderDesk.Domain/src/lib/public-api.ts")]
    public async Task GenerateAsync_CreatesExpectedPackagedTemplate(GenerationKind kind, string expected)
    {
        using var workspace = new TestWorkspace();
        var process = new ThrowingProcessRunner();
        var generator = new IncrementalGenerator(new TransactionalGenerationOutput(), process);

        await generator.GenerateAsync(new GenerationRequest(kind, "OrderDesk", workspace.Root), CancellationToken.None);

        Assert.True(File.Exists(workspace.PathFor(expected.Split('/'))));
        Assert.False(process.WasCalled);
    }

    [Fact]
    public async Task GenerateAsync_SolutionContainsBuildConfigurationsForEveryProject()
    {
        using var workspace = new TestWorkspace();
        var generator = new IncrementalGenerator(new TransactionalGenerationOutput(), new ThrowingProcessRunner());

        var result = await generator.GenerateAsync(
            new GenerationRequest(GenerationKind.Solution, "OrderDesk", workspace.Root),
            CancellationToken.None);

        var solution = File.ReadAllText(Path.Combine(result.TargetPath, "OrderDesk.sln"));
        Assert.Equal(3, solution.Split(".Release|Any CPU.Build.0", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task GenerateAsync_AppPinsPatchedDevelopmentTooling()
    {
        using var workspace = new TestWorkspace();
        var generator = new IncrementalGenerator(new TransactionalGenerationOutput(), new ThrowingProcessRunner());

        var result = await generator.GenerateAsync(
            new GenerationRequest(GenerationKind.App, "OrderDesk", workspace.Root),
            CancellationToken.None);

        var packageJson = File.ReadAllText(Path.Combine(result.TargetPath, "package.json"));
        Assert.Contains("\"@hono/node-server\": \"^2.0.12\"", packageJson, StringComparison.Ordinal);
        Assert.Contains("\"@modelcontextprotocol/sdk\": \"^1.30.0\"", packageJson, StringComparison.Ordinal);
    }

    private sealed class ThrowingProcessRunner : IProcessRunner
    {
        public bool WasCalled { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            throw new InvalidOperationException("Packaged generation must not invoke an external tool.");
        }
    }
}
