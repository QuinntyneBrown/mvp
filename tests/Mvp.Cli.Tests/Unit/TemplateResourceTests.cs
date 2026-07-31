using System.Reflection;
using Mvp.Cli.Features.FullStack.Generation;
using Mvp.Cli.Features.Incremental;

namespace Mvp.Cli.Tests.Unit;

// Acceptance Test
// Traces to: L2-016, L2-058, L2-063
// Description: Every packaged template ships in the generator assembly and resolves by its manifest name.
public sealed class TemplateResourceTests
{
    private const int ExpectedFullStackResources = 92;
    private const int ExpectedIncrementalResources = 20;

    // Derived from the assembly rather than hard-coded, so this suite keeps its meaning if the
    // templates are ever repackaged under a different root namespace or assembly.
    private static readonly Assembly GeneratorAssembly = typeof(FullStackGenerator).Assembly;

    private static string TemplateRoot
    {
        get
        {
            const string Anchor = ".Templates.FullStack.manifest.txt";
            var manifest = Assert.Single(
                GeneratorAssembly.GetManifestResourceNames(),
                name => name.EndsWith(Anchor, StringComparison.Ordinal));
            return manifest[..^Anchor.Length] + ".Templates.";
        }
    }

    [Fact]
    public void GeneratorAssemblyCarriesEveryPackagedTemplate()
    {
        Assert.Same(GeneratorAssembly, typeof(IncrementalGenerator).Assembly);

        var names = GeneratorAssembly.GetManifestResourceNames();

        Assert.All(names, name => Assert.StartsWith(TemplateRoot, name, StringComparison.Ordinal));
        Assert.Equal(
            ExpectedFullStackResources,
            names.Count(name => name.StartsWith(TemplateRoot + "FullStack.", StringComparison.Ordinal)));
        Assert.Equal(
            ExpectedIncrementalResources,
            names.Count(name => name.StartsWith(TemplateRoot + "Incremental.", StringComparison.Ordinal)));
        Assert.Equal(ExpectedFullStackResources + ExpectedIncrementalResources, names.Length);
    }

    [Theory]
    [InlineData(typeof(FullStackGenerator), "FullStack.")]
    [InlineData(typeof(IncrementalGenerator), "Incremental.")]
    public void GeneratorResourcePrefixMatchesThePackagedResourceNames(Type generator, string folder)
    {
        // The generators address templates through a private prefix. Resource names derive from
        // RootNamespace, the prefix does not, so nothing but this assertion keeps the two in step —
        // and a mismatch is invisible at compile time, surfacing only as a runtime GenerationException.
        var field = generator.GetField("ResourcePrefix", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.True(field is not null, $"{generator.Name} no longer declares a static ResourcePrefix.");

        var declared = (field!.IsLiteral ? field.GetRawConstantValue() : field.GetValue(null)) as string;

        Assert.Equal(TemplateRoot + folder, declared);
    }

    [Fact]
    public void EveryFullStackManifestEntryResolvesToAnEmbeddedResource()
    {
        var prefix = TemplateRoot + "FullStack.";
        var entries = ReadManifestEntries(prefix);

        Assert.NotEmpty(entries);
        foreach (var (relativePath, resourceName) in entries)
        {
            // Mirrors FullStackGenerator.ReadResource so a prefix change fails here first.
            var fullName = resourceName.StartsWith(prefix, StringComparison.Ordinal)
                ? resourceName
                : prefix + resourceName;

            using var stream = GeneratorAssembly.GetManifestResourceStream(fullName);
            Assert.True(stream is not null, $"Manifest entry '{relativePath}' names missing resource '{fullName}'.");
        }
    }

    private static List<(string RelativePath, string ResourceName)> ReadManifestEntries(string prefix)
    {
        using var stream = GeneratorAssembly.GetManifestResourceStream(prefix + "manifest.txt");
        Assert.True(stream is not null, "The packaged full-stack template manifest was not found.");

        using var reader = new StreamReader(stream!);
        var entries = new List<(string, string)>();
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separator = trimmed.IndexOf('=', StringComparison.Ordinal);
            Assert.True(separator > 0, $"Invalid packaged template manifest entry: '{trimmed}'.");
            entries.Add((trimmed[..separator].Trim(), trimmed[(separator + 1)..].Trim()));
        }

        return entries;
    }
}
