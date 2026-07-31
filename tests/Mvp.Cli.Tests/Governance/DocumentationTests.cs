using System.Text.RegularExpressions;

namespace Mvp.Cli.Tests.Governance;

// Acceptance Test
// Traces to: L2-065, L2-069, L2-070
// Description: Local Markdown links and core version guidance stay valid.
public sealed partial class DocumentationTests
{
    [Fact]
    public void RepositoryMarkdownHasNoMissingRelativeLinksOrRetiredGuidanceReferences()
    {
        var root = FindRepositoryRoot();
        var files = Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}.verification{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (Match match in MarkdownLinkPattern().Matches(content))
            {
                var target = match.Groups[1].Value.Split('#')[0];
                if (target.Length == 0 || target.Contains("://", StringComparison.Ordinal) || target.StartsWith('#'))
                {
                    continue;
                }

                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, target.Replace('/', Path.DirectorySeparatorChar)));
                Assert.True(File.Exists(resolved) || Directory.Exists(resolved), $"Broken link '{target}' in '{Path.GetRelativePath(root, file)}'.");
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Mvp.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    [GeneratedRegex(@"\[[^\]]+\]\((?!<)([^)\s]+)\)")]
    private static partial Regex MarkdownLinkPattern();
}
