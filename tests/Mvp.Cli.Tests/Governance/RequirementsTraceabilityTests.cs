using System.Text.RegularExpressions;

namespace Mvp.Cli.Tests.Governance;

// Acceptance Test
// Traces to: L2-047, L2-069
// Description: Implemented requirements cannot drift away from named automated evidence or documentation.
public sealed partial class RequirementsTraceabilityTests
{
    [Fact]
    public void AllImplementedRequirementsHaveNamedEvidence()
    {
        var repositoryRoot = FindRepositoryRoot();
        var specification = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "specs", "L2.md"));
        var traceability = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "requirements-traceability.md"));
        var implemented = ImplementedRequirementPattern().Matches(specification)
            .Select(match => match.Groups[1].Value)
            .ToArray();

        Assert.NotEmpty(implemented);
        foreach (var requirement in implemented)
        {
            Assert.Contains(requirement, traceability, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AcceptanceTestFilesDeclareRequirementTraceability()
    {
        var repositoryRoot = FindRepositoryRoot();
        var testRoot = Path.Combine(repositoryRoot, "tests", "Mvp.Cli.Tests");
        var acceptanceTests = Directory.EnumerateFiles(testRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("[Fact]", StringComparison.Ordinal)
                || File.ReadAllText(path).Contains("[Theory]", StringComparison.Ordinal));

        foreach (var path in acceptanceTests)
        {
            Assert.Contains("// Traces to: L2-", File.ReadAllText(path), StringComparison.Ordinal);
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

    [GeneratedRegex(@"(?ms)^## (L2-\d{3}):(?:(?!^## L2-).)*?^\*\*Status:\*\* Implemented$")]
    private static partial Regex ImplementedRequirementPattern();
}
