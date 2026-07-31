using Mvp.Core.Errors;
using Mvp.Core.Features.Generation;

namespace Mvp.Core.Infrastructure.Output;

public sealed class TransactionalGenerationOutput : ITransactionalGenerationOutput
{
    public async Task<GenerationResult> CommitAsync(
        string targetPath,
        bool force,
        Func<string, CancellationToken, Task> generateInStagingDirectory,
        CancellationToken cancellationToken)
    {
        var target = Path.GetFullPath(targetPath);
        var parent = Path.GetDirectoryName(target)
            ?? throw new GenerationException("The target path must have a parent directory.");
        var targetName = Path.GetFileName(target);
        var stage = Path.Combine(parent, $".{targetName}.mvp-stage-{Guid.NewGuid():N}");
        var backup = Path.Combine(parent, $".{targetName}.mvp-backup-{Guid.NewGuid():N}");
        var replaced = false;
        var warnings = new List<string>();

        Directory.CreateDirectory(parent);
        if (PathExists(target) && !force)
        {
            throw new GenerationConflictException(
                $"The target '{target}' already exists. Re-run with --force to replace it safely.");
        }

        try
        {
            Directory.CreateDirectory(stage);
            await generateInStagingDirectory(stage, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var artifacts = Directory.EnumerateFiles(stage, "*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(stage, file).Replace(Path.DirectorySeparatorChar, '/'))
                .Order(StringComparer.Ordinal)
                .ToArray();

            if (PathExists(target))
            {
                Move(target, backup);
                replaced = true;
            }

            try
            {
                Directory.Move(stage, target);
            }
            catch
            {
                if (PathExists(backup) && !PathExists(target))
                {
                    Move(backup, target);
                }

                throw;
            }

            if (PathExists(backup))
            {
                try
                {
                    Delete(backup);
                }
                catch (Exception exception)
                {
                    warnings.Add($"Generation succeeded, but backup cleanup failed: {exception.Message}");
                }
            }

            return new GenerationResult(target, artifacts, replaced, warnings);
        }
        catch (Exception exception) when (exception is not GenerationException and not OperationCanceledException)
        {
            throw new GenerationException($"Generation could not be committed to '{target}'.", exception);
        }
        finally
        {
            if (Directory.Exists(stage))
            {
                try
                {
                    Directory.Delete(stage, recursive: true);
                }
                catch (IOException)
                {
                    // The original failure is more actionable; a unique private stage cannot be mistaken for success.
                }
                catch (UnauthorizedAccessException)
                {
                    // The original failure is more actionable; a unique private stage cannot be mistaken for success.
                }
            }
        }
    }

    private static bool PathExists(string path) => Directory.Exists(path) || File.Exists(path);

    private static void Move(string source, string destination)
    {
        if (Directory.Exists(source))
        {
            Directory.Move(source, destination);
        }
        else
        {
            File.Move(source, destination);
        }
    }

    private static void Delete(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
        else
        {
            File.Delete(path);
        }
    }
}
