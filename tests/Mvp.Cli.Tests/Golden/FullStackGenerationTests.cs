using Mvp.Core.Features.FullStack.Generation;
using Mvp.Core.Features.FullStack.Manifest;
using Mvp.Core.Infrastructure.Output;

namespace Mvp.Cli.Tests.Golden;

// Acceptance Test
// Traces to: L2-009, L2-010, L2-011, L2-016, L2-017, L2-018, L2-019, L2-020, L2-023, L2-024, L2-025, L2-027, L2-028, L2-029, L2-030, L2-031, L2-032, L2-034, L2-035, L2-038, L2-044, L2-045, L2-046, L2-048, L2-050, L2-054, L2-055, L2-067, L2-069, L2-070
// Description: Representative manifests render deterministic, versioned backend/frontend trees.
public sealed class FullStackGenerationTests
{
    [Fact]
    public async Task GenerateAsync_RendersRepresentativeManifestAndComponents()
    {
        using var workspace = new TestWorkspace();
        var manifest = RepresentativeManifest(workspace.Root);

        var result = await new FullStackGenerator(new TransactionalGenerationOutput())
            .GenerateAsync(manifest, false, CancellationToken.None);

        Assert.True(result.Artifacts.Count > 90);
        Assert.Contains("backend/src/OrderDesk.Domain/Order.cs", result.Artifacts);
        Assert.Contains("frontend/projects/order-desk-app/src/app/pages/order-history/order-history.page.ts", result.Artifacts);
        Assert.Contains("frontend/projects/components/src/lib/order-card/order-card.component.ts", result.Artifacts);
        Assert.Contains("net10.0", File.ReadAllText(Path.Combine(result.TargetPath, "backend", "Directory.Build.props")), StringComparison.Ordinal);
        var packageJson = File.ReadAllText(Path.Combine(result.TargetPath, "frontend", "package.json"));
        Assert.Contains("^22.1.0", packageJson, StringComparison.Ordinal);
        Assert.Contains("\"@modelcontextprotocol/sdk\": \"^1.30.0\"", packageJson, StringComparison.Ordinal);
        Assert.Contains(
            "private const int WorkFactor = 12",
            File.ReadAllText(Path.Combine(result.TargetPath, "backend", "src", "OrderDesk.Infrastructure", "BCryptPasswordHasher.cs")),
            StringComparison.Ordinal);
        Assert.Contains(
            "Traces to: L2-027",
            File.ReadAllText(Path.Combine(result.TargetPath, "frontend", "e2e", "tests", "auth.spec.ts")),
            StringComparison.Ordinal);
        Assert.Contains(
            ".NET 10",
            File.ReadAllText(Path.Combine(result.TargetPath, "README.md")),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_IsDeterministicForTheSameManifest()
    {
        using var first = new TestWorkspace();
        using var second = new TestWorkspace();
        var generator = new FullStackGenerator(new TransactionalGenerationOutput());

        var firstResult = await generator.GenerateAsync(RepresentativeManifest(first.Root), false, CancellationToken.None);
        var secondResult = await generator.GenerateAsync(RepresentativeManifest(second.Root), false, CancellationToken.None);

        Assert.Equal(firstResult.Artifacts, secondResult.Artifacts);
        foreach (var artifact in firstResult.Artifacts)
        {
            Assert.Equal(
                File.ReadAllBytes(Path.Combine(firstResult.TargetPath, artifact.Replace('/', Path.DirectorySeparatorChar))),
                File.ReadAllBytes(Path.Combine(secondResult.TargetPath, artifact.Replace('/', Path.DirectorySeparatorChar))));
        }
    }

    private static ValidatedManifest RepresentativeManifest(string output) => new(
        "OrderDesk",
        output,
        [new ValidatedEntity("Order", [new ValidatedProperty("Total", "decimal")])],
        [new ValidatedPage("OrderHistory", "order-history", true)],
        [new ValidatedComponent("OrderCard", FrontendLibrary.Components)],
        []);
}
