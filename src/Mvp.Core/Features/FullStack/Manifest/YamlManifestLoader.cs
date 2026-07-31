using Mvp.Core.Errors;
using Mvp.Core.Manifests;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mvp.Core.Features.FullStack.Manifest;

public sealed class YamlManifestLoader : IManifestLoader
{
    public const long MaximumManifestBytes = 1024 * 1024;

    public ManifestLoadResult Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Manifest file '{path}' was not found.", path);
        }

        var file = new FileInfo(path);
        if (file.Length > MaximumManifestBytes)
        {
            throw new ManifestValidationException(
                [$"Manifest exceeds the {MaximumManifestBytes / 1024} KiB size limit."]);
        }

        string yaml;
        try
        {
            yaml = File.ReadAllText(path);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw new ManifestValidationException([$"Manifest could not be read: {exception.Message}"]);
        }

        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new ManifestValidationException(["Manifest is empty."]);
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var manifest = deserializer.Deserialize<MvpManifest>(yaml)
                ?? throw new ManifestValidationException(["Manifest is empty or invalid."]);
            return new ManifestLoadResult(manifest, FindUnknownFields(yaml));
        }
        catch (ManifestValidationException)
        {
            throw;
        }
        catch (YamlException exception)
        {
            throw new ManifestValidationException([$"Manifest YAML is invalid: {exception.Message}"]);
        }
    }

    private static List<string> FindUnknownFields(string yaml)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root)
        {
            return [];
        }

        var warnings = new List<string>();
        InspectMapping(root, ["name", "output", "entities", "pages", "components"], "manifest", warnings);
        InspectSequenceMappings(root, "entities", ["name", "properties"], "entity", warnings, mapping =>
            InspectSequenceMappings(mapping, "properties", ["name", "type"], "property", warnings));
        InspectSequenceMappings(root, "pages", ["name", "route", "requiresAuth"], "page", warnings);
        InspectSequenceMappings(root, "components", ["name", "library"], "component", warnings);
        return warnings;
    }

    private static void InspectSequenceMappings(
        YamlMappingNode parent,
        string key,
        IReadOnlyCollection<string> allowed,
        string location,
        List<string> warnings,
        Action<YamlMappingNode>? inspectChild = null)
    {
        if (!parent.Children.TryGetValue(new YamlScalarNode(key), out var node) || node is not YamlSequenceNode sequence)
        {
            return;
        }

        foreach (var mapping in sequence.Children.OfType<YamlMappingNode>())
        {
            InspectMapping(mapping, allowed, location, warnings);
            inspectChild?.Invoke(mapping);
        }
    }

    private static void InspectMapping(
        YamlMappingNode mapping,
        IReadOnlyCollection<string> allowed,
        string location,
        List<string> warnings)
    {
        foreach (var key in mapping.Children.Keys.OfType<YamlScalarNode>())
        {
            if (key.Value is not null && !allowed.Contains(key.Value, StringComparer.Ordinal))
            {
                warnings.Add($"Unknown {location} field '{key.Value}' was ignored.");
            }
        }
    }
}
