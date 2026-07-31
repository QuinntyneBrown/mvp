using Mvp.Cli.Bootstrap;
using Mvp.Cli.Features.FullStack.Manifest;

namespace Mvp.Cli.Tests.Unit;

// Acceptance Test
// Traces to: L2-007, L2-012, L2-014, L2-015
// Description: YAML remains bounded data and unknown fields produce warnings.
public sealed class YamlManifestLoaderTests
{
    [Fact]
    public void Load_BindsCamelCaseAndWarnsForUnknownFields()
    {
        using var workspace = new TestWorkspace();
        var path = workspace.PathFor("mvp.yaml");
        File.WriteAllText(path, "name: OrderDesk\nfutureField: true\npages:\n  - name: HomePage\n    requiresAuth: false\n");

        var result = new YamlManifestLoader().Load(path);

        Assert.Equal("OrderDesk", result.Manifest.Name);
        Assert.False(result.Manifest.Pages.Single().RequiresAuth);
        Assert.Contains(result.Warnings, warning => warning.Contains("futureField", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_RejectsOversizedManifest()
    {
        using var workspace = new TestWorkspace();
        var path = workspace.PathFor("large.yaml");
        File.WriteAllText(path, "name: OrderDesk\n#" + new string('x', (int)YamlManifestLoader.MaximumManifestBytes));

        Assert.Throws<ManifestValidationException>(() => new YamlManifestLoader().Load(path));
    }

    [Fact]
    public void Load_RejectsYamlTypeTags()
    {
        using var workspace = new TestWorkspace();
        var path = workspace.PathFor("tagged.yaml");
        File.WriteAllText(path, "!System.IO.FileInfo,%20mscorlib 'secret.txt'");

        Assert.Throws<ManifestValidationException>(() => new YamlManifestLoader().Load(path));
    }
}
