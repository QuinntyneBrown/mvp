using Mvp.Cli.Bootstrap;
using Mvp.Cli.Infrastructure.Output;

namespace Mvp.Cli.Tests.Unit;

// Acceptance Test
// Traces to: L2-062, L2-066, L2-068
// Description: Staging, conflicts, overwrite rollback, and cancellation are failure-safe.
public sealed class TransactionalGenerationOutputTests
{
    [Fact]
    public async Task CommitAsync_CreatesMissingParentsAndPublishesCompleteTree()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("missing", "parent", "OrderDesk");

        var result = await new TransactionalGenerationOutput().CommitAsync(
            target,
            false,
            (stage, token) => SafeFileWriter.WriteAsync(stage, "src/file.txt", "complete", token),
            CancellationToken.None);

        Assert.Equal("complete", File.ReadAllText(Path.Combine(target, "src", "file.txt")));
        Assert.Single(result.Artifacts);
    }

    [Fact]
    public async Task CommitAsync_RejectsExistingTargetWithoutForce()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("OrderDesk");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "user.txt"), "keep");

        await Assert.ThrowsAsync<GenerationConflictException>(() =>
            new TransactionalGenerationOutput().CommitAsync(target, false, (_, _) => Task.CompletedTask, CancellationToken.None));

        Assert.Equal("keep", File.ReadAllText(Path.Combine(target, "user.txt")));
    }

    [Fact]
    public async Task CommitAsync_RejectsExistingFileWithoutForce()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("OrderDesk");
        File.WriteAllText(target, "keep");

        await Assert.ThrowsAsync<GenerationConflictException>(() =>
            new TransactionalGenerationOutput().CommitAsync(target, false, (_, _) => Task.CompletedTask, CancellationToken.None));

        Assert.Equal("keep", File.ReadAllText(target));
    }

    [Fact]
    public async Task CommitAsync_ForceReplacesExistingTargetAfterCompleteRender()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("OrderDesk");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "user.txt"), "replace");

        var result = await new TransactionalGenerationOutput().CommitAsync(
            target,
            true,
            (stage, token) => SafeFileWriter.WriteAsync(stage, "new.txt", "complete", token),
            CancellationToken.None);

        Assert.True(result.ReplacedExistingOutput);
        Assert.False(File.Exists(Path.Combine(target, "user.txt")));
        Assert.Equal("complete", File.ReadAllText(Path.Combine(target, "new.txt")));
    }

    [Fact]
    public async Task CommitAsync_ForceCanReplaceAnExistingFileTarget()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("OrderDesk");
        File.WriteAllText(target, "replace");

        var result = await new TransactionalGenerationOutput().CommitAsync(
            target,
            true,
            (stage, token) => SafeFileWriter.WriteAsync(stage, "new.txt", "complete", token),
            CancellationToken.None);

        Assert.True(result.ReplacedExistingOutput);
        Assert.True(Directory.Exists(target));
        Assert.Equal("complete", File.ReadAllText(Path.Combine(target, "new.txt")));
    }

    [Fact]
    public async Task CommitAsync_RestoresOriginalWhenPublishFails()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("OrderDesk");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "user.txt"), "keep");

        await Assert.ThrowsAsync<GenerationException>(() =>
            new TransactionalGenerationOutput().CommitAsync(
                target,
                true,
                async (stage, token) =>
                {
                    await SafeFileWriter.WriteAsync(stage, "new.txt", "partial", token);
                    throw new InvalidOperationException("planned failure");
                },
                CancellationToken.None));

        Assert.Equal("keep", File.ReadAllText(Path.Combine(target, "user.txt")));
        Assert.DoesNotContain(Directory.EnumerateDirectories(workspace.Root), path => path.Contains("mvp-stage", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CommitAsync_CancellationLeavesNoTargetOrStage()
    {
        using var workspace = new TestWorkspace();
        var target = workspace.PathFor("OrderDesk");
        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            new TransactionalGenerationOutput().CommitAsync(
                target,
                false,
                async (stage, token) =>
                {
                    await SafeFileWriter.WriteAsync(stage, "partial.txt", "partial", token);
                    await cancellation.CancelAsync();
                    token.ThrowIfCancellationRequested();
                },
                cancellation.Token));

        Assert.False(Directory.Exists(target));
        Assert.DoesNotContain(Directory.EnumerateDirectories(workspace.Root), path => path.Contains("mvp-stage", StringComparison.Ordinal));
    }
}
