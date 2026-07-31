using System.Diagnostics;
using Mvp.Cli.Features.FullStack.Generation;
using Mvp.Cli.Features.FullStack.Manifest;
using Mvp.Cli.Infrastructure.Output;

namespace Mvp.Cli.Tests.Golden;

// Acceptance Test
// Traces to: L2-057, L2-058
// Description: Offline template rendering stays within the interactive time and memory budgets.
public sealed class PerformanceGenerationTests
{
    [Fact]
    public async Task GenerateAsync_RepresentativeMaximumStaysWithinBudgets()
    {
        using var workspace = new TestWorkspace();
        var entities = Enumerable.Range(1, 5)
            .Select(index => new ValidatedEntity($"Entity{index}", [new ValidatedProperty("Title", "string")]))
            .ToArray();
        var pages = Enumerable.Range(1, 5)
            .Select(index => new ValidatedPage($"Page{index}", $"page-{index}", true))
            .ToArray();
        var components = Enumerable.Range(1, 5)
            .Select(index => new ValidatedComponent($"Component{index}", FrontendLibrary.Components))
            .ToArray();
        var manifest = new ValidatedManifest("BudgetCheck", workspace.Root, entities, pages, components, []);
        var stopwatch = Stopwatch.StartNew();

        await new FullStackGenerator(new TransactionalGenerationOutput())
            .GenerateAsync(manifest, false, CancellationToken.None);

        stopwatch.Stop();
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(30), $"Generation took {stopwatch.Elapsed}.");
        Assert.True(Process.GetCurrentProcess().PeakWorkingSet64 < 512L * 1024 * 1024, "Peak working set exceeded 512 MiB.");
    }

    [Fact]
    public async Task GenerateAsync_NameOnlyStaysWithinTenSeconds()
    {
        using var workspace = new TestWorkspace();
        var manifest = new ValidatedManifest("EmptyBaseline", workspace.Root, [], [], [], []);
        var stopwatch = Stopwatch.StartNew();

        await new FullStackGenerator(new TransactionalGenerationOutput())
            .GenerateAsync(manifest, false, CancellationToken.None);

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"Generation took {stopwatch.Elapsed}.");
    }
}
