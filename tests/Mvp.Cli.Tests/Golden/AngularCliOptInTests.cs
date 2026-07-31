using Mvp.Cli.Features.Generation;
using Mvp.Cli.Features.Incremental;
using Mvp.Cli.Infrastructure.Output;
using Mvp.Cli.Infrastructure.Processes;

namespace Mvp.Cli.Tests.Golden;

// Acceptance Test
// Traces to: L2-015, L2-024, L2-063
// Description: Angular CLI is an explicit opt-in and receives tokenized arguments.
public sealed class AngularCliOptInTests
{
    [Fact]
    public async Task GenerateAsync_UseAngularCliInvokesTokenizedProcessRequest()
    {
        using var workspace = new TestWorkspace();
        var process = new RecordingProcessRunner();
        var generator = new IncrementalGenerator(new TransactionalGenerationOutput(), process);

        await generator.GenerateAsync(
            new GenerationRequest(GenerationKind.App, "OrderDesk", workspace.Root, UseAngularCli: true),
            CancellationToken.None);

        Assert.NotNull(process.Request);
        Assert.Equal("new", process.Request.Arguments[0]);
        Assert.Equal("order-desk-app", process.Request.Arguments[1]);
        Assert.Contains("--directory", process.Request.Arguments);
    }

    [Fact]
    public async Task GenerateAsync_UnavailableAngularCliBecomesExternalToolFailure()
    {
        using var workspace = new TestWorkspace();
        var generator = new IncrementalGenerator(new TransactionalGenerationOutput(), new UnavailableProcessRunner());

        await Assert.ThrowsAsync<Mvp.Cli.Bootstrap.ExternalToolException>(() => generator.GenerateAsync(
            new GenerationRequest(GenerationKind.App, "OrderDesk", workspace.Root, UseAngularCli: true),
            CancellationToken.None));
    }

    private sealed class RecordingProcessRunner : IProcessRunner
    {
        public ProcessRequest? Request { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty));
        }
    }

    private sealed class UnavailableProcessRunner : IProcessRunner
    {
        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("not installed");
    }
}
