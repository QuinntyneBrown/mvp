using System.Text;
using Mvp.Cli.Bootstrap;

namespace Mvp.Cli.Infrastructure.Output;

public static class SafeFileWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public static async Task WriteAsync(
        string root,
        string relativePath,
        string content,
        CancellationToken cancellationToken)
    {
        var normalizedRoot = Path.GetFullPath(root);
        var destination = Path.GetFullPath(
            Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var prefix = normalizedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!destination.StartsWith(prefix, comparison))
        {
            throw new GenerationException($"Template output path '{relativePath}' escapes the target directory.");
        }

        var directory = Path.GetDirectoryName(destination);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        var normalizedContent = content.Replace("\r\n", "\n", StringComparison.Ordinal);
        await File.WriteAllTextAsync(destination, normalizedContent, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }
}
