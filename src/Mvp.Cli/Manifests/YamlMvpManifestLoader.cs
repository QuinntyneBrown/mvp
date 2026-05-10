using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mvp.Cli.Manifests;

public class YamlMvpManifestLoader : IMvpManifestLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public MvpManifest Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Manifest file not found: {path}", path);
        }

        var yaml = File.ReadAllText(path);
        var manifest = Deserializer.Deserialize<MvpManifest>(yaml)
            ?? throw new InvalidOperationException($"Manifest at '{path}' is empty or invalid.");

        if (string.IsNullOrWhiteSpace(manifest.Name))
        {
            throw new InvalidOperationException("Manifest must specify a non-empty 'name'.");
        }

        return manifest;
    }
}
